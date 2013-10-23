// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class EntityTypeMapping : EFElement
    {
        internal static readonly string ElementName = "EntityTypeMapping";
        internal static readonly string AttributeTypeName = "TypeName";
        internal static readonly string IsTypeOfFormat = "IsTypeOf({0})";
        internal static readonly string IsTypeOf = "IsTypeOf(";
        internal static readonly string IsTypeOfTerminal = ")";

        private readonly List<MappingFragment> _fragments = new List<MappingFragment>();
        private ModificationFunctionMapping _modificationFunctionMapping;
        private EntityTypeMappingTypeNameBinding _entityTypes;
        private EntityTypeMappingKind _kind;

        internal static bool UsesIsTypeOf(string name)
        {
            if (false == string.IsNullOrWhiteSpace(name)
                && name.StartsWith(IsTypeOf, StringComparison.OrdinalIgnoreCase)
                && name.EndsWith(IsTypeOfTerminal, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        internal static string StripOffIsTypeOf(string name)
        {
            var stripped = name.Trim();

            if (UsesIsTypeOf(stripped))
            {
                stripped = stripped.Substring(IsTypeOf.Length); // remove the 'IsTypeOf('
                stripped = stripped.Substring(0, stripped.Length - IsTypeOfTerminal.Length); // remove the trailing ')'
            }

            return stripped;
        }

        internal EntityTypeMapping(EFContainer parent, XElement element)
            : base(parent, element)
        {
            Debug.Assert(parent is EntitySetMapping, "parent should be a EntitySetMapping");
            _kind = EntityTypeMappingKind.Derive;
        }

        /// <summary>
        ///     A constructor to call when we are creating a new ETM.  If you send a specific kind to this
        ///     constructor then this will be used [for deriving RefNames for instance; see EntityType.GetRefNameForBinding()]
        ///     until the item is parsed.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="element"></param>
        /// <param name="kind"></param>
        internal EntityTypeMapping(EFContainer parent, XElement element, EntityTypeMappingKind kind)
            : base(parent, element)
        {
            Debug.Assert(parent is EntitySetMapping, "parent should be a EntitySetMapping");
            _kind = kind;
        }

        internal EntitySetMapping EntitySetMapping
        {
            get
            {
                var parent = Parent as EntitySetMapping;
                Debug.Assert(parent != null, "this.Parent should be a EntitySetMapping");
                return parent;
            }
        }

        /// <summary>
        ///     A multi-item bindable reference of EntityTypes pointed to by this mapping
        /// </summary>
        internal EntityTypeMappingTypeNameBinding TypeName
        {
            get
            {
                if (_entityTypes == null)
                {
                    _entityTypes = new EntityTypeMappingTypeNameBinding(
                        this,
                        EntityTypeMappingTypeNameNormalizer.NameNormalizer);
                }
                return _entityTypes;
            }
        }

        /// <summary>
        ///     For more information on this, see the comment on the c'tor and the comment on
        ///     EntityType.GetRefNameForBinding()
        /// </summary>
        internal EntityTypeMappingKind Kind
        {
            get
            {
                if (_kind == EntityTypeMappingKind.Derive)
                {
                    if (ModificationFunctionMapping != null)
                    {
                        return EntityTypeMappingKind.Function;
                    }
                    else
                    {
                        if (TypeName.IsTypeOfs[0])
                        {
                            return EntityTypeMappingKind.IsTypeOf;
                        }
                        else
                        {
                            return EntityTypeMappingKind.Default;
                        }
                    }
                }
                else
                {
                    return _kind;
                }
            }
        }

        internal IList<MappingFragment> MappingFragments()
        {
            return _fragments.AsReadOnly();
        }

        internal void AddMappingFragment(MappingFragment fragment)
        {
            _fragments.Add(fragment);
        }

        internal ModificationFunctionMapping ModificationFunctionMapping
        {
            get { return _modificationFunctionMapping; }
            set { _modificationFunctionMapping = value; }
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
                foreach (var child in MappingFragments())
                {
                    yield return child;
                }
                if (ModificationFunctionMapping != null)
                {
                    yield return ModificationFunctionMapping;
                }
                yield return TypeName;
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            var child1 = efContainer as MappingFragment;
            if (child1 != null)
            {
                _fragments.Remove(child1);
                return;
            }

            if (efContainer == _modificationFunctionMapping)
            {
                _modificationFunctionMapping = null;
                return;
            }

            base.OnChildDeleted(efContainer);
        }

        internal ConceptualEntityType FirstBoundConceptualEntityType
        {
            get
            {
                foreach (var binding in TypeName.Bindings)
                {
                    if (binding.Status == BindingStatus.Known)
                    {
                        Debug.Assert(binding.Target is ConceptualEntityType, "EntityType is not a ConceptualEntityType");
                        return binding.Target as ConceptualEntityType;
                    }
                }

                return null;
            }
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeTypeName);
            var ghostChild = GetGhostChild();
            if (ghostChild != null)
            {
                var s2 = ghostChild.MyAttributeNames();
                foreach (var str in s2)
                {
                    s.Add(str);
                }
            }
            return s;
        }
#endif

#if DEBUG
        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(MappingFragment.ElementName);
            s.Add(ModificationFunctionMapping.ElementName);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_entityTypes);
            _entityTypes = null;

            // once we have children that we can parse, set this to Derive and figure out the kind dynamically
            _kind = EntityTypeMappingKind.Derive;

            ClearEFObjectCollection(_fragments);

            ClearEFObject(_modificationFunctionMapping);
            _modificationFunctionMapping = null;

            base.PreParse();
        }

        protected override void DoParse(ICollection<XName> unprocessedElements)
        {
            // call base's DoParse() first to process elements
            base.DoParse(unprocessedElements);

            // next see if we have a StoreEntitySet attribute
            if (GetAttributeValue(MappingFragment.AttributeStoreEntitySet) != null)
            {
                if (_fragments.Count == 0)
                {
                    // create a "ghost-node"
                    var frag = new MappingFragment(this, XElement);
                    _fragments.Add(frag);
                    frag.Parse(unprocessedElements);

                    // Add an error - we don't want to support this syntax in the designer.  This can be on an EntitySetMapping or an EntityTypeMapping node
                    var elementName = XElement.Name.LocalName;
                    var msg = String.Format(
                        CultureInfo.CurrentCulture, Resources.ModelParse_GhostNodeNotSupportedByDesigner,
                        MappingFragment.AttributeStoreEntitySet, elementName);
                    Artifact.AddParseErrorForObject(this, msg, ErrorCodes.ModelParse_GhostNodeNotSupportedByDesigner);
                }
                else
                {
                    // TypeName attribute and EntityTypeMapping children.  These are mutually exclusive.
                    var msg = String.Format(
                        CultureInfo.CurrentCulture, Resources.ModelParse_MutuallyExclusiveAttributeAndChildElement,
                        MappingFragment.AttributeStoreEntitySet, MappingFragment.ElementName);
                    Artifact.AddParseErrorForObject(this, msg, ErrorCodes.ModelParse_MutuallyExclusiveAttributeAndChildElement);
                }
            }
        }

        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == MappingFragment.ElementName)
            {
                var frag = new MappingFragment(this, elem);
                _fragments.Add(frag);
                frag.Parse(unprocessedElements);
            }
            else if (elem.Name.LocalName == ModificationFunctionMapping.ElementName)
            {
                _modificationFunctionMapping = new ModificationFunctionMapping(this, elem);
                _modificationFunctionMapping.Parse(unprocessedElements);
            }
            else
            {
                return base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            TypeName.Rebind();
            if (TypeName.Status == (int)BindingStatus.Known)
            {
                State = EFElementState.Resolved;
            }
        }

        private string DisplayNameInternal(bool localize)
        {
            string resource;
            if (localize)
            {
                resource = Resources.MappingModel_EntityTypeMappingDisplayName;
            }
            else
            {
                resource = "{0} (EntityType)";
            }

            if (TypeName.Status == (int)BindingStatus.Known)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    resource,
                    TypeName.TargetsListForDisplay);
            }
            return string.Format(
                CultureInfo.CurrentCulture,
                resource,
                TypeName.RefName);
        }

        internal override string DisplayName
        {
            get { return DisplayNameInternal(true); }
        }

        internal override string NonLocalizedDisplayName
        {
            get { return DisplayNameInternal(false); }
        }

        internal override DeleteEFElementCommand GetDeleteCommand()
        {
            DeleteEFElementCommand cmd = new DeleteEntityTypeMappingCommand(this);
            if (cmd == null)
            {
                // shouldn't happen, just to be safe
                throw new InvalidOperationException();
            }
            return cmd;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal EntityTypeMapping Clone(EntitySetMapping newEntitySetMapping)
        {
            // here we clone the entity type mapping, instead of re-parenting it
            // this works around an XML editor bug where re-parenting an element causes asserts

            // first create the new XElement
            var tempDoc = XDocument.Parse(XElement.ToString(SaveOptions.None), LoadOptions.None);
            var newetmXElement = tempDoc.Root;
            newetmXElement.Remove();
            // format the XML we just parsed
            Utils.FormatXML(newetmXElement, newEntitySetMapping.GetIndentLevel() + 1);

            // create the EntityTypeMapping & hook in it's xml. 
            var newetm = new EntityTypeMapping(newEntitySetMapping, newetmXElement);
            newetm.AddXElementToParent(newetmXElement);

            // parse & Resolve the new EntityTypeMapping
            newetm.Parse(new HashSet<XName>());
            XmlModelHelper.NormalizeAndResolve(newetm);

            // add it to new EntitySetMapping
            newEntitySetMapping.AddEntityTypeMapping(newetm);

            return newetm;
        }
    }
}
