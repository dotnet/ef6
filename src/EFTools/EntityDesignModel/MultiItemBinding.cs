// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;

    /// <summary>
    ///     Represents information about an EFElement whose corresponding XElement
    ///     node defining it may not have been parsed yet or may not exist.	 If it
    ///     is known, then ReferencedInfo will return non-null and the Status will
    ///     be "Known".  If a name has not been provided for this reference, then
    ///     Name will be null and Status will be "Undef".  Otherwise, if a name is
    ///     provided but the reference has not been encountered, Status will be
    ///     "Unknown" if parsing is complete (i.e. it is a dangling reference).
    /// </summary>
    internal class MultiItemBinding<T> : ItemBinding
        where T : EFNormalizableItem
    {
        internal delegate NormalizedName NameNormalizer(EFElement parent, string refName);

        private readonly List<Binding<T>> _bindings = new List<Binding<T>>();
        private readonly string _attributeName;
        private readonly NameNormalizer _nameNormalizer;
        protected char _delimiter;

        internal MultiItemBinding(EFElement parent, string attributeName, char delimiter, NameNormalizer nameNormalizer)
            : base(parent, parent.XElement.Attribute(attributeName))
        {
            _attributeName = attributeName;
            _delimiter = delimiter;
            _nameNormalizer = nameNormalizer;
        }

        private void DisposeBindings()
        {
            if (_bindings != null)
            {
                foreach (var b in _bindings)
                {
                    b.Dispose();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeBindings();
            }
        }

        internal override string ToPrettyString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}::{1}>>{2}", Parent.EFTypeName, _attributeName, RefName);
        }

        internal char Delimiter
        {
            get { return _delimiter; }
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

        protected override List<NormalizedName> GetRefNameAsNormalizedNames()
        {
            var normalizedNamesList = new List<NormalizedName>();
            if (RefName != null)
            {
                var newRefNames = RefName.Split(_delimiter);
                foreach (var newRefName in newRefNames)
                {
                    if (_nameNormalizer == null)
                    {
                        var symbol = new Symbol(newRefName);
                        normalizedNamesList.Add(new NormalizedName(symbol, null, null, newRefName));
                    }
                    else
                    {
                        normalizedNamesList.Add(_nameNormalizer(Parent as EFElement, newRefName));
                    }
                }
            }

            return normalizedNamesList;
        }

        protected virtual void Init()
        {
            Unbind();

            if (RefName != null)
            {
                var artifactSet = Artifact.ModelManager.GetArtifactSet(Artifact.Uri);
                foreach (var normalizedNameRef in GetRefNameAsNormalizedNames())
                {
                    _bindings.Add(CreateBindingInstance<T>(artifactSet, normalizedNameRef.Symbol, this));
                }
            }
            else
            {
                var binding = new Binding<T>(this, BindingStatus.Undefined, null);
                _bindings.Add(binding);
            }
        }

        /// <summary>
        ///     Returns a read-only list of bindings.
        /// </summary>
        internal IList<Binding<T>> Bindings
        {
            get { return _bindings.AsReadOnly(); }
        }

        internal override IEnumerable<EFNormalizableItem> ResolvedTargets
        {
            get
            {
                foreach (var b in _bindings)
                {
                    if (b.Status == BindingStatus.Known)
                    {
                        yield return b.Target;
                    }
                }
            }
        }

        internal string TargetsListForDisplay
        {
            get
            {
                var targetsList = string.Empty;
                foreach (var binding in _bindings)
                {
                    var target = string.Empty;
                    if (binding.Status == BindingStatus.Known
                        ||
                        binding.Status == BindingStatus.Duplicate)
                    {
                        target = binding.Target.DisplayName;
                    }
                    else
                    {
                        // add a spacer so the array stays the same size
                        target = "<null>";
                    }

                    if (!string.IsNullOrEmpty(targetsList))
                    {
                        targetsList += ", ";
                    }
                    targetsList += target;
                }

                return targetsList;
            }
        }

        /// <summary>
        ///     NOTE: This will replace the entire RefName with this item's name, replacing any
        ///     list that might be there.  If you want to maintain the list, call SetRefName(IEnumerable<T> items).
        /// </summary>
        /// <param name="item"></param>
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

        /// <summary>
        ///     Sets the list of references to point to the given items
        /// </summary>
        /// <param name="items"></param>
        internal virtual void SetRefName(IEnumerable<T> items)
        {
            throw new NotImplementedException("Forgot to override method in derived class?");
        }

        internal override string RefName
        {
            get { return GetXAttributeValue(); }
        }

        internal int Status
        {
            get
            {
                var status = 0;
                foreach (var binding in _bindings)
                {
                    status |= (int)binding.Status;
                }

                return status;
            }
        }

        internal override bool IsStatusUnknown
        {
            get { return ((Status & (int)BindingStatus.Unknown) == (int)BindingStatus.Unknown); }
        }

        internal override bool IsBindingTargetDisposed
        {
            get
            {
                foreach (var binding in Bindings)
                {
                    if (binding.Target != null
                        && binding.Target.IsDisposed)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        internal override bool Resolved
        {
            get { return (Status & (int)BindingStatus.Known) == (int)BindingStatus.Known; }
        }

        internal override void Rebind()
        {
            // do a brute force recompile by just re-init'ing the object
            Init();
        }

        internal override void ResetBind()
        {
            Unbind();
            var binding = new Binding<T>(this, BindingStatus.Undefined, null);
            _bindings.Add(binding);
        }

        internal override void Unbind()
        {
            DisposeBindings();
            _bindings.Clear();
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
            Debug.Assert(!string.IsNullOrEmpty(toBeReplaced), "UpdateRefNameUsingReplacement: toBeReplaced must not null or empty");
            Debug.Assert(!string.IsNullOrEmpty(replacement), "UpdateRefNameUsingReplacement: replacement must not null or empty");
            if (string.IsNullOrEmpty(toBeReplaced)
                || string.IsNullOrEmpty(replacement))
            {
                return;
            }

            // loop over all the references replacing toBeReplaced with
            // replacement if present in the correct part
            var origRefNameAsNormalizedNames = GetRefNameAsNormalizedNames();
            var replacementAttrValue = new StringBuilder();
            var replacedAnyPart = false;
            var firstRef = true;
            foreach (var normalizedName in origRefNameAsNormalizedNames)
            {
                // prepend the delimiter if this is not the first reference
                if (!firstRef)
                {
                    replacementAttrValue.Append(_delimiter);
                }
                else
                {
                    firstRef = false;
                }

                // Replace the appropriate part of the reference if necessary.
                // Note: replacementRefName will contain the reference with 
                // the appropriate part replaced or the original reference if no 
                // replacement is necessary.
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

                replacementAttrValue.Append(replacementRefName);
            }

            // if any replacement was made above then unbind the Binding and 
            // replace the underlying attribute value
            var replacementString = replacementAttrValue.ToString();
            if (replacedAnyPart)
            {
                Debug.Assert(
                    replacementString != RefName,
                    "UpdateRefNameNamespaceOrAlias() is about to replace the following RefName with an identical value " + RefName);
                Unbind();
                SetXAttributeValue(replacementAttrValue.ToString());
            }
            else
            {
                Debug.Assert(
                    replacementString == RefName,
                    "UpdateRefNameNamespaceOrAlias() for RefName, " + RefName
                    + ", is about to ignore the following replacement string which is different " + replacementString);
            }
        }
    }
}
