// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Allows the construction and modification of a foreign key constraint sourced by a <see cref="DbTableMetadata" /> instance.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    public class DbForeignKeyConstraintMetadata : DbConstraintMetadata
    {
        private IList<DbTableColumnMetadata> dependentColumnsList = new List<DbTableColumnMetadata>();

        internal override DbItemKind GetMetadataKind()
        {
            return DbItemKind.ForeignKeyConstraint;
        }

        //public virtual DbUniqueConstraintMetadata PrincipalConstraint { get; set; }
        public virtual DbTableMetadata PrincipalTable { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<DbTableColumnMetadata> DependentColumns
        {
            get { return dependentColumnsList; }
            set { dependentColumnsList = value; }
        }

        /// <summary>
        ///     Gets or sets the <see cref="OperationAction" /> to take when a delete operation is attempted.
        /// </summary>
        public virtual OperationAction DeleteAction { get; set; }
    }
}
