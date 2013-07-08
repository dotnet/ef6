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
    ///     Configures the table and column mapping of a many:many relationship.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref="DbModelBuilder" />.
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
            DebugCheck.NotNull(source);

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
        /// <param name="tableName"> Name of the table. </param>
        /// <returns> The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ManyToManyAssociationMappingConfiguration ToTable(string tableName)
        {
            Check.NotEmpty(tableName, "tableName");

            return ToTable(tableName, null);
        }

        /// <summary>
        ///     Configures the join table name and schema for the relationship.
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
        ///     Configures the name of the column(s) for the left foreign key.
        ///     The left foreign key points to the parent entity of the navigation property specified in the HasMany call.
        /// </summary>
        /// <param name="keyColumnNames"> The foreign key column names. When using multiple foreign key properties, the properties must be specified in the same order that the the primary key properties were configured for the target entity type. </param>
        /// <returns> The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ManyToManyAssociationMappingConfiguration MapLeftKey(params string[] keyColumnNames)
        {
            Check.NotNull(keyColumnNames, "keyColumnNames");

            _leftKeyColumnNames.Clear();
            _leftKeyColumnNames.AddRange(keyColumnNames);

            return this;
        }

        /// <summary>
        ///     Configures the name of the column(s) for the right foreign key.
        ///     The right foreign key points to the parent entity of the the navigation property specified in the WithMany call.
        /// </summary>
        /// <param name="keyColumnNames"> The foreign key column names. When using multiple foreign key properties, the properties must be specified in the same order that the the primary key properties were configured for the target entity type. </param>
        /// <returns> The same ManyToManyAssociationMappingConfiguration instance so that multiple calls can be chained. </returns>
        public ManyToManyAssociationMappingConfiguration MapRightKey(params string[] keyColumnNames)
        {
            Check.NotNull(keyColumnNames, "keyColumnNames");

            _rightKeyColumnNames.Clear();
            _rightKeyColumnNames.AddRange(keyColumnNames);

            return this;
        }

        internal override void Configure(
            StorageAssociationSetMapping associationSetMapping, EdmModel database, PropertyInfo navigationProperty)
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
                    associationSetMapping.SourceEndMapping.EndMember.GetClrPropertyInfo());

            ConfigureColumnNames(
                sourceEndIsPrimaryConfiguration ? _leftKeyColumnNames : _rightKeyColumnNames,
                associationSetMapping.SourceEndMapping.PropertyMappings.ToList());

            ConfigureColumnNames(
                sourceEndIsPrimaryConfiguration ? _rightKeyColumnNames : _leftKeyColumnNames,
                associationSetMapping.TargetEndMapping.PropertyMappings.ToList());
        }

        private static void ConfigureColumnNames(
            ICollection<string> keyColumnNames, IList<StorageScalarPropertyMapping> propertyMappings)
        {
            DebugCheck.NotNull(keyColumnNames);
            DebugCheck.NotNull(propertyMappings);

            if ((keyColumnNames.Count > 0)
                && (keyColumnNames.Count != propertyMappings.Count))
            {
                throw Error.IncorrectColumnCount(string.Join(", ", keyColumnNames));
            }

            keyColumnNames.Each((n, i) => propertyMappings[i].ColumnProperty.Name = n);
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

            if (obj.GetType()
                != typeof(ManyToManyAssociationMappingConfiguration))
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
                return ((_tableName != null ? _tableName.GetHashCode() : 0) * 397)
                       ^ _leftKeyColumnNames.Union(_rightKeyColumnNames)
                                            .Aggregate(0, (t, n) => t + n.GetHashCode());
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
