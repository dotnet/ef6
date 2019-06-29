// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    ///     Represents information about an EFElement whose corresponding XElement
    ///     node defining it may not have been parsed yet or may not exist.	 If it
    ///     is known, then ReferencedInfo will return non-null and the Status will
    ///     be "Known".  If a name has not been provided for this reference, then
    ///     Name will be null and Status will be "Undef".  Otherwise, if a name is
    ///     provided but the reference has not been encountered, Status will be
    ///     "Unknown" if parsing is complete (i.e. it is a dangling reference).
    /// </summary>
    [DebuggerDisplay("SIB<{typeof(T).ToString()}> -> {RefName}")]
    internal class SingleItemBinding<T> : ItemBinding
        where T : EFNormalizableItem, IDisposable
    {
        internal delegate NormalizedName NameNormalizer(EFElement parent, string refName);

        private Binding<T> _binding;
        private readonly string _attributeName;
        private readonly NameNormalizer _nameNormalizer;

        internal SingleItemBinding(EFElement parent, string attributeName, NameNormalizer nameNormalizer)
            : base(parent, parent.GetAttribute(attributeName))
        {
            _attributeName = attributeName;
            _nameNormalizer = nameNormalizer;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_binding != null)
                {
                    _binding.Dispose();
                }
            }
        }

        internal override string ToPrettyString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}::{1}>>{2}", Parent.EFTypeName, _attributeName, RefName);
        }

        #region EFObject overrides

        internal override string SemanticName
        {
            get { return Parent.SemanticName + "/" + _attributeName + "[0](ItemBinding)"; }
        }

        internal override string EFTypeName
        {
            get { return _attributeName; }
        }

        #endregion

        internal Symbol NormalizedName()
        {
            // just return the first (and only) symbol from GetRefNameAsNormalizedNames()
            var normalizedNames = GetRefNameAsNormalizedNames();
            Debug.Assert(
                normalizedNames.Count == 1,
                GetType().FullName + ".NormalizedName(): expected normalizedNames.Count to be 1, got " + normalizedNames.Count);
            foreach (var normalizedName in normalizedNames)
            {
                return normalizedName.Symbol;
            }

            return null;
        }

        protected override List<NormalizedName> GetRefNameAsNormalizedNames()
        {
            var normalizedNamesList = new List<NormalizedName>();
            if (RefName == null)
            {
                var symbol = new Symbol(String.Empty);
                normalizedNamesList.Add(new NormalizedName(symbol, null, null, String.Empty));
            }
            else
            {
                if (_nameNormalizer == null)
                {
                    var symbol = new Symbol(RefName);
                    normalizedNamesList.Add(new NormalizedName(symbol, null, null, RefName));
                }
                else
                {
                    var nname = _nameNormalizer(Parent as EFElement, RefName);
                    if (nname == null)
                    {
                        throw new InvalidOperationException(
                            string.Format(CultureInfo.CurrentCulture, Resources.UnresolvedReference_0, RefName));
                    }
                    else
                    {
                        normalizedNamesList.Add(nname);
                    }
                }
            }

            return normalizedNamesList;
        }

        private void Init()
        {
            Unbind();

            if (RefName != null)
            {
                var normalizedName = NormalizedName();
                var artifactSet = Artifact.ModelManager.GetArtifactSet(Artifact.Uri);
                _binding = CreateBindingInstance<T>(artifactSet, normalizedName, this);
            }
            else
            {
                _binding = new Binding<T>(this, BindingStatus.Undefined, null);
            }
        }

        /// <summary>
        ///     Returns the target value of the reference, i.e. the EFElement object
        ///     being referred to.  Returns null unless the reference's status is
        ///     Known or Duplicate.
        /// </summary>
        public T Target
        {
            get
            {
                if (_binding == null)
                {
                    return null;
                }
                else
                {
                    return _binding.Target;
                }
            }
        }

        internal override IEnumerable<EFNormalizableItem> ResolvedTargets
        {
            get
            {
                if (_binding != null
                    && _binding.Status == BindingStatus.Known)
                {
                    yield return _binding.Target;
                }
            }
        }

        /// <summary>
        ///     Returns the target value of the reference if it is not null.
        ///     Throws otherwise.
        /// </summary>
        internal T SafeTarget
        {
            get
            {
                var target = Target;
                if (target == null)
                {
                    ModelHelper.InvalidSchemaError(Resources.UnresolvedReference_0, RefName);
                }
                return target;
            }
        }

        internal override void SetRefName(EFNormalizableItem item)
        {
            if (item == null)
            {
                SetXAttributeValue(null);
            }
            else
            {
                SetXAttributeValue(item.GetRefNameForBinding(this));
            }
        }

        internal override string RefName
        {
            get { return GetXAttributeValue(); }
        }

        public BindingStatus Status
        {
            get
            {
                if (_binding == null
                    && RefName == null)
                {
                    return BindingStatus.Undefined;
                }
                else if (_binding == null)
                {
                    // we haven't been resolved yet. 
                    return BindingStatus.None;
                }
                else
                {
                    return _binding.Status;
                }
            }
        }

        internal override bool IsStatusUnknown
        {
            get { return Status == BindingStatus.Unknown; }
        }

        internal override bool IsBindingTargetDisposed
        {
            get { return (Target != null && Target.IsDisposed); }
        }

        internal override bool Resolved
        {
            get { return Status == BindingStatus.Known; }
        }

        internal override void Rebind()
        {
            // do a brute force recompile by just re-init'ing the object
            Init();
        }

        internal override void ResetBind()
        {
            Unbind();
            _binding = new Binding<T>(this, BindingStatus.Undefined, null);
        }

        internal override void Unbind()
        {
            if (_binding != null)
            {
                _binding.Dispose();
                _binding = null;
            }
        }

        internal override void UpdateRefNameNamespaces(string oldNamespace, string newNamespace)
        {
            UpdateRefNameUsingReplacement(oldNamespace, newNamespace, RefNameReplacementPart.NamespacePart);
        }

        internal override void UpdateRefNameAliases(string oldAlias, string newAlias)
        {
            UpdateRefNameUsingReplacement(oldAlias, newAlias, RefNameReplacementPart.AliasPart);
        }

        internal override void UpdateRefNameNamePart(string oldName, string newName)
        {
            UpdateRefNameUsingReplacement(oldName, newName, RefNameReplacementPart.NamePart);
        }

        private void UpdateRefNameUsingReplacement(string toBeReplaced, string replacement, RefNameReplacementPart partToBeReplaced)
        {
            Debug.Assert(
                !string.IsNullOrEmpty(toBeReplaced),
                GetType().FullName + ".UpdateRefNameUsingReplacement(): toBeReplaced must not null or empty");
            Debug.Assert(
                !string.IsNullOrEmpty(replacement),
                GetType().FullName + ".UpdateRefNameUsingReplacement(): replacement must not null or empty");
            if (string.IsNullOrEmpty(toBeReplaced)
                || string.IsNullOrEmpty(replacement))
            {
                return;
            }

            var refNameAsNormalizedNames = GetRefNameAsNormalizedNames();
            if (1 != refNameAsNormalizedNames.Count)
            {
                Debug.Fail(
                    GetType().FullName
                    + ".UpdateRefNameUsingReplacement(): expecting only 1 symbol from GetRefNameAsNormalizedNames(). Received "
                    + refNameAsNormalizedNames.Count);
                return;
            }

            // replace toBeReplaced with replacement if present in the correct part
            var refNameAsNameRef = refNameAsNormalizedNames[0];
            var madeReplacement = false;
            string replacementRefName;
            if (RefNameReplacementPart.NamespacePart == partToBeReplaced)
            {
                if (refNameAsNameRef.ConstructBindingStringWithReplacedNamespace(
                    toBeReplaced, replacement, out replacementRefName))
                {
                    madeReplacement = true;
                }
            }
            else if (RefNameReplacementPart.AliasPart == partToBeReplaced)
            {
                if (refNameAsNameRef.ConstructBindingStringWithReplacedAlias(
                    toBeReplaced, replacement, out replacementRefName))
                {
                    madeReplacement = true;
                }
            }
            else
            {
                if (refNameAsNameRef.ConstructBindingStringWithReplacedNamePart(
                    toBeReplaced, replacement, out replacementRefName))
                {
                    madeReplacement = true;
                }
            }

            // if any replacement was made above then unbind the Binding and 
            // replace the underlying attribute value
            if (madeReplacement)
            {
                Debug.Assert(
                    replacementRefName != RefName,
                    "UpdateRefNameNamespaceOrAlias() is about to replace the following RefName with an identical value " + RefName);
                Unbind();

                // For most scenarios, RefName should be equal to refNameAsNameRef.ToBindingString()
                // But in FunctionImport's Return, the values are as follow:
                // RefName : "Collection([Old Fully-qualified EntityType/ComplexType])"
                // refNameAsNameRef.ToBindingString(): [Old Fully-qualified EntityType/ComplexType]
                // replacementRefName: [New Fully-qualified EntityType/ComplexType]
                if (String.Compare(RefName, refNameAsNameRef.ToBindingString(), StringComparison.CurrentCulture) != 0)
                {
                    replacementRefName = RefName.Replace(refNameAsNameRef.ToBindingString(), replacementRefName);
                }
                SetXAttributeValue(replacementRefName);
            }
            else
            {
                Debug.Assert(
                    replacementRefName == RefName,
                    "UpdateRefNameNamespaceOrAlias() for RefName, " + RefName
                    + ", is about to ignore the following replacement string which is different " + replacementRefName);
            }
        }
    }
}
