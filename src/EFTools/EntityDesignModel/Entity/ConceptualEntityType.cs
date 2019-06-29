// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml.Linq;

    internal class ConceptualEntityType : EntityType
    {
        internal static readonly string AttributeBaseType = "BaseType";
        internal static readonly string AttributeAbstract = "Abstract";

        private DefaultableValue<string> _typeAccessAttr;
        private EntityTypeBaseType _baseTypeBinding;
        private DefaultableValue<bool> _abstractAttr;
        private readonly List<NavigationProperty> _navigationProperties = new List<NavigationProperty>();

        internal ConceptualEntityType(ConceptualEntityModel model, XElement element)
            : base(model, element)
        {
        }

        /// <summary>
        ///     Returns all properties in the inheritance tree
        ///     Throws if any base types of the 'starting' entity type are unresolved or undefined
        /// </summary>
        internal IEnumerable<Property> SafePropertiesInInheritanceHierarchy
        {
            get
            {
                return (from property in SafeInheritedAndDeclaredProperties.Concat(SafeDescendantProperties)
                        select property).Reverse();
            }
        }

        /// <summary>
        ///     Recursively find all derived types of an entity type. This keeps track of the derived types it has found
        ///     so that if we find one that equals one we've found already it throws a cyclic inheritance error. We use a hashset
        ///     here because Contains() will be a O(1) operation.
        /// </summary>
        /// <param name="existingDerivedTypes"></param>
        private void GetSafeDescendantTypesHelper(ref HashSet<ConceptualEntityType> existingDerivedTypes)
        {
            // TODO: later on we can convert this method to a tree-based look-ahead to trim the O(n) space we use. 
            var antiDeps = GetAntiDependenciesOfType<ConceptualEntityType>();

            foreach (var et in antiDeps)
            {
                if (et != null)
                {
                    Debug.Assert(
                        et.BaseType.Target == this, "Why isn't this conceptual entity type the base type of the found derived type?");

                    // if we've already found the entity type during our previous traversal, throw an error
                    if (existingDerivedTypes.Contains(et))
                    {
                        ModelHelper.InvalidSchemaError(Resources.CyclicInheritanceHierarchy, et.NormalizedNameExternal);
                    }
                    else
                    {
                        // add the entity type and its derived types
                        var cet = et;
                        Debug.Assert(cet != null, "entity type wasn't a ConceptualEntityType!");
                        existingDerivedTypes.Add(cet);
                        cet.GetSafeDescendantTypesHelper(ref existingDerivedTypes);
                    }
                }
            }
        }

        /// <summary>
        ///     Returns derived types by traversing inheritance hierarchy top down.
        ///     Throws if it discovers it is in a cyclic inheritance hierarchy.
        /// </summary>
        internal IEnumerable<ConceptualEntityType> SafeDescendantTypes
        {
            get
            {
                if (!EntityModel.IsCSDL)
                {
                    throw new InvalidOperationException();
                }

                var existingDerivedTypes = new HashSet<ConceptualEntityType>();
                GetSafeDescendantTypesHelper(ref existingDerivedTypes);

                return existingDerivedTypes.AsEnumerable();
            }
        }

        /// <summary>
        ///     Returns properties from derived entity types.
        ///     Throws if cycles are encountered.
        /// </summary>
        internal IEnumerable<Property> SafeDescendantProperties
        {
            get
            {
                return (from type in SafeDescendantTypes
                        from property in type.Properties()
                        select property).Reverse();
            }
        }

        internal ConceptualEntityType LowestDerivedTypeOrSelf
        {
            get { return SafeDescendantTypes.LastOrDefault() ?? this; }
        }

        /// <summary>
        ///     Gets all types within the inheritance tree, including self and siblings.
        ///     The way this works is by traversing down to the lowest-derived type in the inheritance graph
        ///     and then working upwards to the root type - using BaseType and then traversing down through
        ///     the subtree using GetAntiDependencies, but short-circuiting root nodes of subtrees that we've
        ///     already visited. This removes redundancy and optimizes for perf.
        /// </summary>
        internal IEnumerable<EntityType> SafeTypesInInheritanceTree
        {
            get
            {
                var safeTypesInInheritanceTree = new HashSet<ConceptualEntityType>();

                var currentType = LowestDerivedTypeOrSelf;
                while (currentType != null)
                {
                    if (safeTypesInInheritanceTree.Contains(currentType))
                    {
                        ModelHelper.InvalidSchemaError(Resources.CyclicInheritanceHierarchy, currentType.NormalizedNameExternal);
                    }

                    safeTypesInInheritanceTree.Add(currentType);
                    var baseType = currentType.BaseType.Target;
                    if (baseType != null)
                    {
                        // For each of the direct derived types of the base type, add all of the types in their subtrees
                        // but skip over the subtrees of nodes we've visited
                        var directDerivedTypesQueue = new Queue<ConceptualEntityType>();
                        foreach (var directDerivedType in baseType.ResolvableDirectDerivedTypes.Where(dt => dt != currentType))
                        {
                            directDerivedTypesQueue.Enqueue(directDerivedType);
                        }

                        while (directDerivedTypesQueue.Count > 0)
                        {
                            var directDerivedType = directDerivedTypesQueue.Dequeue();

                            if (safeTypesInInheritanceTree.Contains(directDerivedType))
                            {
                                ModelHelper.InvalidSchemaError(
                                    Resources.CyclicInheritanceHierarchy, directDerivedType.NormalizedNameExternal);
                            }

                            safeTypesInInheritanceTree.Add(directDerivedType);

                            foreach (var descendantTypeOfDirectDerivedType in directDerivedType.SafeDescendantTypes)
                            {
                                if (safeTypesInInheritanceTree.Contains(descendantTypeOfDirectDerivedType))
                                {
                                    ModelHelper.InvalidSchemaError(
                                        Resources.CyclicInheritanceHierarchy, descendantTypeOfDirectDerivedType.NormalizedNameExternal);
                                }

                                safeTypesInInheritanceTree.Add(descendantTypeOfDirectDerivedType);
                            }
                        }
                    }
                    currentType = baseType;
                }

                return safeTypesInInheritanceTree;
            }
        }

        /// <summary>
        ///     Manages the content of the TypeAccess attribute
        /// </summary>
        internal DefaultableValue<string> TypeAccess
        {
            get
            {
                if (_typeAccessAttr == null)
                {
                    _typeAccessAttr = new TypeAccessDefaultableValue(this);
                }
                return _typeAccessAttr;
            }
        }

        // we unfortunately get a warning from the compiler when we use the "base" keyword in "iterator" types generated by using the
        // "yield return" keyword.  By adding this method, I was able to get around this.  Unfortunately, I wasn't able to figure out
        // a way to implement this once and have derived classes share the implementation (since the "base" keyword is resolved at 
        // compile-time and not at runtime.
        private IEnumerable<EFObject> BaseChildren
        {
            get { return base.Children; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                foreach (var efobj in BaseChildren)
                {
                    yield return efobj;
                }

                foreach (var p in _navigationProperties)
                {
                    yield return p;
                }

                yield return BaseType;
                yield return Abstract;
                yield return TypeAccess;
            }
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeBaseType);
            s.Add(AttributeAbstract);
            s.Add(TypeAccessDefaultableValue.AttributeTypeAccess);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObjectCollection(_navigationProperties);
            ClearEFObject(_baseTypeBinding);
            ClearEFObject(_abstractAttr);
            ClearEFObject(_typeAccessAttr);
            _baseTypeBinding = null;
            _abstractAttr = null;
            _typeAccessAttr = null;

            base.PreParse();
        }

        public bool IsAbstract
        {
            get { return Abstract.Value; }
            set { Abstract.Value = value; }
        }

        internal DefaultableValue<bool> Abstract
        {
            get
            {
                if (_abstractAttr == null)
                {
                    _abstractAttr = new AbstractDefaultableValue(this);
                }
                return _abstractAttr;
            }
        }

        private class AbstractDefaultableValue : DefaultableValue<bool>
        {
            internal AbstractDefaultableValue(EFElement parent)
                : base(parent, AttributeAbstract)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeAbstract; }
            }

            public override bool DefaultValue
            {
                get { return false; }
            }
        }

        internal bool IsConcrete
        {
            get { return false == IsAbstract; }
        }

        /// <summary>
        ///     A bindable reference to the EntityType that is this type's base type (will be null if no inheritance)
        /// </summary>
        internal EntityTypeBaseType BaseType
        {
            get
            {
                if (_baseTypeBinding == null)
                {
                    _baseTypeBinding = new EntityTypeBaseType(
                        this,
                        AttributeBaseType,
                        EntityTypeNameNormalizer.NameNormalizer);
                }

                return _baseTypeBinding;
            }
        }

        /// <summary>
        ///     Return true if the input is the base entity type of the instance.
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        internal bool IsDerivedFrom(ConceptualEntityType baseEntityType)
        {
            if (!HasResolvableBaseType)
            {
                return false;
            }

            return ((from type in ResolvableBaseTypes
                     where type == baseEntityType
                     select type).Count<EntityType>() > 0);
        }

        internal ConceptualEntityType ResolvableTopMostBaseType
        {
            get
            {
                if (HasResolvableBaseType)
                {
                    foreach (var baseType in ResolvableBaseTypes)
                    {
                        if (baseType.HasResolvableBaseType == false)
                        {
                            return baseType;
                        }
                    }
                }

                return this;
            }
        }

        /// <summary>
        ///     Returns base type if defined and resolved, null if undefined,
        ///     and throws an exception if reference is unresolved
        /// </summary>
        internal ConceptualEntityType SafeBaseType
        {
            get
            {
                switch (BaseType.Status)
                {
                    case BindingStatus.Known:
                        var cet = BaseType.Target;
                        Debug.Assert(cet != null, "binding status is known, but Target is null or not an ConceptualEntityType");
                        return cet;
                    case BindingStatus.Undefined:
                        return null;
                    default:
                        var etbt = BaseType;
                        Debug.Assert(etbt != null, "BaseType is not an EntityTypeBaseType");
                        ModelHelper.InvalidSchemaError(Resources.UnresolvedBaseType_1, etbt.RefName, NormalizedNameExternal);
                        return null;
                }
            }
        }

        /// <summary>
        ///     Returns itself followed by base types obtained by traversing inheritance hierarchy bottom up.
        ///     Throws if any of the base type references is unresolved.
        /// </summary>
        internal IEnumerable<ConceptualEntityType> SafeSelfAndBaseTypes
        {
            get
            {
                yield return this;

                foreach (var baseType in ResolvableBaseTypes)
                {
                    yield return baseType;
                }
            }
        }

        /// <summary>
        ///     Returns itself followed by base types obtained by traversing inheritance hierarchy bottom up.
        ///     Throws if any of the base type references is unresolved.
        /// </summary>
        internal HashSet<ConceptualEntityType> GetSafeSelfAndBaseTypesAsHashSet()
        {
            var selfAndBaseTypes = new HashSet<ConceptualEntityType>();
            foreach (var val in SafeSelfAndBaseTypes)
            {
                selfAndBaseTypes.Add(val);
            }

            return selfAndBaseTypes;
        }

        /// <summary>
        ///     Returns the top-most base type of this type.
        ///     Throws if any of the base type references on the way to the root are unresolved.
        /// </summary>
        internal ConceptualEntityType SafeRootType
        {
            get { return SafeSelfAndBaseTypes.Last(); }
        }

        /// <summary>
        ///     Returns properties inherited from all base types (base-most properties first).
        ///     Throws if any of the base types are unresolved or cycles are encountered.
        /// </summary>
        internal IEnumerable<Property> SafeInheritedProperties
        {
            get
            {
                return (from type in ResolvableBaseTypes
                        from property in type.Properties()
                        select property).Reverse();
            }
        }

        /// <summary>
        ///     Returns properties inherited from all base types (base-most properties first),
        ///     followed by the declared properties of the given entity type.
        ///     Throws if any of the base types are unresolved or cycles are encountered.
        /// </summary>
        internal IEnumerable<Property> SafeInheritedAndDeclaredProperties
        {
            get { return SafeInheritedProperties.Concat(Properties()); }
        }

        internal ICollection<ConceptualEntityType> ResolvableBaseTypes
        {
            get
            {
                var baseTypes = new List<ConceptualEntityType>();

                foreach (ConceptualEntityType baseType in AncestorEntityTypes(false))
                {
                    baseTypes.Add(baseType);
                }

                return baseTypes.AsReadOnly();
            }
        }

        internal ICollection<ConceptualEntityType> ResolvableDirectDerivedTypes
        {
            get
            {
                var directDerivedTypes = new List<ConceptualEntityType>();

                foreach (var antiDep in GetAntiDependencies())
                {
                    var baseBinding = antiDep as EntityTypeBaseType;
                    if (baseBinding != null)
                    {
                        var derivedType = baseBinding.Parent as ConceptualEntityType;
                        Debug.Assert(derivedType != null, "baseBinding.Parent should be a ConceptualEntityType");

                        directDerivedTypes.Add(derivedType);
                    }
                }

                return directDerivedTypes.AsReadOnly();
            }
        }

        internal ICollection<ConceptualEntityType> ResolvableAllDerivedTypes
        {
            get
            {
                var etbt = BaseType;

                Debug.Assert(BaseType != null ? etbt != null : true, "BaseType is not an EntityTypeBaseType");
                Debug.Assert(!ModelHelper.CheckForCircularInheritance(this, etbt.Target), "Circular inheritance detected");

                var directDerivedTypes = ResolvableDirectDerivedTypes;
                var allDerivedTypes = new List<ConceptualEntityType>(directDerivedTypes);

                foreach (var entityType in directDerivedTypes)
                {
                    allDerivedTypes.AddRange(entityType.ResolvableAllDerivedTypes);
                }

                return allDerivedTypes.AsReadOnly();
            }
        }

        /// <summary>
        /// </summary>
        internal bool HasResolvableBaseType
        {
            get { return BaseType.Target != null; }
        }

        internal ICollection<ConceptualEntityType> DerivedTypes
        {
            get
            {
                if (!EntityModel.IsCSDL)
                {
                    throw new InvalidOperationException();
                }

                var antiDeps = Artifact.ArtifactSet.GetAntiDependencies(this);

                var derivedTypes = new List<ConceptualEntityType>();
                foreach (var antiDep in antiDeps)
                {
                    var et = antiDep as ConceptualEntityType;
                    if (et == null
                        && antiDep.Parent != null)
                    {
                        et = antiDep.Parent as ConceptualEntityType;
                    }

                    if (et != null)
                    {
                        if (derivedTypes.Contains(et) == false)
                        {
                            derivedTypes.Add(et);
                        }
                        derivedTypes.AddRange(et.DerivedTypes);
                    }
                }

                return derivedTypes.AsReadOnly();
            }
        }

        internal override EntitySet EntitySet
        {
            get
            {
                EntityType topMostBaseType = null;
                foreach (var et in AncestorEntityTypes(true))
                {
                    topMostBaseType = et;
                }

                Debug.Assert(topMostBaseType != null, "could not find topmst base type");

                EntitySet entitySet = null;
                if (topMostBaseType != null)
                {
                    foreach (var es in topMostBaseType.GetAntiDependenciesOfType<EntitySet>())
                    {
                        entitySet = es;
                        break;
                    }
                }
                return entitySet;
            }
        }

        /// <summary>
        ///     returns an IEnumerable of all EntitySets for this EntityType, or one of this EntityType's base types
        ///     Currently, Escher only supports a single entity-set per type, so the presence of more than one of these
        ///     indicates an error condition.
        /// </summary>
        internal override IEnumerable<EntitySet> AllEntitySets
        {
            get
            {
                var entitySets = new HashSet<EntitySet>();

                foreach (var et in AncestorEntityTypes(true))
                {
                    foreach (var es in et.GetAntiDependenciesOfType<EntitySet>())
                    {
                        entitySets.Add(es);
                    }
                }

                return entitySets;
            }
        }

        internal void AddNavigationProperty(NavigationProperty navProp)
        {
            _navigationProperties.Add(navProp);
        }

        internal IEnumerable<NavigationProperty> NavigationProperties()
        {
            foreach (var np in _navigationProperties)
            {
                yield return np;
            }
        }

        internal int NavigationPropertyCount
        {
            get { return _navigationProperties.Count; }
        }

        internal NavigationProperty FindNavigationPropertyForEnd(AssociationEnd end)
        {
            foreach (var np in _navigationProperties)
            {
                if (np.FromRole.Target == end)
                {
                    return np;
                }
            }
            return null;
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            var child2 = efContainer as NavigationProperty;
            if (child2 != null)
            {
                _navigationProperties.Remove(child2);
                return;
            }

            base.OnChildDeleted(efContainer);
        }

