// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    ///     Represents a end of a Association Type
    /// </summary>
    public sealed class AssociationEndMember : RelationshipEndMember
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of AssociationEndMember
        /// </summary>
        /// <param name="name"> name of the association end member </param>
        /// <param name="endRefType"> Ref type that this end refers to </param>
        /// <param name="multiplicity"> multiplicity of the end </param>
        internal AssociationEndMember(
            string name,
            RefType endRefType,
            RelationshipMultiplicity multiplicity)
            : base(name, endRefType, multiplicity)
        {
        }

        #endregion

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.AssociationEndMember; }
        }

        private Func<RelationshipManager, RelatedEnd, RelatedEnd> _getRelatedEndMethod;

        /// <summary>
        ///     cached dynamic method to set a CLR property value on a CLR instance
        /// </summary>
        internal Func<RelationshipManager, RelatedEnd, RelatedEnd> GetRelatedEnd
        {
            get { return _getRelatedEndMethod; }
            set
            {
                Debug.Assert(null != value, "clearing GetRelatedEndMethod");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _getRelatedEndMethod, value, null);
            }
        }
    }
}
