namespace System.Data.Entity.Edm.Db
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;

    internal abstract class DbConstraintMetadata
        : DbNamedMetadataItem
    {
    }

    /// <summary>
    ///     Allows the construction and modification of a foreign key constraint sourced by a <see cref = "DbTableMetadata" /> instance.
    /// </summary>
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

    /*
    internal class DbUniqueConstraintMetadata : DbConstraintMetadata
    {
        internal DbUniqueConstraintMetadata()
        {
        }

        public virtual IList<DbTableColumnMetadata> UniqueColumns { get { return this.uniqueColumnsList.EnsureValue(); } set { this.uniqueColumnsList.SetValue(value); } }
    }
    
    internal class DbRelationMetadata : DbNamedMetadataItem
    {
        public virtual DbTableMetadata PrincipalTable { get; set; }
        public virtual DbUniqueConstraintMetadata PrincipalConstraint { get; set; } // If null, primary key is assumed
        public virtual DbTableMetadata DependentTable { get; set; }
        public virtual DbForeignKeyConstraintMetadata DependentConstraint { get; set; }
    }*/
}