#if DEBUG
        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(NavigationProperty.ElementName);
            return s;
        }
#endif

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == Property.ElementName)
            {
                Property prop = null;

                var conceptualModel = (ConceptualEntityModel)GetParentOfType(typeof(ConceptualEntityModel));
                Debug.Assert(conceptualModel != null, "ParseSingleElement: Unable to find parent of type ConceptualEntityModel");

                if (conceptualModel != null
                    && ModelHelper.IsElementComplexProperty(elem, conceptualModel))
                {
                    prop = new ComplexConceptualProperty(this, elem);
                }
                else
                {
                    prop = new ConceptualProperty(this, elem);
                }

                prop.Parse(unprocessedElements);
                AddProperty(prop);
            }
            else if (elem.Name.LocalName == NavigationProperty.ElementName)
            {
                var prop = new NavigationProperty(this, elem);
                prop.Parse(unprocessedElements);
                AddNavigationProperty(prop);
            }
            else
            {
                return base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            // first, try to resolve the base type if there is one
            if (BaseType.RefName == null)
            {
                // rebind so we can re-set any pre-bound things
                BaseType.Rebind();
                base.DoResolve(artifactSet);
            }
            else
            {
                BaseType.Rebind();
                if (BaseType.Status == BindingStatus.Known)
                {
                    State = EFElementState.Resolved;
                }
            }
        }

        private IEnumerable<EntityType> AncestorEntityTypes(bool includeSelf)
        {
            var visitedAncestors = new Dictionary<ConceptualEntityType, object>();

            var topMostBaseType = this;
            visitedAncestors.Add(topMostBaseType, null);
            if (includeSelf)
            {
                yield return topMostBaseType;
            }

            while (topMostBaseType.HasResolvableBaseType
                   && !visitedAncestors.ContainsKey((topMostBaseType = topMostBaseType.BaseType.Target)))
            {
                visitedAncestors.Add(topMostBaseType, null);
                yield return topMostBaseType;
            }
        }
    }
}
