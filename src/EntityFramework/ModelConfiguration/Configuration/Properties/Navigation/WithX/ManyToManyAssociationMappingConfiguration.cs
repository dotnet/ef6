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
    /// Configures the table and column mapping of a many:many relationship.
    /// This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
    /// </summary>
    public sealed class ManyToManyAssociationMappingConfiguration : AssociationMappingConfiguration
    {
        private readonly List<string> _leftKeyColumnNames = new List<string>();
        private readonly List<string> _rightKeyColumnNames = new List<string>();

        private DatabaseName _tableName;

        private readonly IDictionary<string, object> _annotations = new Dictionary<string, object>();

        internal ManyToManyAssociationMappingConfiguration()
        {
        }

        private ManyToManyAssociationMappingConfiguration(ManyToManyAssociationMappingConfiguration source)
        {
            DebugCheck.NotNull(source);

            _leftKeyColumnNames.AddRange(source._leftKeyColumnNames);
            _rightKeyColumnNames.AddRange(source._rightKeyColumnNames);
            _tableName = source._tableName;

            foreach (var annotation in source._annotations)
            {
                _annotations.Add(annotation);
            }
        }

        internal override AssociationMappingConfiguration Clone()
        {
            return new ManyToManyAssociationMappingConfiguration(this);
        }

        /// <summary>
        /// Configures the join table name for the relationship.
        /// </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <returns> The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ManyToManyAssociationMappingConfiguration ToTable(string tableName)
        {
            Check.NotEmpty(tableName, "tableName");

            return ToTable(tableName, null);
        }

        /// <summary>
        /// Configures the join table name and schema for the relationship.
        /// </summary>
        /// <param name="tableName"> Name of the table. </param>
        /// <param name="schemaName"> Schema of the table. </param>
        /// <returns> The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ManyToManyAssociationMappingConfiguration ToTable(string tableName, string schemaName)
        {
            Check.NotEmpty(tableName, "tableName");

            _tableName = new DatabaseName(tableName, schemaName);

            return this;
        }

        /// <summary>
        /// Sets an annotation in the model for the join table. The annotation value can later be used when
        /// processing the table such as when creating migrations.
        /// </summary>
        /// <remarks>
        /// It will likely be necessary to register a <see cref="IMetadataAnnotationSerializer"/> if the type of
        /// the annotation value is anything other than a string. Passing a null value clears any annotation with
        /// the given name on the column that had been previously set.
        /// </remarks>
        /// <param name="name">The annotation name, which must be a valid C#/EDM identifier.</param>
        /// <param name="value">The annotation value, which may be a string or some other type that
        /// can be serialized with an <see cref="IMetadataAnnotationSerializer"/></param>.
        /// <returns>The same configuration instance so that multiple calls can be chained.</returns>
        public ManyToManyAssociationMappingConfiguration HasTableAnnotation(string name, object value)
        {
            Check.NotEmpty(name, "name");

            // Technically we could accept some names that are invalid in EDM, but this is not too restrictive
            // and is an easy way of ensuring that name is valid all places we want to use it--i.e. in the XML
            // and in the MetadataWorkspace.
            if (!name.IsValidUndottedName())
            {
                throw new ArgumentException(Strings.BadAnnotationName(name));
            }

            _annotations[name] = value;

            return this;
        }

        /// <summary>
        /// Configures the name of the column(s) for the left foreign key.
        /// The left foreign key points to the parent entity of the navigation property specified in the HasMany call.
        /// </summary>
        /// <param name="keyColumnNames"> The foreign key column names. When using multiple foreign key properties, the properties must be specified in the same order that the primary key properties were configured for the target entity type. </param>
        /// <returns> The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ManyToManyAssociationMappingConfiguration MapLeftKey(params string[] keyColumnNames)
        {
            Check.NotNull(keyColumnNames, "keyColumnNames");

            _leftKeyColumnNames.Clear();
            _leftKeyColumnNames.AddRange(keyColumnNames);

            return this;
        }

        /// <summary>
        /// Configures the name of the column(s) for the right foreign key.
        /// The right foreign key points to the parent entity of the navigation property specified in the WithMany call.
        /// </summary>
        /// <param name="keyColumnNames"> The foreign key column names. When using multiple foreign key properties, the properties must be specified in the same order that the primary key properties were configured for the target entity type. </param>
        /// <returns> The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ManyToManyAssociationMappingConfiguration MapRightKey(params string[] keyColumnNames)
        {
            Check.NotNull(keyColumnNames, "keyColumnNames");

            _rightKeyColumnNames.Clear();
            _rightKeyColumnNames.AddRange(keyColumnNames);

            return this;
        }

        internal override void Configure(
            AssociationSetMapping associationSetMapping, EdmModel database, PropertyInfo navigationProperty)
        {
            DebugCheck.NotNull(associationSetMapping);
            DebugCheck.NotNull(database);
            DebugCheck.NotNull(navigationProperty);

            var table = associationSetMapping.Table;

            if (_tableName != null)
            {
                table.SetTableName(_tableName);
                table.SetConfiguration(this);
            }

            var sourceEndIsPrimaryConfiguration
                = navigationProperty.IsSameAs(
                    associationSetMapping.SourceEndMapping.AssociationEnd.GetClrPropertyInfo());

            ConfigureColumnNames(
                sourceEndIsPrimaryConfiguration ? _leftKeyColumnNames : _rightKeyColumnNames,
                associationSetMapping.SourceEndMapping.PropertyMappings.ToList());

            ConfigureColumnNames(
                sourceEndIsPrimaryConfiguration ? _rightKeyColumnNames : _leftKeyColumnNames,
                associationSetMapping.TargetEndMapping.PropertyMappings.ToList());

            foreach (var annotation in _annotations)
            {
                table.AddAnnotation(XmlConstants.CustomAnnotationPrefix + annotation.Key, annotation.Value);
            }
        }

        private static void ConfigureColumnNames(
            ICollection<string> keyColumnNames, IList<ScalarPropertyMapping> propertyMappings)
        {
            DebugCheck.NotNull(keyColumnNames);
            DebugCheck.NotNull(propertyMappings);

            if ((keyColumnNames.Count > 0)
                && (keyColumnNames.Count != propertyMappings.Count))
            {
                throw Error.IncorrectColumnCount(string.Join(", ", keyColumnNames));
            }

            keyColumnNames.Each((n, i) => propertyMappings[i].Column.Name = n);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        /// <param name="other">The object to compare with the current object.</param>
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

            return Equals(other._tableName, _tableName)
                   && ((_leftKeyColumnNames.SequenceEqual(other._leftKeyColumnNames)
                        && _rightKeyColumnNames.SequenceEqual(other._rightKeyColumnNames))
                       || (_leftKeyColumnNames.SequenceEqual(other._rightKeyColumnNames)
                           && _rightKeyColumnNames.SequenceEqual(other._leftKeyColumnNames)))
                   && _annotations.OrderBy(a => a.Key).SequenceEqual(other._annotations.OrderBy(a => a.Key));
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

            if (obj.GetType() != typeof(ManyToManyAssociationMappingConfiguration))
            {
                return false;
            }

            return Equals((ManyToManyAssociationMappingConfiguration)obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_tableName != null ? _tableName.GetHashCode() : 0) * 397;
                hashCode = _leftKeyColumnNames.Aggregate(hashCode, (h, v) => (h * 397) ^ v.GetHashCode());
                hashCode = _rightKeyColumnNames.Aggregate(hashCode, (h, v) => (h * 397) ^ v.GetHashCode());
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
