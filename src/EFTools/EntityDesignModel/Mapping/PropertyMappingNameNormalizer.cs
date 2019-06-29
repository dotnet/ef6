// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal static class PropertyMappingNameNormalizer
    {
        internal static NormalizedName NameNormalizer(EFElement parent, string refName)
        {
            Debug.Assert(parent != null, "parent should not be null");

            if (refName == null)
            {
                return null;
            }

            var parentItem = parent.Parent as EFElement;
            Debug.Assert(parentItem != null, "parent.Parent should be an EFElement");

            Debug.Assert(
                parentItem.GetParentOfType(typeof(FunctionImportTypeMapping)) == null,
                "Use the FunctionImportPropertyMappingNameNormalizer to normalize children of " + parent.GetType().FullName);

            Debug.Assert(
                parentItem.GetParentOfType(typeof(FunctionComplexProperty)) == null,
                "Use the FunctionPropertyMappingNameNormalizer to normalize children of " + parent.GetType().FullName);

            Debug.Assert(
                parentItem.GetParentOfType(typeof(FunctionAssociationEnd)) == null,
                "Use the FunctionPropertyMappingNameNormalizer to normalize children of " + parent.GetType().FullName);

            NormalizedName normalizedName = null;

            //
            // try to normalize for location in a ComplexProperty
            //
            var cp = parentItem.GetParentOfType(typeof(ComplexProperty)) as ComplexProperty;
            normalizedName = NormalizePropertyNameRelativeToComplexProperty(cp, refName);
            if (normalizedName != null)
            {
                return normalizedName;
            }

            //
            // try to normalize for an EntityTyepMapping with no FunctionAssociationEnd 
            //
            var etm = parentItem.GetParentOfType(typeof(EntityTypeMapping)) as EntityTypeMapping;
            normalizedName = NormalizePropertyNameRelativeToEntityTypeMapping(etm, parent, refName);
            if (normalizedName != null)
            {
                return normalizedName;
            }

            //
            // try to normalize for an AssociationSetMapping
            //
            var asm = parentItem.GetParentOfType(typeof(AssociationSetMapping)) as AssociationSetMapping;
            var ep = parentItem.GetParentOfType(typeof(EndProperty)) as EndProperty;
            normalizedName = NormalizePropertyNameRelativeToAssociationSetMapping(asm, ep, parent, refName);
            if (normalizedName != null)
            {
                return normalizedName;
            }

            if (asm != null)
            {
                var cond = parent as Condition;
                Debug.Assert(
                    cond == null, "It is assumed that Conditions under an AssociationSetMapping cannot have their Name property set.");
            }

            //
            //  Default case...
            //
            return new NormalizedName(new Symbol(refName), null, null, refName);
        }

        /// <summary>
        ///     Normalize a ref name relative to an Association End.  This will return true if the symbol is valid & points to a valid
        ///     EFElement, false otherwise.
        /// </summary>
        /// <param name="setEnd"></param>
        /// <param name="parent"></param>
        /// <param name="refName"></param>
        /// <param name="symbol"></param>
        /// <returns></returns>
        internal static NormalizedName NormalizeNameFromAssociationSetEnd(AssociationSetEnd setEnd, EFElement parent, string refName)
        {
            if (setEnd.Role.Status == BindingStatus.Known)
            {
                var end = setEnd.Role.Target;
                if (end.Type.Status == BindingStatus.Known)
                {
                    var type = end.Type.Target;
                    if (type != null)
                    {
                        var cet = type as ConceptualEntityType;
                        if (cet != null)
                        {
                            // this is a c-side entity type
                            while (cet != null)
                            {
                                var nn = GetNormalizedNameRelativeToEntityType(cet, refName, parent.Artifact.ArtifactSet);
                                if (nn != null)
                                {
                                    return nn;
                                }
                                cet = cet.BaseType.Target;
                            }
                        }
                        else
                        {
                            // this is an s-side entity type
                            var nn = GetNormalizedNameRelativeToEntityType(type, refName, parent.Artifact.ArtifactSet);
                            if (nn != null)
                            {
                                return nn;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private static NormalizedName GetNormalizedNameRelativeToEntityType(EntityType et, string refName, EFArtifactSet artifactSet)
        {
            var symbol = new Symbol(et.NormalizedName, refName);
            var item = artifactSet.LookupSymbol(symbol);
            if (item != null)
            {
                return new NormalizedName(symbol, null, null, refName);
            }
            return null;
        }

        /// <summary>
        ///     return a new NormalizedName for a refName that is a child of ComplexProperty
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="refName"></param>
        /// <returns></returns>
        private static NormalizedName NormalizePropertyNameRelativeToComplexProperty(ComplexProperty cp, string refName)
        {
            NormalizedName nn = null;
            if (cp != null)
            {
                // for complex property mapping we create Symbol from corresponding ComplexType
                if (cp.Name.Status == BindingStatus.Known)
                {
                    var complexType = cp.Name.Target.ComplexType.Target;
                    if (complexType != null)
                    {
                        var symbol = new Symbol(complexType.NormalizedName, refName);
                        nn = new NormalizedName(symbol, null, null, refName);
                    }
                }
            }
            return nn;
        }

        /// <summary>
        ///     Normalize a refName where the refName is a child of a EntityTypeMapping
        ///     <param name="etm"></param>
        ///     <param name="fae"></param>
        ///     <param name="parent"></param>
        ///     <param name="refName"></param>
        ///     <returns></returns>
        internal static NormalizedName NormalizePropertyNameRelativeToEntityTypeMapping(
            EntityTypeMapping etm, EFElement parent, string refName)
        {
            NormalizedName nn = null;

            if (etm != null
                && etm.TypeName.Status == (int)BindingStatus.Known)
            {
                // walk up until we find our EntityTypeMapping, then we can walk over to the EntityType(s)
                // that should contain this property name
                foreach (var binding in etm.TypeName.Bindings)
                {
                    if (binding.Status == BindingStatus.Known)
                    {
                        var cet = binding.Target as ConceptualEntityType;
                        Debug.Assert(cet != null, "EntityType is not EntityTypeMapping");

                        // for each entity type in the list, look to see it contains the property
                        // if not, then check its parent
                        var typesToCheck = new List<ConceptualEntityType>();
                        typesToCheck.Add(cet);
                        typesToCheck.AddRange(cet.ResolvableBaseTypes);

                        foreach (EntityType entityType in typesToCheck)
                        {
                            var entityTypeSymbol = entityType.NormalizedName;
                            var symbol = new Symbol(entityTypeSymbol, refName);

                            var artifactSet = parent.Artifact.ModelManager.GetArtifactSet(parent.Artifact.Uri);
                            var item = artifactSet.LookupSymbol(symbol);
                            if (item != null)
                            {
                                nn = new NormalizedName(symbol, null, null, refName);
                            }
                        }
                    }
                }
            }
            return nn;
        }

        /// <summary>
        ///     Normalize a refName where the refName is a child of a AssociationSetMapping
        /// </summary>
        /// <param name="asm"></param>
        /// <param name="ep"></param>
        /// <param name="parent"></param>
        /// <param name="refName"></param>
        /// <returns></returns>
        private static NormalizedName NormalizePropertyNameRelativeToAssociationSetMapping(
            AssociationSetMapping asm, EndProperty ep, EFElement parent, string refName)
        {
            NormalizedName nn = null;
            if (ep != null
                && asm != null)
            {
                var prop = parent as ScalarProperty;
                Debug.Assert(prop != null, "parent should be a ScalarProperty");

                if (ep.Name.Status == BindingStatus.Known)
                {
                    nn = NormalizeNameFromAssociationSetEnd(ep.Name.Target, parent, refName);
                }
            }
            return nn;
        }
    }
}
