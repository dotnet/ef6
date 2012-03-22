namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Configures the table and column mapping of a many:many relationship.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref = "DbModelBuilder" />.
    /// </summary>
    public sealed class ManyToManyAssociationMappingConfiguration : AssociationMappingConfiguration
    {
        private readonly List<string> _leftKeyColumnNames = new List<string>();
        private readonly List<string> _rightKeyColumnNames = new List<string>();

        private DatabaseName _tableName;

        internal ManyToManyAssociationMappingConfiguration()
        {
        }

        private ManyToManyAssociationMappingConfiguration(ManyToManyAssociationMappingConfiguration source)
        {
            Contract.Requires(source != null);

            _leftKeyColumnNames.AddRange(source._leftKeyColumnNames);
            _rightKeyColumnNames.AddRange(source._rightKeyColumnNames);
            _tableName = source._tableName;
        }

        internal override AssociationMappingConfiguration Clone()
        {
            return new ManyToManyAssociationMappingConfiguration(this);
        }

        /// <summary>
        ///     Configures the join table name for the relationship.
        /// </summary>
        /// <param name = "tableName">Name of the table.</param>
        /// <returns>The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained.</returns>
        public ManyToManyAssociationMappingConfiguration ToTable(string tableName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(tableName));

            return ToTable(tableName, null);
        }

        /// <summary>
        ///     Configures the join table name and schema for the relationship.
        /// </summary>
        /// <param name = "tableName">Name of the table.</param>
        /// <param name = "schemaName">Schema of the table.</param>
        /// <returns>The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained.</returns>
        public ManyToManyAssociationMappingConfiguration ToTable(string tableName, string schemaName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(tableName));

            _tableName = new DatabaseName(tableName, schemaName);

            return this;
        }

        /// <summary>
        ///     Configures the name of the column(s) for the left foreign key.
        ///     The left foreign key represents the navigation property specified in the HasMany call.
        /// </summary>
        /// <param name = "keyColumnNames">
        ///     The foreign key column names.
        ///     When using multiple foreign key properties, the properties must be specified in the same order that the
        ///     the primary key properties were configured for the target entity type.
        /// </param>
        /// <returns>The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained.</returns>
        public ManyToManyAssociationMappingConfiguration MapLeftKey(params string[] keyColumnNames)
        {
            Contract.Requires(keyColumnNames != null);

            _leftKeyColumnNames.Clear();
            _leftKeyColumnNames.AddRange(keyColumnNames);

            return this;
        }

        /// <summary>
        ///     Configures the name of the column(s) for the right foreign key.
        ///     The right foreign key represents the navigation property specified in the WithMany call.
        /// </summary>
        /// <param name = "keyColumnNames">
        ///     The foreign key column names.
        ///     When using multiple foreign key properties, the properties must be specified in the same order that the
        ///     the primary key properties were configured for the target entity type.
        /// </param>
        /// <returns>The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained.</returns>
        public ManyToManyAssociationMappingConfiguration MapRightKey(params string[] keyColumnNames)
        {
            Contract.Requires(keyColumnNames != null);

            _rightKeyColumnNames.Clear();
            _rightKeyColumnNames.AddRange(keyColumnNames);

            return this;
        }

        internal override void Configure(DbAssociationSetMapping associationSetMapping, DbDatabaseMetadata database)
        {
            var table = associationSetMapping.Table;

            if (_tableName != null)
            {
                table.SetTableName(_tableName);
                table.SetConfiguration(this);
            }

            ConfigureColumnNames(_leftKeyColumnNames, associationSetMapping.SourceEndMapping.PropertyMappings);
            ConfigureColumnNames(_rightKeyColumnNames, associationSetMapping.TargetEndMapping.PropertyMappings);
        }

        private static void ConfigureColumnNames(
            ICollection<string> keyColumnNames, IList<DbEdmPropertyMapping> propertyMappings)
        {
            Contract.Requires(keyColumnNames != null);
            Contract.Requires(propertyMappings != null);

            if ((keyColumnNames.Count > 0)
                && (keyColumnNames.Count != propertyMappings.Count))
            {
                throw Error.IncorrectColumnCount(string.Join(", ", keyColumnNames));
            }

            keyColumnNames.Each((n, i) => propertyMappings[i].Column.Name = n);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Equals(ManyToManyAssociationMappingConfiguration other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (!Equals(other._tableName, _tableName))
            {
                return false;
            }

            if (_leftKeyColumnNames.SequenceEqual(other._leftKeyColumnNames)
                && _rightKeyColumnNames.SequenceEqual(other._rightKeyColumnNames))
            {
                return true;
            }

            if (_leftKeyColumnNames.SequenceEqual(other._rightKeyColumnNames)
                && _rightKeyColumnNames.SequenceEqual(other._leftKeyColumnNames))
            {
                return true;
            }

            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType()
                != typeof(ManyToManyAssociationMappingConfiguration))
            {
                return false;
            }

            return Equals((ManyToManyAssociationMappingConfiguration)obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_tableName != null ? _tableName.GetHashCode() : 0) * 397)
                       ^ _leftKeyColumnNames.Union(_rightKeyColumnNames)
                             .Aggregate(0, (t, n) => t + n.GetHashCode());
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
