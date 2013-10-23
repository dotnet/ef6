// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     This class derives from MultiItemBinding
    ///     <EntityType>
    ///         , so its able to track
    ///         a delimited list of items and bind them to EntityTypes.  Also, the X reference
    ///         may include the "IsTypeOf()" modifier:
    ///         <EntityTypeMapping cdm:TypeName="IsTypeOf(Test.Aruba1a.Baseline)">
    ///             So, this class also keeps track of a list of bools, denoting whether an item in the
    ///             binding list is using the "IsTypeOf()" modifier.
    /// </summary>
    internal class EntityTypeMappingTypeNameBinding : MultiItemBinding<EntityType>
    {
        private List<bool> _isTypeOfs;

        internal EntityTypeMappingTypeNameBinding(EFElement parent, NameNormalizer nameNormalizer)
            : base(parent, EntityTypeMapping.AttributeTypeName, ';', nameNormalizer)
        {
        }

        protected override void Init()
        {
            base.Init();

            _isTypeOfs = new List<bool>();

            if (RefName != null)
            {
                var typeNames = RefName.Split(_delimiter);
                foreach (var typeName in typeNames)
                {
                    if (EntityTypeMapping.UsesIsTypeOf(typeName))
                    {
                        _isTypeOfs.Add(true);
                    }
                    else
                    {
                        _isTypeOfs.Add(false);
                    }
                }
            }
        }

        internal IList<bool> IsTypeOfs
        {
            get { return _isTypeOfs.AsReadOnly(); }
        }

        internal bool IsTypeOf(Binding<EntityType> index)
        {
            var i = 0;
            foreach (var binding in Bindings)
            {
                if (binding == index)
                {
                    break;
                }
                i++;
            }

            Debug.Assert(i >= 0 && i < _isTypeOfs.Count, "i (" + i + ") should be >= 0 and <= _isTypeOfs.Count (" + _isTypeOfs.Count + ")");
            return _isTypeOfs[i];
        }

        /// <summary>
        ///     Set the list of type names. All type references are used directly (no IsOfType()).
        ///     Abstract types are ignored (they don't contribute anything to the mapping constraint unless IsOfType is used).
        ///     Pre-condition: given types contain at least one concrete type.
        /// </summary>
        /// <param name="items"></param>
        internal override void SetRefName(IEnumerable<EntityType> mappedTypes)
        {
            Entries = from cet in
                          (from entityType in mappedTypes
                           where entityType is ConceptualEntityType
                           select entityType as ConceptualEntityType)
                      where cet.IsConcrete
                      select new Entry(cet, false /* isTypeOf */);
        }

        /// <summary>
        ///     Entries referenced in the TypeName property of EntityTypeMapping.
        ///     Only resolved entries are returned by the getter.
        /// </summary>
        internal IEnumerable<Entry> Entries
        {
            get
            {
                for (var i = 0; i < Bindings.Count; i++)
                {
                    var mappedType = Bindings[i].Target;
                    if (mappedType != null)
                    {
                        yield return new Entry(mappedType, _isTypeOfs[i]);
                    }
                }
            }
            set
            {
                var typeListBuilder = new StringBuilder();
                foreach (var entry in value)
                {
                    if (typeListBuilder.Length > 0)
                    {
                        typeListBuilder.Append(_delimiter);
                    }
                    typeListBuilder.Append(entry.TypeReferenceString);
                }
                Debug.Assert(typeListBuilder.Length > 0, "Empty type list");

                SetXAttributeValue(typeListBuilder.ToString());

                Init();
            }
        }

        internal override void UpdateRefNameNamespaces(string oldNamespace, string newNamespace)
        {
            UpdateRefNameUsingReplacement(oldNamespace, newNamespace, RefNameReplacementPart.NamespacePart);
        }

        internal override void UpdateRefNameAliases(string oldAlias, string newAlias)
        {
            Debug.Fail("UpdateRefNameAliases() should never be called for EntityTypeMappingTypeNameBinding");
            return;
        }

        internal override void UpdateRefNameNamePart(string oldName, string newName)
        {
            UpdateRefNameUsingReplacement(oldName, newName, RefNameReplacementPart.NamePart);
            return;
        }

        private void UpdateRefNameUsingReplacement(string toBeReplaced, string replacement, RefNameReplacementPart partToBeReplaced)
        {
            Debug.Assert(!string.IsNullOrEmpty(toBeReplaced), "toBeReplaced must not null or empty");
            Debug.Assert(!string.IsNullOrEmpty(replacement), "replacement must not null or empty");
            if (string.IsNullOrEmpty(toBeReplaced)
                ||
                string.IsNullOrEmpty(replacement))
            {
                return;
            }

            // loop over all the references replacing toBeReplaced with
            // replacement if present in the correct part
            // Note: origRefNameAsNormalizedNames will contain the references without any
            // surrounding 'IsTypeOf()', we rely on _isTypeOfs to keep track of those.
            var origRefNameAsNormalizedNames = GetRefNameAsNormalizedNames();
            if (origRefNameAsNormalizedNames.Count != _isTypeOfs.Count)
            {
                Debug.Fail(
                    "Expected origRefNameAsNormalizedNames.Count == _isTypeOfs.Count, but got origRefNameAsNormalizedNames.Count = "
                    + origRefNameAsNormalizedNames.Count + ", _isTypeOfs.Count = " + _isTypeOfs.Count);
                return;
            }

            var replacementAttrValue = new StringBuilder();
            var replacedAnyPart = false;
            for (var i = 0; i < origRefNameAsNormalizedNames.Count; i++)
            {
                // prepend the delimiter if this is not the first reference
                if (i > 0)
                {
                    replacementAttrValue.Append(_delimiter);
                }

                // Replace the namespace part of the reference if necessary.
                // Note: replacementRefName will contain the reference with 
                // the namespace replaced or the original reference if no 
                // replacement is necessary.
                var normalizedName = origRefNameAsNormalizedNames[i];
                var isTypeOf = _isTypeOfs[i];
                string replacementRefName;
                if (RefNameReplacementPart.NamespacePart == partToBeReplaced)
                {
                    if (normalizedName.ConstructBindingStringWithReplacedNamespace(
                        toBeReplaced, replacement, out replacementRefName))
                    {
                        replacedAnyPart = true;
                    }
                }
                else if (RefNameReplacementPart.AliasPart == partToBeReplaced)
                {
                    if (normalizedName.ConstructBindingStringWithReplacedAlias(
                        toBeReplaced, replacement, out replacementRefName))
                    {
                        replacedAnyPart = true;
                    }
                }
                else
                {
                    if (normalizedName.ConstructBindingStringWithReplacedNamePart(
                        toBeReplaced, replacement, out replacementRefName))
                    {
                        replacedAnyPart = true;
                    }
                }

                if (isTypeOf)
                {
                    replacementAttrValue.Append(EntityTypeMapping.IsTypeOf);
                    replacementAttrValue.Append(replacementRefName);
                    replacementAttrValue.Append(EntityTypeMapping.IsTypeOfTerminal);
                }
                else
                {
                    replacementAttrValue.Append(replacementRefName);
                }
            }

            // if any replacement was made above then unbind the Binding and 
            // replace the underlying attribute value
            var replacementString = replacementAttrValue.ToString();
            if (replacedAnyPart)
            {
                Debug.Assert(
                    replacementString != RefName,
                    "EntityTypeMappingTypeNameBinding.UpdateRefNameNamespaces() is about to replace the following RefName with an identical value "
                    + RefName);
                Unbind();
                SetXAttributeValue(replacementAttrValue.ToString());
            }
            else
            {
                Debug.Assert(
                    replacementString == RefName,
                    "EntityTypeMappingTypeNameBinding.UpdateRefNameNamespaces() for RefName, " + RefName
                    + ", is about to ignore the following replacement string which is different " + replacementString);
            }
        }

        /// <summary>
        ///     An entry in the list of types referenced in EntityTypeMapping.
        ///     For example, "IsTypeOf(T1);T2" results in two entries,
        ///     the first one having IsTypeOf property set to true.
        /// </summary>
        internal struct Entry
        {
            private readonly EntityType _entityType;
            private readonly bool _isTypeOf;

            internal Entry(EntityType entityType, bool isTypeOf)
            {
                _entityType = entityType;
                _isTypeOf = isTypeOf;
            }

            internal bool IsTypeOf
            {
                get { return _isTypeOf; }
            }

            internal EntityType EntityType
            {
                get { return _entityType; }
            }

            internal string TypeReferenceString
            {
                get
                {
                    if (_isTypeOf)
                    {
                        return string.Format(CultureInfo.CurrentCulture,
                            EntityTypeMapping.IsTypeOfFormat, _entityType.NormalizedNameExternal);
                    }
                    else
                    {
                        return _entityType.NormalizedNameExternal;
                    }
                }
            }

            public override string ToString()
            {
                return TypeReferenceString;
            }
        }
    }
}
