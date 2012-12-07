// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Configures the table and column mapping of a relationship that does not expose foreign key properties in the object model.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
    /// </summary>
    public sealed class ForeignKeyAssociationMappingConfiguration : AssociationMappingConfiguration
    {
        private readonly List<string> _keyColumnNames = new List<string>();

        private DatabaseName _tableName;

        internal ForeignKeyAssociationMappingConfiguration()
        {
        }

        private ForeignKeyAssociationMappingConfiguration(ForeignKeyAssociationMappingConfiguration source)
        {
            DebugCheck.NotNull(source);

            _keyColumnNames.AddRange(source._keyColumnNames);
            _tableName = source._tableName;
        }

        internal override AssociationMappingConfiguration Clone()
        {
            return new ForeignKeyAssociationMappingConfiguration(this);
        }

        /// <summary>
        ///     Configures the name of the column(s) for the foreign key.
        /// </summary>
        /// <param name="keyColumnNames"> The foreign key column names. When using multiple foreign key properties, the properties must be specified in the same order that the the primary key properties were configured for the target entity type. </param>
        /// <returns> The same ForeignKeyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ForeignKeyAssociationMappingConfiguration MapKey(params string[] keyColumnNames)
        {
            Check.NotNull(keyColumnNames, "keyColumnNames");

            _keyColumnNames.Clear();
            _keyColumnNames.AddRange(keyColumnNames);

            return this;
        }

        /// <summary>
        ///     Configures the table name that the foreign key column(s) reside in.
        ///     The table that is specified must already be mapped for the entity type.
        ///     If you want the foreign key(s) to reside in their own table then use the Map method
        ///     on <see cref="T:System.Data.Entity.ModelConfiguration.EntityTypeConfiguration" /> to perform
        ///     entity splitting to create the table with just the primary key property. Foreign keys can
        ///     then be added to the table via this method.
        /// </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <returns> The same ForeignKeyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ForeignKeyAssociationMappingConfiguration ToTable(string tableName)
        {
            Check.NotEmpty(tableName, "tableName");

            return ToTable(tableName, null);
        }

        /// <summary>
        ///     Configures the table name and schema that the foreign key column(s) reside in.
        ///     The table that is specified must already be mapped for the entity type.
        ///     If you want the foreign key(s) to reside in their own table then use the Map method
        ///     on <see cref="T:System.Data.Entity.ModelConfiguration.EntityTypeConfiguration" /> to perform
        ///     entity splitting to create the table with just the primary key property. Foreign keys can
        ///     then be added to the table via this method.
        /// </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <param name="schemaName"> Schema of the table. </param>
        /// <returns> The same ForeignKeyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ForeignKeyAssociationMappingConfiguration ToTable(string tableName, string schemaName)
        {
            Check.NotEmpty(tableName, "tableName");

            _tableName = new DatabaseName(tableName, schemaName);

            return this;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override void Configure(
            StorageAssociationSetMapping associationSetMapping, EdmModel database, PropertyInfo navigationProperty)
        {
            DebugCheck.NotNull(associationSetMapping);
            
            DebugCheck.NotNull(navigationProperty);

            // By convention source end contains the dependent column mappings
            var propertyMappings = associationSetMapping.SourceEndMapping.PropertyMappings.ToList();

            if (_tableName != null)
            {
                var targetTable
                    = ((from t in database.EntityTypes
                        let n = t.GetTableName()
                        where (n != null && n.Equals(_tableName))
                        select t)
                          .SingleOrDefault())
                      ?? (from es in database.GetEntitySets()
                          where string.Equals(es.Table, _tableName.Name, StringComparison.Ordinal)
                          select es.ElementType).SingleOrDefault();

                if (targetTable == null)
                {
                    throw Error.TableNotFound(_tableName);
                }

                var sourceTable = associationSetMapping.Table;

                if (sourceTable != targetTable)
                {
                    var foreignKeyConstraint
                        = sourceTable.ForeignKeyBuilders
                                     .Single(fk => fk.DependentColumns.SequenceEqual(propertyMappings.Select(pm => pm.ColumnProperty)));

                    sourceTable.RemoveForeignKey(foreignKeyConstraint);
                    targetTable.AddForeignKey(foreignKeyConstraint);

                    foreignKeyConstraint.DependentColumns
                                        .Each(
                                            c =>
                                                {
                                                    var isKey = c.IsPrimaryKeyColumn;

                                                    sourceTable.RemoveMember(c);
                                                    targetTable.AddMember(c);

                                                    if (isKey)
                                                    {
                                                        targetTable.AddKeyMember(c);
                                                    }
                                                });

                    associationSetMapping.StoreEntitySet = database.GetEntitySet(targetTable);
                }
            }

            if ((_keyColumnNames.Count > 0)
                && (_keyColumnNames.Count != propertyMappings.Count()))
            {
                throw Error.IncorrectColumnCount(string.Join(", ", _keyColumnNames));
            }

            _keyColumnNames.Each((n, i) => propertyMappings[i].ColumnProperty.Name = n);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool Equals(ForeignKeyAssociationMappingConfiguration other)
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

            return other._keyColumnNames.SequenceEqual(_keyColumnNames);
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
                != typeof(ForeignKeyAssociationMappingConfiguration))
            {
                return false;
            }

            return Equals((ForeignKeyAssociationMappingConfiguration)obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_tableName != null ? _tableName.GetHashCode() : 0) * 397)
                       ^ _keyColumnNames.Aggregate(0, (t, n) => t + n.GetHashCode());
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
