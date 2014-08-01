// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class TphColumnFixer
    {
        private readonly IList<ColumnMappingBuilder> _columnMappings;
        private readonly EntityType _table;
        private readonly EdmModel _storeModel;

        public TphColumnFixer(IEnumerable<ColumnMappingBuilder> columnMappings, EntityType table, EdmModel storeModel)
        {
            DebugCheck.NotNull(columnMappings);
            DebugCheck.NotNull(table);
            DebugCheck.NotNull(storeModel);

            _columnMappings = columnMappings.OrderBy(m => m.ColumnProperty.Name).ToList();
            _table = table;
            _storeModel = storeModel;
        }

        public void RemoveDuplicateTphColumns()
        {
            for (var i = 0; i < _columnMappings.Count - 1;)
            {
                var entityType = _columnMappings[i].PropertyPath[0].DeclaringType;
                var column = _columnMappings[i].ColumnProperty;

                var indexAfterLastDuplicate = i + 1;
                EdmType _;
                while (indexAfterLastDuplicate < _columnMappings.Count
                       && column.Name == _columnMappings[indexAfterLastDuplicate].ColumnProperty.Name
                       && entityType != _columnMappings[indexAfterLastDuplicate].PropertyPath[0].DeclaringType
                       && TypeSemantics.TryGetCommonBaseType(
                           entityType, _columnMappings[indexAfterLastDuplicate].PropertyPath[0].DeclaringType, out _))
                {
                    indexAfterLastDuplicate++;
                }

                var columnConfig = column.GetConfiguration() as Properties.Primitive.PrimitivePropertyConfiguration;

                for (var toChangeIndex = i + 1; toChangeIndex < indexAfterLastDuplicate; toChangeIndex++)
                {
                    var toFixup = _columnMappings[toChangeIndex];
                    var toChangeConfig = toFixup.ColumnProperty.GetConfiguration() as Properties.Primitive.PrimitivePropertyConfiguration;

                    string configError;
                    if (columnConfig == null
                        || columnConfig.IsCompatible(toChangeConfig, inCSpace: false, errorMessage: out configError))
                    {
                        if (toChangeConfig != null)
                        {
                            toChangeConfig.Configure(column, _table, _storeModel.ProviderManifest);
                        }
                    }
                    else
                    {
                        throw new MappingException(
                            Strings.BadTphMappingToSharedColumn(
                                string.Join(".", _columnMappings[i].PropertyPath.Select(p => p.Name)),
                                entityType.Name,
                                string.Join(".", toFixup.PropertyPath.Select(p => p.Name)),
                                toFixup.PropertyPath[0].DeclaringType.Name,
                                column.Name,
                                column.DeclaringType.Name,
                                configError));
                    }

                    column.Nullable = true;

                    var associations = from a in _storeModel.AssociationTypes
                        where a.Constraint != null
                        let p = a.Constraint.ToProperties
                        where p.Contains(column) || p.Contains(toFixup.ColumnProperty)
                        select a;

                    foreach (var association in associations.ToArray())
                    {
                        _storeModel.RemoveAssociationType(association);
                    }

                    if (toFixup.ColumnProperty.DeclaringType.HasMember(toFixup.ColumnProperty))
                    {
                        toFixup.ColumnProperty.DeclaringType.RemoveMember(toFixup.ColumnProperty);
                    }
                    toFixup.ColumnProperty = column;
                }

                i = indexAfterLastDuplicate;
            }
        }
    }
}
