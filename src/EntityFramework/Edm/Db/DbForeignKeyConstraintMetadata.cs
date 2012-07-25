// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Allows the construction and modification of a foreign key constraint sourced by a <see cref = "DbTableMetadata" /> instance.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal class DbForeignKeyConstraintMetadata : DbConstraintMetadata
    {
        private readonly BackingList<DbTableColumnMetadata> dependentColumnsList =
            new BackingList<DbTableColumnMetadata>();

        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.ForeignKeyConstraint;
        }

        //public virtual DbUniqueConstraintMetadata PrincipalConstraint { get; set; }
        public virtual DbTableMetadata PrincipalTable { get; set; }

        public virtual IList<DbTableColumnMetadata> DependentColumns
        {
            get { return dependentColumnsList.EnsureValue(); }
            set { dependentColumnsList.SetValue(value); }
        }

        internal bool HasDependentColumns
        {
            get { return dependentColumnsList.HasValue; }
        }

        /// <summary>
        ///     Gets or sets the <see cref = "DbOperationAction" /> to take when a delete operation is attempted.
        /// </summary>
        public virtual DbOperationAction DeleteAction { get; set; }
    }
}
