// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    ///     Represents the Relationship type
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public abstract class RelationshipType : EntityTypeBase
    {
        private ReadOnlyMetadataCollection<RelationshipEndMember> _relationshipEndMembers;

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of relationship type
        /// </summary>
        /// <param name="name"> name of the relationship type </param>
        /// <param name="namespaceName"> namespace of the relationship type </param>
        /// <param name="version"> version of the relationship type </param>
        /// <param name="dataSpace"> dataSpace in which this edmtype belongs to </param>
        /// <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal RelationshipType(
            string name,
            string namespaceName,
            DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Returns the list of ends for this relationship type
        /// </summary>
        public ReadOnlyMetadataCollection<RelationshipEndMember> RelationshipEndMembers
        {
            get
            {
                Debug.Assert(
                    IsReadOnly,
                    "this is a wrapper around this.Members, don't call it during metadata loading, only call it after the metadata is set to readonly");
                if (null == _relationshipEndMembers)
                {
                    var relationshipEndMembers = new FilteredReadOnlyMetadataCollection<RelationshipEndMember, EdmMember>(
                        Members, Helper.IsRelationshipEndMember);
                    Interlocked.CompareExchange(ref _relationshipEndMembers, relationshipEndMembers, null);
                }
                return _relationshipEndMembers;
            }
        }

        #endregion
    }
}
