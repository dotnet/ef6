// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Configures the table and column mapping of a relationship that does not expose foreign key properties in the object model.
    /// This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
    /// </summary>
    public sealed class ForeignKeyAssociationMappingConfiguration : AssociationMappingConfiguration
    {
        private readonly List<string> _keyColumnNames = new List<string>();
        private readonly IDictionary<Tuple<string, string>, object> _annotations = new Dictionary<Tuple<string, string>, object>();

        private DatabaseName _tableName;

        internal ForeignKeyAssociationMappingConfiguration()
        {
        }

        private ForeignKeyAssociationMappingConfiguration(ForeignKeyAssociationMappingConfiguration source)
        {
            DebugCheck.NotNull(source);

            _keyColumnNames.AddRange(source._keyColumnNames);
            _tableName = source._tableName;

            foreach (var annotation in source._annotations)
            {
                _annotations.Add(annotation);
            }
        }

        internal override AssociationMappingConfiguration Clone()
        {
            return new ForeignKeyAssociationMappingConfiguration(this);
        }

        /// <summary>
        /// Configures the name of the column(s) for the foreign key.
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
        /// Sets an annotation in the model for a database column that has been configured with <see cref="MapKey"/>.
        /// The annotation value can later be used when processing the column such as when creating migrations.
        /// </summary>
        /// <remarks>
        /// It will likely be necessary to register a <see cref="IMetadataAnnotationSerializer"/> if the type of
        /// the annotation value is anything other than a string. Passing a null value clears any annotation with
        /// the given name on the column that had been previously set.
        /// </remarks>
        /// <param name="keyColumnName">The name of the column that was configured with the HasKey method.</param>
        /// <param name="annotationName">The annotation name, which must be a valid C#/EDM identifier.</param>
        /// <param name="value">The annotation value, which may be a string or some other type that
        /// can be serialized with an <see cref="IMetadataAnnotationSerializer"/></param>.
        /// <returns>The same ForeignKeyAssociationMappingConfiguration instance so that multiple calls can be chained.</returns>
        public ForeignKeyAssociationMappingConfiguration HasKeyAnnotation(string keyColumnName, string annotationName, object value)
        {
            Check.NotEmpty(keyColumnName, "keyColumnName");
            Check.NotEmpty(annotationName, "annotationName");

            _annotations[Tuple.Create(keyColumnName, annotationName)] = value;

            return this;
        }

        /// <summary>
        /// Configures the table name that the foreign key column(s) reside in.
        /// The table that is specified must already be mapped for the entity type.
        /// If you want the foreign key(s) to reside in their own table then use the Map method
        /// on <see cref="T:System.Data.Entity.ModelConfiguration.EntityTypeConfiguration" /> to perform
        /// entity splitting to create the table with just the primary key property. Foreign keys can
        /// then be added to the table via this method.
        /// </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <returns> The same ForeignKeyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ForeignKeyAssociationMappingConfiguration ToTable(string tableName)
        {
            Check.NotEmpty(tableName, "tableName");

            return ToTable(tableName, null);
        }

        /// <summary>
        /// Configures the table name and schema that the foreign key column(s) reside in.
        /// The table that is specified must already be mapped for the entity type.
        /// If you want the foreign key(s) to reside in their own table then use the Map method
        /// on <see cref="T:System.Data.Entity.ModelConfiguration.EntityTypeConfiguration" /> to perform
        /// entity splitting to create the table with just the primary key property. Foreign keys can
        /// then be added to the table via this method.
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

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override void Configure(
            AssociationSetMapping associationSetMapping, EdmModel database, PropertyInfo navigationProperty)
        {
            DebugCheck.NotNull(associationSetMapping);

            DebugCheck.NotNull(navigationProperty);

            // By convention source end contains the dependent column mappings
            var propertyMappings = associationSetMapping.SourceEndMapping.Properties.ToList();

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
                                     .Single(fk => fk.DependentColumns.SequenceEqual(propertyMappings.Select(pm => pm.Column)));

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

            _keyColumnNames.Each((n, i) => propertyMappings[i].Column.Name = n);

            foreach (var annotation in _annotations)
            {
                var index = _keyColumnNames.IndexOf(annotation.Key.Item1);

                if (index == -1)
                {
                    throw new InvalidOperationException(Strings.BadKeyNameForAnnotation(annotation.Key.Item1, annotation.Key.Item2));
                }

                propertyMappings[index].Column.AddAnnotation(
                    XmlConstants.CustomAnnotationNamespace + ":" + annotation.Key.Item2,
                    annotation.Value);
            }
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
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

            return Equals(other._tableName, _tableName)
                   && other._keyColumnNames.SequenceEqual(_keyColumnNames)
                   && other._annotations.OrderBy(a => a.Key).SequenceEqual(_annotations.OrderBy(a => a.Key));
        }

        /// <inheritdoc />
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

            if (obj.GetType() != typeof(ForeignKeyAssociationMappingConfiguration))
            {
                return false;
            }

            return Equals((ForeignKeyAssociationMappingConfiguration)obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_tableName != null ? _tableName.GetHashCode() : 0) * 397;
                hashCode = _keyColumnNames.Aggregate(hashCode, (h, v) => (h * 397) ^ v.GetHashCode());
                return _annotations.OrderBy(a => a.Key).Aggregate(hashCode, (h, v) => (h * 397) ^ v.GetHashCode());
            }
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
