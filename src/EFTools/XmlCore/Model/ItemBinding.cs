// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    internal abstract class ItemBinding : EFAttribute
    {
        protected enum RefNameReplacementPart
        {
            NamespacePart,
            AliasPart,
            NamePart
        }

        protected ItemBinding(EFContainer parent, XAttribute xattribute)
            : base(parent, xattribute)
        {
        }

        /// <summary>
        ///     Returns the string specified in the underlying attribute that set
        ///     this reference.  Setting this property updates this attribute.
        /// </summary>
        internal abstract string RefName { get; }

        /// <summary>
        ///     Sets the RefName property of this binding to point to the item.
        /// </summary>
        /// <param name="item"></param>
        internal abstract void SetRefName(EFNormalizableItem item);

        /// <summary>
        ///     Return true if this binding is resolved, check the status for details
        ///     if its not resolved.
        /// </summary>
        internal abstract bool Resolved { get; }

        /// <summary>
        ///     Tells the binding to use the current RefName and rebind itself
        /// </summary>
        internal abstract void Rebind();

        /// <summary>
        ///     Nulls out the binding. This is useful if we can't trust the Refname.
        /// </summary>
        internal abstract void ResetBind();

        /// <summary>
        ///     Tells the binding to "unbind" itself
        /// </summary>
        internal abstract void Unbind();

        /// <summary>
        ///     Returns true if the binding's status is unknown
        /// </summary>
        internal abstract bool IsStatusUnknown { get; }

        /// <summary>
        ///     Return true if the binding's target is disposed.
        /// </summary>
        internal abstract bool IsBindingTargetDisposed { get; }

        /// <summary>
        ///     Updates any references to the oldNamespace to refer to newNamespace
        /// </summary>
        internal abstract void UpdateRefNameNamespaces(string oldNamespace, string newNamespace);

        /// <summary>
        ///     Updates any references to the oldAlias to refer to newAlias
        /// </summary>
        internal abstract void UpdateRefNameAliases(string oldAlias, string newAlias);

        /// <summary>
        ///     Updates any references whose name part is oldName to refer to newName
        /// </summary>
        internal abstract void UpdateRefNameNamePart(string oldName, string newName);

        /// <summary>
        ///     Splits the RefName string into (potentially) multiple NormalizedName object
        /// </summary>
        protected abstract List<NormalizedName> GetRefNameAsNormalizedNames();

        /// <summary>
        ///     returns the resolved Targets that this ItemBinding points to
        /// </summary>
        internal abstract IEnumerable<EFNormalizableItem> ResolvedTargets { get; }

        protected static Binding<T> CreateBindingInstance<T>(
            EFArtifactSet artifactSet, Symbol normalizedName, ItemBinding parentItemBinding) where T : EFNormalizableItem
        {
            var status = BindingStatus.None;
            T target = null;

            var items = artifactSet.GetSymbolList(normalizedName);
            if (items.Count == 0)
            {
                // TODO: Enable this assert once we can suppress it during our InvalidDocumentTests. This will help in providing guidance for BindingStatus.Unknown issues.
                //Debug.Fail("Attempted to CreateBindingInstance for '" + parentItemBinding.ToPrettyString() + "' given the symbol: '" +
                //    normalizedName.ToDebugString() + "' but this symbol was not found in the ArtifactSet. This can happen if the given symbol " +
                //    " is composed of a name that is not constructed correctly. Check the custom NameNormalizer code for this ItemBinding.");
                status = BindingStatus.Unknown;
            }
            else
            {
                // Loop through items collection to find an element with the same type
                foreach (var element in items)
                {
                    target = element as T;
                    if (null != target)
                    {
                        break;
                    }
                }

                // Ideal case: where there is only 1 instance with the same name 
                if (null != target
                    && items.Count == 1)
                {
                    status = BindingStatus.Known;
                }
                else if (null != target
                         && items.Count > 1)
                {
                    status = BindingStatus.Duplicate;
                }
                else // target is null
                {
                    // TODO: Enable this assert once we can suppress it during our InvalidDocumentTests. This will help in providing guidance for BindingStatus.Unknown issues.
                    //Debug.Fail("Attempted to CreateBindingInstance for '" + parentItemBinding.ToPrettyString() + "' given the symbol: '" +
                    //    normalizedName.ToDebugString() + "' but none of the symbols found in the ArtifactSet match the type " + typeof(T).ToString());
                    status = BindingStatus.Unknown;
                }
            }
            return new Binding<T>(parentItemBinding, status, target);
        }
    }
}
