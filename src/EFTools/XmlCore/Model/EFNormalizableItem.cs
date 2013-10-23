// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;
    using System.Xml.Linq;

    internal abstract class EFNormalizableItem : EFElement
    {
        private Symbol _normalizedName; // = string.Empty;

        /// <summary>
        ///     This returns the last "part" of the normalized name
        /// </summary>
        internal static string GetLocalNameFromNormalizedName(Symbol normalizedName)
        {
            var localName = normalizedName.GetLocalName();
            Debug.Assert(string.IsNullOrEmpty(localName) == false);
            return localName;
        }

        /// <summary>
        ///     This removes our separator and replaces it with the runtime's separator
        /// </summary>
        internal static string ConvertSymbolToExternal(Symbol symbol)
        {
            return symbol.ToDisplayString();
        }

        protected EFNormalizableItem(EFContainer parent, XElement element)
            : base(parent, element)
        {
        }

        #region EFNormalizableItem Members

        /// <summary>
        ///     This is used to set and get the item's normalized name.  The normalized name is a set of
        ///     item names concatenated together with the EFNormalizableItem.NORMALIZED_NAME_SEPARATOR.
        ///     NOTE: use the NormalizedNameExternal version to display to the user or write to the XML.
        /// </summary>
        internal Symbol NormalizedName
        {
            get
            {
                if (_normalizedName != null)
                {
                    return _normalizedName;
                }
                else
                {
                    return new Symbol(GetType().Name);
                }
            }
            set
            {
                var artifactSet = Artifact.ModelManager.GetArtifactSet(Artifact.Uri);

                if (_normalizedName != null)
                {
                    artifactSet.RemoveSymbol(_normalizedName, this);
                }
                _normalizedName = value;
                artifactSet.AddSymbol(_normalizedName, this);
            }
        }

        /// <summary>
        ///     This property returns the normalized name in a way that works for writing out to the XML file
        ///     and for displaying to the user; it strips out our extra stuff that we use to ensure that symbols
        ///     can be bound correctly.
        /// </summary>
        internal string NormalizedNameExternal
        {
            get { return NormalizedName.ToExternalString(); }
        }

        /// <summary>
        ///     This the text that should be written out to the XML that can refer back to this
        ///     item.  Most items can use the NormalizedNameExternal so that is what we
        ///     will use by default.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        internal virtual string GetRefNameForBinding(ItemBinding binding)
        {
            return NormalizedNameExternal;
        }

        #endregion

        internal override string ToPrettyString()
        {
            return NormalizedNameExternal;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_normalizedName != null)
                {
                    Artifact.ModelManager.GetArtifactSet(Artifact.Uri).RemoveSymbol(_normalizedName, this);
                }
            }
        }

        /// <summary>
        ///     Derived classes need to implement this if they want to support Rename.
        /// </summary>
        /// <param name="newName"></param>
        internal virtual void Rename(string newName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Derived classes need to implement this if they want to support rename.
        /// </summary>
        /// <returns></returns>
        internal virtual DefaultableValue<string> GetNameAttribute()
        {
            throw new NotImplementedException();
        }

        public abstract IValueProperty<string> Name { get; }
    }
}
