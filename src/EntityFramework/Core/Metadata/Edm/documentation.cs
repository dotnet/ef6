// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    /// Class representing the Documentation associated with an item
    /// </summary>
    public sealed class Documentation : MetadataItem
    {
        private string _summary = "";
        private string _longDescription = "";

        // <summary>
        // Default constructor - primarily created for supporting usage of this Documentation class by SOM.
        // </summary>
        internal Documentation()
        {
        }

        /// <summary>
        /// Initializes a new Documentation instance.
        /// </summary>
        /// <param name="summary">A summary string.</param>
        /// <param name="longDescription">A long description string.</param>
        public Documentation(string summary, string longDescription)
        {
            Summary = summary;
            LongDescription = longDescription;
        }

        /// <summary>
        /// Gets the built-in type kind for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" />.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" />
        /// .
        /// </returns>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.Documentation; }
        }

        /// <summary>
        /// Gets the summary for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" />.
        /// </summary>
        /// <returns>
        /// The summary for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" />.
        /// </returns>
        public string Summary
        {
            get { return _summary; }
            internal set
            {
                if (value != null)
                {
                    _summary = value;
                }
                else
                {
                    _summary = "";
                }
            }
        }

        /// <summary>
        /// Gets the long description for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" />.
        /// </summary>
        /// <returns>
        /// The long description for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" />.
        /// </returns>
        public string LongDescription
        {
            get { return _longDescription; }
            internal set
            {
                if (value != null)
                {
                    _longDescription = value;
                }
                else
                {
                    _longDescription = "";
                }
            }
        }

        // <summary>
        // This property is required to be implemented for inheriting from MetadataItem. As there can be atmost one
        // instance of a nested-Documentation, return the constant "Documentation" as it's identity.
        // </summary>
        internal override string Identity
        {
            get { return "Documentation"; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" /> object contains only a null or an empty
        /// <see
        ///     cref="P:System.Data.Entity.Core.Metadata.Edm.Documentation.Summary" />
        /// and a
        /// <see
        ///     cref="P:System.Data.Entity.Core.Metadata.Edm.Documentation.Longdescription" />
        /// .
        /// </summary>
        /// <returns>
        /// true if this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" /> object contains only a null or an empty
        /// <see
        ///     cref="P:System.Data.Entity.Core.Metadata.Edm.Documentation.Summary" />
        /// and a
        /// <see
        ///     cref="P:System.Data.Entity.Core.Metadata.Edm.Documentation.LongDescription" />
        /// ; otherwise, false.
        /// </returns>
        public bool IsEmpty
        {
            get
            {
                if (string.IsNullOrEmpty(_summary)
                    && string.IsNullOrEmpty(_longDescription))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns the summary for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" />.
        /// </summary>
        /// <returns>
        /// The summary for this <see cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" />.
        /// </returns>
        public override string ToString()
        {
            return _summary;
        }
    }
}
