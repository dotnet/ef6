// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery;

    internal class StoreModelBuilder
    {
        private const string TypeAttributeName = "Type";

        // values of Type attribute
        private const string TablesTypeAttributeValue = "Tables";
        private const string ViewsTypeAttributeValue = "Views";

        private readonly Dictionary<string, PrimitiveType> _nameToEdmType;
        private readonly Version _targetEntityFrameworkVersion;
        private readonly string _namespaceName;
        private readonly string _providerInvariantName;
        private readonly string _providerManifestToken;
        private readonly IDbDependencyResolver _dependencyResolver;

        private readonly UniqueIdentifierService _usedTypeNames = new UniqueIdentifierService(
            StringComparer.OrdinalIgnoreCase, s => s.Replace(".", "_"));

        private readonly bool _generateForeignKeyProperties;

        public StoreModelBuilder(
            string providerInvariantName,
            string providerManifestToken,
            Version targetEntityFrameworkVersion,
            string namespaceName,
            IDbDependencyResolver resolver,
            bool generateForeignKeyProperties)
        {
            Debug.Assert(
                !string.IsNullOrWhiteSpace(providerInvariantName),
                "!string.IsNullOrWhiteSpace(providerInvariantName");
            Debug.Assert(
                !string.IsNullOrWhiteSpace(providerManifestToken),
                "!string.IsNullOrWhiteSpace(providerManifestToken");
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(targetEntityFrameworkVersion), "invalid entityFrameworkVersion");
            Debug.Assert(!string.IsNullOrWhiteSpace(namespaceName), "!string.IsNullOrWhiteSpace(namespaceName)");
            Debug.Assert(resolver != null, "resolver != null");

            _providerInvariantName = providerInvariantName;
            _providerManifestToken = providerManifestToken;
            _targetEntityFrameworkVersion = targetEntityFrameworkVersion;
            _namespaceName = namespaceName;
            _dependencyResolver = resolver;
            _generateForeignKeyProperties = generateForeignKeyProperties;

            var providerServices = resolver.GetService<DbProviderServices>(providerInvariantName);
            Debug.Assert(providerServices != null, "providerServices != null");

            _nameToEdmType = providerServices.GetProviderManifest(providerManifestToken)
                .GetStoreTypes()
                .ToDictionary(t => t.Name, t => t);
        }

        public EdmModel Build(StoreSchemaDetails storeSchemaDetails)
        {
            Debug.Assert(storeSchemaDetails != null, "storeSchemaDetails != null");

            var entityRegister = new EntityRegister();
            CreateEntitySets(
                storeSchemaDetails.TableDetails,
                storeSchemaDetails.ViewDetails,
                entityRegister);

            var associationTypes = new List<AssociationType>();
            var associationSets =
                CreateAssociationSets(
                    storeSchemaDetails.RelationshipDetails,
                    entityRegister,
                    associationTypes);

            var entityContainer =
                EntityContainer.Create(
                    _namespaceName.Replace(".", string.Empty) + "Container",
                    DataSpace.SSpace,
                    entityRegister.EntitySets.Union<EntitySetBase>(associationSets),
                    null,
                    null);

            var storeModel =
                EdmModel.CreateStoreModel(
                    entityContainer,
                    new DbProviderInfo(_providerInvariantName, _providerManifestToken),
                    null,
                    EntityFrameworkVersion.VersionToDouble(_targetEntityFrameworkVersion));

            foreach (var entityType in entityRegister.EntityTypes)
            {
                storeModel.AddItem(entityType);
            }

            foreach (var associationType in associationTypes)
            {
                storeModel.AddItem(associationType);
            }

            var functions = CreateFunctions(storeSchemaDetails.FunctionDetails, storeSchemaDetails.TVFReturnTypeDetails);
            foreach (var function in functions)
            {
                storeModel.AddItem(function);
            }

            return storeModel;
        }

        // internal for testing
        internal void CreateEntitySets(
            IEnumerable<TableDetailsRow> tableDetailsRowsForTables,
            IEnumerable<TableDetailsRow> tableDetailsRowsForViews,
            EntityRegister entityRegister)
        {
            Debug.Assert(tableDetailsRowsForTables != null, "tableDetailsRowsForTables != null");
            Debug.Assert(tableDetailsRowsForViews != null, "tableDetailsRowsForViews != null");
            Debug.Assert(entityRegister != null, "entityRegister != null");

            var entitySetsForReadOnlyEntityTypes = new List<EntitySet>();

            CreateEntitySets(tableDetailsRowsForTables, entityRegister, entitySetsForReadOnlyEntityTypes, DbObjectType.Table);

            CreateEntitySets(tableDetailsRowsForViews, entityRegister, entitySetsForReadOnlyEntityTypes, DbObjectType.View);

            if (entitySetsForReadOnlyEntityTypes.Any())
            {
                // readonly entity sets need to be rewritten so that they 
                // contain provider specific SQL query to retrieve the data
                entityRegister.AddEntitySets(
                    EntitySetDefiningQueryConverter.Convert(
                        entitySetsForReadOnlyEntityTypes,
                        _targetEntityFrameworkVersion,
                        _providerInvariantName,
                        _providerManifestToken,
                        _dependencyResolver));
            }
        }

        // internal for testing
        internal void CreateEntitySets(
            IEnumerable<TableDetailsRow> tableDetailsRows,
            EntityRegister entityRegister,
            IList<EntitySet> entitySetsForReadOnlyEntityTypes,
            DbObjectType objectType)
        {
            Debug.Assert(tableDetailsRows != null, "tableDetailsRows != null");
            Debug.Assert(entityRegister != null, "entityRegister != null");
            Debug.Assert(entitySetsForReadOnlyEntityTypes != null, "entitySetsForReadOnlyEntityTypes != null");
            Debug.Assert(
                objectType == DbObjectType.Table || objectType == DbObjectType.View,
                "Unexpected object type - only tables and views are supported");

            foreach (var tableDetailsRowsForTable in SplitRows(tableDetailsRows))
            {
                var firstRow = tableDetailsRowsForTable[0];

                bool needsDefiningQuery;
                var entityType = CreateEntityType(tableDetailsRowsForTable, out needsDefiningQuery);
                entityRegister.AddEntityType(firstRow.GetMostQualifiedTableName(), entityType);

                // skip EntitySet creation for invalid entity types. We still need the types themselves - they
                // will be written to the ssdl in comments for informational and debugging purposes.
                if (!MetadataItemHelper.IsInvalid(entityType))
                {
                    var entitySet =
                        EntitySet.Create(
                            entityType.Name,
                            !firstRow.IsSchemaNull() ? firstRow.Schema : null,
                            firstRow.TableName != entityType.Name ? firstRow.TableName : null,
                            null,
                            entityType,
                            new[]
                                {
                                    CreateStoreModelBuilderMetadataProperty(
                                        TypeAttributeName,
                                        objectType == DbObjectType.Table
                                            ? TablesTypeAttributeValue
                                            : ViewsTypeAttributeValue)
                                });

                    if (needsDefiningQuery)
                    {
                        entitySetsForReadOnlyEntityTypes.Add(entitySet);
                    }
                    else
                    {
                        entityRegister.AddEntitySet(entitySet);
                    }
                }
            }
        }

        // internal for testing
        internal EntityType CreateEntityType(IList<TableDetailsRow> columns, out bool needsDefiningQuery)
        {
            Debug.Assert(columns.Count > 0, "Trying to create an EntityType with 0 properties");

            needsDefiningQuery = false;

            var entityTypeName = _usedTypeNames.AdjustIdentifier(columns[0].TableName);
            var tableName = columns[0].GetMostQualifiedTableName();

            var errors = new List<EdmSchemaError>();
            List<string> excludedColumnNames; // columns that were invalid and could not be converted to properties (e.g. of unknown type)
            List<string> keyColumnNames; // columns that were marked as key columns (including excluded/excluded columns)
            List<string> invalidKeyTypeColumnNames;
                // columns that could be converted to properties but cannot be key properties (e.g. of geography type)
            var properties = CreateProperties(columns, errors, out keyColumnNames, out excludedColumnNames, out invalidKeyTypeColumnNames);

            var excludedKeyColumnNames = keyColumnNames.Intersect(excludedColumnNames).ToArray();

            // excluded key columns found - a read only entity type will be created if there are any valid 
            // key columns remaining; otherwise an invalid entity (without key properties) will be created
            if (excludedKeyColumnNames.Any())
            {
                return
                    CreateEntityTypeWithExcludedKeyProperties(
                        entityTypeName, properties, keyColumnNames,
                        excludedKeyColumnNames, tableName, errors, out needsDefiningQuery);
            }

            // no key columns found - a read only entity type will be created if key columns can be inferred;
            // otherwise an invalid entity (without key properties) will be created
            if (!keyColumnNames.Any())
            {
                return
                    CreateEntityTypeWithoutDefinedKeys(
                        entityTypeName, properties, tableName, errors, out needsDefiningQuery);
            }

            // found key columns whose types are valid EF primitive types but not valid key property types (e.g. binary type in v1 or 
            // geospatial types) - a read only entity will be created if key columns can be inferred; // otherwise an invalid entity 
            // (without key properties) will be created
            if (invalidKeyTypeColumnNames.Any())
            {
                errors.Add(
                    new EdmSchemaError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.InvalidTypeForPrimaryKey,
                            tableName,
                            invalidKeyTypeColumnNames[0],
                            properties.First(p => p.Name == invalidKeyTypeColumnNames[0]).PrimitiveType.Name),
                        (int)ModelBuilderErrorCode.InvalidKeyTypeFound,
                        EdmSchemaErrorSeverity.Warning));

                return
                    CreateEntityTypeWithoutDefinedKeys(
                        entityTypeName, properties, tableName, errors, out needsDefiningQuery);
            }

            var metadataProperties = CreateMetadataProperties(false, errors);

            return CreateEntityType(entityTypeName, keyColumnNames, properties, metadataProperties);
        }

        private EntityType CreateEntityTypeWithExcludedKeyProperties(
            string name,
            IList<EdmProperty> properties,
            IEnumerable<string> keyColumnNames,
            string[] excludedKeyColumnNames,
            string tableName,
            IList<EdmSchemaError> errors,
            out bool needsDefiningQuery)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), "!string.IsNullOrEmpty(name)");
            Debug.Assert(properties != null && properties.Count > 0, "non-empty property list expected");
            Debug.Assert(
                excludedKeyColumnNames != null && excludedKeyColumnNames.Length > 0,
                "non empty array of excluded key columns expected");
            Debug.Assert(!string.IsNullOrEmpty(tableName), "!string.IsNullOrEmpty(tableName)");
            Debug.Assert(errors != null, "errors != null");

            var nonExcludedKeyColumnNames = keyColumnNames.Except(excludedKeyColumnNames).ToArray();

            var validKeyColumnNames =
                nonExcludedKeyColumnNames
                    .Where(
                        k =>
                        IsValidKeyType(
                            _targetEntityFrameworkVersion,
                            properties.First(p => p.Name == k).TypeUsage.EdmType)).ToArray();

            // the entity is read only if there is at least one valid key column and therefore will need the defining query
            // otherwise the entity is invalid and will not need a defining query since there will be no corresponding entity
            // set for this entity type and the type itself will be commented out in the SSDL
            needsDefiningQuery = validKeyColumnNames.Any();

            foreach (var excludedKeyColumnName in excludedKeyColumnNames)
            {
                errors.Add(
                    new EdmSchemaError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            needsDefiningQuery
                                ? Resources_VersioningFacade.ExcludedColumnWasAKeyColumnEntityIsReadOnly
                                : Resources_VersioningFacade.ExcludedColumnWasAKeyColumnEntityIsInvalid,
                            excludedKeyColumnName,
                            tableName),
                        (int)ModelBuilderErrorCode.ExcludedColumnWasAKeyColumn,
                        EdmSchemaErrorSeverity.Warning));
            }

            if (validKeyColumnNames.Length < nonExcludedKeyColumnNames.Length)
            {
                var invalidKeyTypeProperty =
                    properties.First(p => p.Name == nonExcludedKeyColumnNames.Except(validKeyColumnNames).First());

                errors.Add(
                    new EdmSchemaError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.InvalidTypeForPrimaryKey,
                            tableName,
                            invalidKeyTypeProperty.Name,
                            invalidKeyTypeProperty.PrimitiveType.Name),
                        (int)ModelBuilderErrorCode.InvalidKeyTypeFound,
                        EdmSchemaErrorSeverity.Warning));
            }

            var metadataProperties = CreateMetadataProperties(!needsDefiningQuery, errors);

            return CreateEntityType(name, validKeyColumnNames, properties, metadataProperties);
        }

        private EntityType CreateEntityTypeWithoutDefinedKeys(
            string name,
            IList<EdmProperty> properties,
            string tableName,
            IList<EdmSchemaError> errors,
            out bool needsDefiningQuery)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), "!string.IsNullOrEmpty(name)");
            Debug.Assert(properties != null && properties.Count > 0, "non-empty property list expected");
            Debug.Assert(!string.IsNullOrEmpty(tableName), "!string.IsNullOrEmpty(tableName)");
            Debug.Assert(errors != null, "errors != null");

            var inferredKeyProperties = InferKeyProperties(properties);

            // the entity type is read only if we could infer at least one valid key property and therefore will need the defining query
            // otherwise the entity is invalid and will not need a defining query since there will be no corresponding entity
            // set for this entity type and the type itself will be commented out in the SSDL
            needsDefiningQuery = inferredKeyProperties.Any();

            errors.Add(
                needsDefiningQuery
                    ? new EdmSchemaError(
                          string.Format(
                              CultureInfo.InvariantCulture,
                              Resources_VersioningFacade.NoPrimaryKeyDefined,
                              tableName),
                          (int)ModelBuilderErrorCode.NoPrimaryKeyDefined,
                          EdmSchemaErrorSeverity.Warning)
                    : new EdmSchemaError(
                          string.Format(
                              CultureInfo.InvariantCulture,
                              Resources_VersioningFacade.CannotCreateEntityWithNoPrimaryKeyDefined,
                              tableName),
                          (int)ModelBuilderErrorCode.CannotCreateEntityWithoutPrimaryKey,
                          EdmSchemaErrorSeverity.Warning));

            var metadataProperties = CreateMetadataProperties(!needsDefiningQuery, errors);

            return CreateEntityType(name, inferredKeyProperties, properties, metadataProperties);
        }

        private EntityType CreateEntityType(
            string name, IEnumerable<string> keyMemberNames,
            IEnumerable<EdmMember> members, IList<MetadataProperty> metadataProperties)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), "!string.IsNullOrEmpty(name)");

            return EntityType.Create(
                name,
                _namespaceName,
                DataSpace.SSpace,
                keyMemberNames,
                members,
                metadataProperties);
        }

        // internal to allow unit testing
        internal IList<EdmProperty> CreateProperties(
            IList<TableDetailsRow> columns,
            IList<EdmSchemaError> errors,
            out List<string> keyColumns,
            out List<string> excludedColumns,
            out List<string> invalidKeyTypeColumns)
        {
            Debug.Assert(columns.Count > 0, "columns.Count > 0");
            Debug.Assert(errors != null, "errors != null");

            var members = new List<EdmProperty>();
            excludedColumns = new List<string>();
            keyColumns = new List<string>();
            invalidKeyTypeColumns = new List<string>();
            foreach (var row in columns)
            {
                Debug.Assert(row.ColumnName != null);

                var property = CreateProperty(row, errors);
                if (property != null)
                {
                    members.Add(property);

                    if (row.IsPrimaryKey
                        && !IsValidKeyType(_targetEntityFrameworkVersion, property.TypeUsage.EdmType))
                    {
                        invalidKeyTypeColumns.Add(property.Name);
                    }
                }
                else
                {
                    excludedColumns.Add(row.ColumnName);
                }

                if (row.IsPrimaryKey)
                {
                    keyColumns.Add(row.ColumnName);
                }
            }

            return members;
        }

        // internal to allow unit testing
        internal EdmProperty CreateProperty(TableDetailsRow row, IList<EdmSchemaError> errors)
        {
            Debug.Assert(row != null, "row != null");
            Debug.Assert(errors != null, "errors != null");

            if (!ValidatePropertyType(row, errors))
            {
                return null;
            }

            return ConfigureProperty(
                EdmProperty.CreatePrimitive(row.ColumnName, _nameToEdmType[row.DataType]),
                row,
                errors);
        }

        private static EdmProperty ConfigureProperty(EdmProperty property, TableDetailsRow row, IList<EdmSchemaError> errors)
        {
            Debug.Assert(property != null, "property != null");
            Debug.Assert(row != null, "row != null");
            Debug.Assert(errors != null, "errors != null");

            switch (property.PrimitiveType.PrimitiveTypeKind)
            {
                case PrimitiveTypeKind.Decimal:
                    property = ConfigureDecimalProperty(property, row, errors);
                    break;

                case PrimitiveTypeKind.DateTime:
                case PrimitiveTypeKind.DateTimeOffset:
                case PrimitiveTypeKind.Time:
                    property = ConfigureDateTimeLikeProperty(property, row, errors);
                    break;

                case PrimitiveTypeKind.String:
                case PrimitiveTypeKind.Binary:
                    property = ConfigurePropertyWithMaxLength(property, row, errors);
                    break;
            }

            if (property != null)
            {
                if (row.IsPrimaryKey && row.IsNullable)
                {
                    errors.Add(
                        new EdmSchemaError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources_VersioningFacade.CoercingNullablePrimaryKeyPropertyToNonNullable,
                                row.ColumnName,
                                row.TableName),
                            (int)ModelBuilderErrorCode.CoercingNullablePrimaryKeyPropertyToNonNullable,
                            EdmSchemaErrorSeverity.Warning));
                }

                property.Nullable = row.IsPrimaryKey ? false : row.IsNullable;

                if (!row.IsIsIdentityNull()
                    && row.IsIdentity)
                {
                    property.StoreGeneratedPattern = StoreGeneratedPattern.Identity;
                }
                else if (!row.IsIsServerGeneratedNull()
                         && row.IsServerGenerated)
                {
                    property.StoreGeneratedPattern = StoreGeneratedPattern.Computed;
                }
            }

            return property;
        }

        private static EdmProperty ConfigureDecimalProperty(EdmProperty property, TableDetailsRow row, ICollection<EdmSchemaError> errors)
        {
            Debug.Assert(property != null, "property != null");
            Debug.Assert(property.PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Decimal, "decimal property expected");
            Debug.Assert(row != null, "row != null");
            Debug.Assert(errors != null, "errors != null");

            if (!row.IsPrecisionNull())
            {
                if (!ValidateFacetValueInRange(property.PrimitiveType, DbProviderManifest.PrecisionFacetName, row.Precision, row, errors))
                {
                    return null;
                }

                // it is OK to set even if facet is const - it will be ignored
                property.Precision = (byte)row.Precision;
            }

            if (!row.IsScaleNull())
            {
                if (!ValidateFacetValueInRange(property.PrimitiveType, DbProviderManifest.ScaleFacetName, row.Scale, row, errors))
                {
                    return null;
                }

                // it is OK to set even if facet is const - it will be ignored
                property.Scale = (byte)row.Scale;
            }

            return property;
        }

        private static EdmProperty ConfigureDateTimeLikeProperty(
            EdmProperty property, TableDetailsRow row, ICollection<EdmSchemaError> errors)
        {
            Debug.Assert(property != null, "property != null");
            Debug.Assert(
                property.PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Time ||
                property.PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.DateTime ||
                property.PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.DateTimeOffset,
                "datetime like property expected");
            Debug.Assert(row != null, "row != null");
            Debug.Assert(errors != null, "errors != null");

            if (!row.IsDateTimePrecisionNull())
            {
                if (
                    !ValidateFacetValueInRange(
                        property.PrimitiveType, DbProviderManifest.PrecisionFacetName, row.DateTimePrecision, row, errors))
                {
                    return null;
                }

                // it is OK to set even if facet is const - it will be ignored
                property.Precision = (byte)row.DateTimePrecision;
            }

            return property;
        }

        private static EdmProperty ConfigurePropertyWithMaxLength(
            EdmProperty property, TableDetailsRow row, ICollection<EdmSchemaError> errors)
        {
            Debug.Assert(property != null, "property != null");
            Debug.Assert(
                property.PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.String ||
                property.PrimitiveType.PrimitiveTypeKind == PrimitiveTypeKind.Binary,
                "string or binary property expected");
            Debug.Assert(row != null, "row != null");
            Debug.Assert(errors != null, "errors != null");

            if (!row.IsMaximumLengthNull())
            {
                if (
                    !ValidateFacetValueInRange(
                        property.PrimitiveType, DbProviderManifest.MaxLengthFacetName, row.MaximumLength, row, errors))
                {
                    return null;
                }

                // it is OK to set even if facet is const - it will be ignored
                property.MaxLength = row.MaximumLength;
            }

            return property;
        }

        private static bool ValidateFacetValueInRange(
            PrimitiveType storePrimitiveType, string facetName, int actualValue, TableDetailsRow row, ICollection<EdmSchemaError> errors)
        {
            Debug.Assert(storePrimitiveType != null, "storePrimitiveType != null");
            Debug.Assert(facetName != null, "facetName != null");
            Debug.Assert(row != null, "row != null");
            Debug.Assert(errors != null, "errors != null");

            var facetDescription = storePrimitiveType.FacetDescriptions.SingleOrDefault(f => f.FacetName == facetName);

            if (facetDescription != null
                && !facetDescription.IsConstant)
            {
                if (actualValue < facetDescription.MinValue
                    || actualValue > facetDescription.MaxValue)
                {
                    errors.Add(
                        new EdmSchemaError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources_VersioningFacade.ColumnFacetValueOutOfRange,
                                facetName,
                                actualValue,
                                facetDescription.MinValue,
                                facetDescription.MaxValue,
                                row.ColumnName,
                                row.GetMostQualifiedTableName()),
                            (int)ModelBuilderErrorCode.FacetValueOutOfRange,
                            EdmSchemaErrorSeverity.Warning));

                    return false;
                }
            }

            return true;
        }

        private bool ValidatePropertyType(TableDetailsRow row, ICollection<EdmSchemaError> errors)
        {
            string errorMessage = null;

            if (row.IsDataTypeNull())
            {
                errorMessage =
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.UnsupportedDataTypeUnknownType,
                        row.ColumnName,
                        row.GetMostQualifiedTableName());
            }
            else
            {
                bool excludedForVersion;
                var propertyType = GetStorePrimitiveTypeForVersion(row.DataType, out excludedForVersion);

                if (propertyType == null)
                {
                    errorMessage =
                        string.Format(
                            CultureInfo.InvariantCulture,
                            excludedForVersion
                                ? Resources_VersioningFacade.UnsupportedDataTypeForTarget
                                : Resources_VersioningFacade.UnsupportedDataType,
                            row.DataType,
                            row.GetMostQualifiedTableName(),
                            row.ColumnName);
                }
            }

            if (errorMessage != null)
            {
                errors.Add(
                    new EdmSchemaError(
                        errorMessage,
                        (int)ModelBuilderErrorCode.UnsupportedType,
                        EdmSchemaErrorSeverity.Warning));
            }

            return errorMessage == null;
        }

        // internal for testing
        internal List<string> InferKeyProperties(IList<EdmProperty> properties)
        {
            return
                properties
                    .Where(p => !p.Nullable && IsValidKeyType(_targetEntityFrameworkVersion, p.TypeUsage.EdmType))
                    .Select(p => p.Name)
                    .ToList();
        }

        // internal for testing
        internal Dictionary<string, RowType> CreateTvfReturnTypes(IEnumerable<TableDetailsRow> tvfReturnTypeDetailsRows)
        {
            Debug.Assert(tvfReturnTypeDetailsRows != null, "tvfReturnTypeDetailsRows != null");

            var rowTypes = new Dictionary<string, RowType>();

            foreach (var rowTypeDetailsRows in SplitRows(tvfReturnTypeDetailsRows))
            {
                var errors = new List<EdmSchemaError>();

                // columns that were invalid and could not be converted to properties (e.g. of unknown type)
                List<string> excludedColumnNames;

                // columns that were marked as key columns (including excluded/excluded columns)
                List<string> keyColumnNames;

                // columns that could be converted to properties but cannot be key properties (e.g. of geography type)
                List<string> invalidKeyTypeColumnNames;

                var properties = CreateProperties(
                    rowTypeDetailsRows, errors, out keyColumnNames, out excludedColumnNames, out invalidKeyTypeColumnNames);

                rowTypes[rowTypeDetailsRows[0].GetMostQualifiedTableName()]
                    = RowType.Create(properties, CreateMetadataErrorProperty(errors));
            }

            return rowTypes;
        }

        internal IEnumerable<EdmFunction> CreateFunctions(
            IEnumerable<FunctionDetailsRowView> functionDetailsRows,
            IEnumerable<TableDetailsRow> tvfReturnTypeDetailsRows)
        {
            Debug.Assert(functionDetailsRows != null, "functionDetailsRows != null");
            Debug.Assert(tvfReturnTypeDetailsRows != null, "tvfReturnTypeDetailsRows != null");

            var tvfReturnTypes = CreateTvfReturnTypes(tvfReturnTypeDetailsRows);
            return SplitRows(functionDetailsRows).Select(functionDetails => CreateFunction(functionDetails, tvfReturnTypes));
        }

        // internal for testing
        internal EdmFunction CreateFunction(IList<FunctionDetailsRowView> functionDetailsRows, Dictionary<string, RowType> tvfReturnTypes)
        {
            Debug.Assert(functionDetailsRows != null, "functionDetailsRows != null");
            Debug.Assert(tvfReturnTypes != null, "tvfReturnTypes != null");

            var functionDetails = functionDetailsRows[0];

            if (functionDetails.IsTvf
                && _targetEntityFrameworkVersion < EntityFrameworkVersion.Version3)
            {
                // TVFs were not supported prior to EF5 (schema version 3)
                return null;
            }

            var errors = new List<EdmSchemaError>();
            var parameters = functionDetails.IsParameterNameNull
                                 ? null
                                 : CreateFunctionParameters(functionDetailsRows, errors).ToArray();
            var returnParameter = CreateReturnParameter(functionDetails, tvfReturnTypes, errors);

            var functionName =
                _usedTypeNames.AdjustIdentifier(
                    ModelGeneratorUtils.CreateValidEcmaName(functionDetails.ProcedureName, 'f'));

            return
                EdmFunction.Create(
                    functionName,
                    _namespaceName,
                    DataSpace.SSpace,
                    new EdmFunctionPayload
                        {
                            Schema = functionDetails.Schema,
                            StoreFunctionName = functionName != functionDetails.ProcedureName ? functionDetails.ProcedureName : null,
                            IsAggregate = functionDetails.IsIsAggregate,
                            IsBuiltIn = functionDetails.IsBuiltIn,
                            IsNiladic = functionDetails.IsNiladic,
                            IsComposable = functionDetails.IsComposable,
                            ReturnParameters = returnParameter != null ? new[] { returnParameter } : new FunctionParameter[0],
                            Parameters = parameters
                        },
                    CreateMetadataErrorProperty(errors)
                    );
        }

        // internal for testing
        internal IEnumerable<FunctionParameter> CreateFunctionParameters(
            IList<FunctionDetailsRowView> functionDetailsRows, IList<EdmSchemaError> errors)
        {
            Debug.Assert(functionDetailsRows != null, "functionDetailsRows != null");
            Debug.Assert(errors != null, "errors != null");

            var uniqueIdentifierService = new UniqueIdentifierService();
            var parameterIdx = 0;

            return
                functionDetailsRows
                    .Select(
                        functionDetailsRow =>
                        CreateFunctionParameter(functionDetailsRow, uniqueIdentifierService, parameterIdx++, errors))
                    .Where(p => p != null);
        }

        // internal for testing
        internal FunctionParameter CreateFunctionParameter(
            FunctionDetailsRowView functionDetailsRow, UniqueIdentifierService uniqueIdentifierService, int parameterIndex,
            IList<EdmSchemaError> errors)
        {
            Debug.Assert(functionDetailsRow != null, "functionDetailsRow != null");
            Debug.Assert(uniqueIdentifierService != null, "uniqueIdentifierService != null");
            Debug.Assert(errors != null, "errors != null");

            var parameterType = GetFunctionParameterType(functionDetailsRow, parameterIndex, errors);
            if (parameterType == null)
            {
                return null;
            }

            ParameterMode parameterMode;
            if (!functionDetailsRow.TryGetParameterMode(out parameterMode))
            {
                errors.Add(
                    new EdmSchemaError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.ParameterDirectionNotValid,
                            functionDetailsRow.ProcedureName,
                            functionDetailsRow.ParameterName,
                            functionDetailsRow.ProcParameterMode),
                        (int)ModelBuilderErrorCode.ParameterDirectionNotValid,
                        EdmSchemaErrorSeverity.Warning));
                return null;
            }

            var parameterName =
                uniqueIdentifierService.AdjustIdentifier(
                    ModelGeneratorUtils.CreateValidEcmaName(functionDetailsRow.ParameterName, 'p'));

            return FunctionParameter.Create(parameterName, parameterType, parameterMode);
        }

        // internal for testing
        internal PrimitiveType GetFunctionParameterType(
            FunctionDetailsRowView functionDetailsRow, int parameterIndex, IList<EdmSchemaError> errors)
        {
            Debug.Assert(functionDetailsRow != null, "functionDetailsRow != null");
            Debug.Assert(errors != null, "errors != null");

            var parameterTypeName = functionDetailsRow.IsParameterTypeNull ? null : functionDetailsRow.ParameterType;

            bool excludedForVersion;
            var parameterType = GetStorePrimitiveTypeForVersion(parameterTypeName, out excludedForVersion);

            if (parameterType == null)
            {
                errors.Add(
                    new EdmSchemaError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            excludedForVersion
                                ? Resources_VersioningFacade.UnsupportedFunctionParameterDataTypeForTarget
                                : Resources_VersioningFacade.UnsupportedFunctionParameterDataType,
                            functionDetailsRow.ProcedureName,
                            functionDetailsRow.ParameterName,
                            parameterIndex,
                            parameterTypeName ?? "null"),
                        (int)ModelBuilderErrorCode.UnsupportedType,
                        EdmSchemaErrorSeverity.Warning));
            }

            return parameterType;
        }

        // internal for testing
        internal FunctionParameter CreateReturnParameter(
            FunctionDetailsRowView functionDetailsRow, Dictionary<string, RowType> tvfReturnTypes, List<EdmSchemaError> errors)
        {
            Debug.Assert(functionDetailsRow != null, "functionDetailsRow != null");
            Debug.Assert(tvfReturnTypes != null, "tvfReturnTypes != null");
            Debug.Assert(errors != null, "errors != null");

            EdmType returnType = null;

            if (functionDetailsRow.ReturnType != null)
            {
                bool excludedForVersion;
                returnType = GetStorePrimitiveTypeForVersion(functionDetailsRow.ReturnType, out excludedForVersion);

                if (returnType == null)
                {
                    errors.Add(
                        new EdmSchemaError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                excludedForVersion
                                    ? Resources_VersioningFacade.UnsupportedFunctionReturnDataTypeForTarget
                                    : Resources_VersioningFacade.UnsupportedFunctionReturnDataType,
                                functionDetailsRow.ProcedureName,
                                functionDetailsRow.ReturnType),
                            (int)ModelBuilderErrorCode.UnsupportedType,
                            EdmSchemaErrorSeverity.Warning));
                }
            }
            else if (functionDetailsRow.IsTvf)
            {
                RowType rowType;
                if (tvfReturnTypes.TryGetValue(functionDetailsRow.GetMostQualifiedFunctionName(), out rowType)
                    && !MetadataItemHelper.HasSchemaErrors(rowType))
                {
                    returnType = rowType.GetCollectionType();
                }
                else
                {
                    errors.Add(
                        new EdmSchemaError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources_VersioningFacade.TableReferencedByTvfWasNotFound,
                                functionDetailsRow.GetMostQualifiedFunctionName()),
                            (int)ModelBuilderErrorCode.MissingTvfReturnTable,
                            EdmSchemaErrorSeverity.Warning));

                    // invalid row types will not be serialized so 
                    // reassign errors to the parent TVF definition 
                    if (rowType != null)
                    {
                        errors.AddRange(MetadataItemHelper.GetSchemaErrors(rowType));
                    }
                }
            }

            return
                returnType != null
                    ? FunctionParameter.Create("ReturnType", returnType, ParameterMode.ReturnValue)
                    : null;
        }

        private PrimitiveType GetStorePrimitiveTypeForVersion(string typeName, out bool excludedForVersion)
        {
            excludedForVersion = false;

            PrimitiveType storeType = null;
            if (typeName != null
                && _nameToEdmType.TryGetValue(typeName, out storeType))
            {
                if ((storeType.PrimitiveTypeKind == PrimitiveTypeKind.Geography ||
                     storeType.PrimitiveTypeKind == PrimitiveTypeKind.Geometry ||
                     storeType.PrimitiveTypeKind == PrimitiveTypeKind.HierarchyId)
                    && _targetEntityFrameworkVersion < EntityFrameworkVersion.Version3)
                {
                    excludedForVersion = true;
                    storeType = null;
                }
            }

            return storeType;
        }

        // internal for testing
        internal static bool IsValidKeyType(Version entityFrameworkVersion, EdmType edmType)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(entityFrameworkVersion), "invalid entityFrameworkVersion");
            Debug.Assert(edmType != null, "primitiveType != null");

            var primitiveType = edmType as PrimitiveType;

            if (primitiveType == null)
            {
                return false;
            }

            if (EntityFrameworkVersion.Version1 == entityFrameworkVersion)
            {
                return primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Binary;
            }

            return
                primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Geography &&
                primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Geometry;
        }

        // internal for testing
        internal static List<List<TableDetailsRow>> SplitRows(IEnumerable<TableDetailsRow> tableDetailsRows)
        {
            Debug.Assert(tableDetailsRows != null, "tableDetailsRows != null");

            return
                tableDetailsRows
                    .GroupBy(t => t.GetMostQualifiedTableName())
                    .Select(g => g.ToList())
                    .ToList();
        }

        // internal for testing
        internal static List<List<FunctionDetailsRowView>> SplitRows(
            IEnumerable<FunctionDetailsRowView> functionDetailsRows)
        {
            Debug.Assert(functionDetailsRows != null, "functionDetailsRows != null");

            return
                functionDetailsRows
                    .GroupBy(f => f.GetMostQualifiedFunctionName())
                    .Select(g => g.ToList())
                    .ToList();
        }

        internal static MetadataProperty CreateStoreModelBuilderMetadataProperty(string name, string value)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), "!string.IsNullOrEmpty(name)");
            Debug.Assert(!string.IsNullOrEmpty(value), "!string.IsNullOrEmpty(value)");

            return MetadataProperty.Create(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}:{1}",
                    SchemaManager.GetEntityStoreSchemaGeneratorNamespaceName(),
                    name),
                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                value);
        }

        #region Association types and sets

        internal List<AssociationSet> CreateAssociationSets(
            IEnumerable<RelationshipDetailsRow> relationshipDetailsRows,
            EntityRegister entityRegister,
            List<AssociationType> associationTypes)
        {
            Debug.Assert(relationshipDetailsRows != null, "relationshipDetailsRows != null");
            Debug.Assert(entityRegister != null, "entityRegister != null");
            Debug.Assert(associationTypes != null, "associationTypes != null");

            var associationSets = new List<AssociationSet>();
            var rowGroups =
                relationshipDetailsRows
                    .GroupBy(row => row.RelationshipId)
                    .Select(g => g.ToList());

            foreach (var group in rowGroups)
            {
                var set = TryCreateAssociationSet(group, entityRegister, associationTypes);
                if (set != null)
                {
                    associationSets.Add(set);
                }
            }

            return associationSets;
        }

        internal AssociationSet TryCreateAssociationSet(
            List<RelationshipDetailsRow> relationshipDetailsRows,
            EntityRegister entityRegister,
            List<AssociationType> associationTypes)
        {
            Debug.Assert(relationshipDetailsRows.Count > 0, "relationshipDetailsRows.Count > 0");

            var firstRow = relationshipDetailsRows.First();
            var errors = new List<EdmSchemaError>();

            AssociationType associationType;
            var isValidAssociationType = false;
            var typeName = _usedTypeNames.AdjustIdentifier(firstRow.RelationshipName);
            AssociationEndMember pkEnd = null;
            AssociationEndMember fkEnd = null;
            ReferentialConstraint constraint = null;

            var pkEntityType = TryGetEndEntity(entityRegister, firstRow.GetMostQualifiedPrimaryKey(), errors);
            var fkEntityType = TryGetEndEntity(entityRegister, firstRow.GetMostQualifiedForeignKey(), errors);

            if (ValidateEndEntities(relationshipDetailsRows, pkEntityType, fkEntityType, errors))
            {
                var someFKColumnsAreNullable =
                    _targetEntityFrameworkVersion == EntityFrameworkVersion.Version1
                        ? AreAllFKColumnsNullable(relationshipDetailsRows, fkEntityType)
                        : AreAnyFKColumnsNullable(relationshipDetailsRows, fkEntityType);

                var usedEndNames = new UniqueIdentifierService(StringComparer.OrdinalIgnoreCase);

                pkEnd = AssociationEndMember.Create(
                    usedEndNames.AdjustIdentifier(pkEntityType.Name),
                    pkEntityType.GetReferenceType(),
                    someFKColumnsAreNullable
                        ? RelationshipMultiplicity.ZeroOrOne
                        : RelationshipMultiplicity.One,
                    firstRow.RelationshipIsCascadeDelete
                        ? OperationAction.Cascade
                        : OperationAction.None,
                    null);

                fkEnd = AssociationEndMember.Create(
                    usedEndNames.AdjustIdentifier(fkEntityType.Name),
                    fkEntityType.GetReferenceType(),
                    !someFKColumnsAreNullable && AreRelationshipColumnsTheFullPrimaryKey(relationshipDetailsRows, fkEntityType, r => r.FKColumn)
                        ? RelationshipMultiplicity.ZeroOrOne
                        : RelationshipMultiplicity.Many,
                    OperationAction.None,
                    null);

                constraint = TryCreateReferentialConstraint(relationshipDetailsRows, pkEnd, fkEnd, errors);
                if (constraint != null
                    && ValidateReferentialConstraint(constraint, _generateForeignKeyProperties, typeName, associationTypes, errors))
                {
                    isValidAssociationType = true;
                }
            }

            associationType = AssociationType.Create(
                typeName,
                _namespaceName,
                false,
                DataSpace.SSpace,
                pkEnd,
                fkEnd,
                constraint,
                CreateMetadataProperties(!isValidAssociationType, errors));

            associationTypes.Add(associationType);

            return isValidAssociationType
                       ? CreateAssociationSet(associationType, entityRegister)
                       : null;
        }

        private static AssociationSet CreateAssociationSet(AssociationType associationType, EntityRegister entityRegister)
        {
            Debug.Assert(associationType.AssociationEndMembers.Count == 2);

            var sourceEnd = associationType.AssociationEndMembers[0];
            var targetEnd = associationType.AssociationEndMembers[1];

            EntitySet sourceSet, targetSet;
            if (!entityRegister.EntityTypeToSet.TryGetValue(sourceEnd.GetEntityType(), out sourceSet)
                || !entityRegister.EntityTypeToSet.TryGetValue(targetEnd.GetEntityType(), out targetSet))
            {
                return null;
            }

            return AssociationSet.Create(associationType.Name, associationType, sourceSet, targetSet, null);
        }

        private static EntityType TryGetEndEntity(EntityRegister entityRegister, string key, List<EdmSchemaError> errors)
        {
            EntityType type;
            if (entityRegister.EntityLookup.TryGetValue(key, out type)
                && !MetadataItemHelper.IsInvalid(type))
            {
                return type;
            }

            errors.Add(
                new EdmSchemaError(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        Resources_VersioningFacade.TableReferencedByAssociationWasNotFound,
                        key),
                    (int)ModelBuilderErrorCode.MissingEntity,
                    EdmSchemaErrorSeverity.Error));

            return null;
        }

        private static bool ValidateEndEntities(
            List<RelationshipDetailsRow> relationshipDetailsRows,
            EntityType pkEntityType,
            EntityType fkEntityType,
            List<EdmSchemaError> errors)
        {
            if (pkEntityType == null
                || fkEntityType == null)
            {
                return false;
            }

            if (!AreRelationshipColumnsTheFullPrimaryKey(relationshipDetailsRows, pkEntityType, r => r.PKColumn))
            {
                errors.Add(
                    new EdmSchemaError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.UnsupportedDbRelationship,
                            relationshipDetailsRows.First().RelationshipName),
                        (int)ModelBuilderErrorCode.UnsupportedDbRelationship,
                        EdmSchemaErrorSeverity.Warning));

                return false;
            }

            return true;
        }

        private static bool AreAllFKColumnsNullable(
            List<RelationshipDetailsRow> relationshipDetailsRows,
            EntityType entityType)
        {
            foreach (var row in relationshipDetailsRows)
            {
                EdmProperty property;
                if (entityType.Properties.TryGetValue(row.FKColumn, false, out property))
                {
                    if (!property.Nullable)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AreAnyFKColumnsNullable(
            List<RelationshipDetailsRow> relationshipDetailsRows,
            EntityType entityType)
        {
            foreach (var row in relationshipDetailsRows)
            {
                EdmProperty property;
                if (entityType.Properties.TryGetValue(row.FKColumn, false, out property))
                {
                    if (property.Nullable)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private static bool AreRelationshipColumnsTheFullPrimaryKey(
            IList<RelationshipDetailsRow> relationshipDetailsRows,
            EntityType entityType,
            Func<RelationshipDetailsRow, string> getColumnName)
        {
            if (entityType.KeyMembers.Count != relationshipDetailsRows.Count)
            {
                return false;
            }

            foreach (var row in relationshipDetailsRows)
            {
                if (!entityType.KeyMembers.Contains(getColumnName(row)))
                {
                    return false;
                }
            }

            return true;
        }

        private static ReferentialConstraint TryCreateReferentialConstraint(
            IList<RelationshipDetailsRow> relationshipDetailsRows,
            AssociationEndMember pkEnd,
            AssociationEndMember fkEnd,
            IList<EdmSchemaError> errors)
        {
            var fromProperties = new EdmProperty[relationshipDetailsRows.Count];
            var toProperties = new EdmProperty[relationshipDetailsRows.Count];
            var pkEntityType = pkEnd.GetEntityType();
            var fkEntityType = fkEnd.GetEntityType();

            for (var index = 0; index < relationshipDetailsRows.Count; index++)
            {
                EdmProperty property;
                var row = relationshipDetailsRows[index];

                if (!pkEntityType.Properties.TryGetValue(row.PKColumn, false, out property))
                {
                    errors.Add(
                        new EdmSchemaError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources_VersioningFacade.AssociationMissingKeyColumn,
                                pkEntityType.Name,
                                fkEntityType.Name,
                                pkEntityType.Name + "." + row.PKColumn),
                            (int)ModelBuilderErrorCode.AssociationMissingKeyColumn,
                            EdmSchemaErrorSeverity.Warning));

                    return null;
                }

                fromProperties[index] = property;

                if (!fkEntityType.Properties.TryGetValue(row.FKColumn, false, out property))
                {
                    errors.Add(
                        new EdmSchemaError(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                Resources_VersioningFacade.AssociationMissingKeyColumn,
                                pkEntityType.Name,
                                fkEntityType.Name,
                                pkEntityType.Name + "." + row.FKColumn),
                            (int)ModelBuilderErrorCode.AssociationMissingKeyColumn,
                            EdmSchemaErrorSeverity.Warning));

                    return null;
                }

                toProperties[index] = property;
            }

            return new ReferentialConstraint(pkEnd, fkEnd, fromProperties, toProperties);
        }

        private static bool ValidateReferentialConstraint(
            ReferentialConstraint constraint,
            bool generateForeignKeyProperties,
            string associationTypeName,
            IEnumerable<AssociationType> associationTypes,
            IList<EdmSchemaError> errors)
        {
            if (constraint == null)
            {
                return false;
            }

            // We can skip validation checks if the FKs are directly surfaced (since we can produce valid mappings in these cases).
            if (generateForeignKeyProperties)
            {
                return true;
            }

            if (IsFKPartiallyContainedInPK(constraint, associationTypeName, errors))
            {
                return false;
            }

            // Check if any FK (which could also be a PK) is shared among multiple Associations (ie shared via foreign key constraint).
            // To do this we check if the AssociationType being generated has any dependent property which is also a dependent in one 
            // of the association typed already added. If so, we keep one association and throw the rest away.
            foreach (var toPropertyOfAddedAssociation in associationTypes.SelectMany(
                t => t.ReferentialConstraints.SelectMany(refconst => refconst.ToProperties)))
            {
                foreach (var toProperty in constraint.ToProperties)
                {
                    if (toProperty.DeclaringType.Equals(toPropertyOfAddedAssociation.DeclaringType)
                        && toProperty.Equals(toPropertyOfAddedAssociation))
                    {
                        errors.Add(
                            new EdmSchemaError(
                                string.Format(
                                    CultureInfo.InvariantCulture,
                                    Resources_VersioningFacade.SharedForeignKey,
                                    associationTypeName,
                                    toProperty,
                                    toProperty.DeclaringType),
                                (int)ModelBuilderErrorCode.SharedForeignKey,
                                EdmSchemaErrorSeverity.Warning));

                        return false;
                    }
                }
            }

            return true;
        }

        private static bool IsFKPartiallyContainedInPK(
            ReferentialConstraint constraint,
            string associationTypeName,
            IList<EdmSchemaError> errors)
        {
            var toType = (EntityType)constraint.ToProperties[0].DeclaringType;
            var toPropertiesAreFullyContainedInPK = true;
            var toPropertiesContainedAtLeastOnePK = false;

            foreach (var edmProperty in constraint.ToProperties)
            {
                // Check if there is at least one to property is not primary key.
                toPropertiesAreFullyContainedInPK &= toType.KeyMembers.Contains(edmProperty);
                // Check if there is one to property is primary key.
                toPropertiesContainedAtLeastOnePK |= toType.KeyMembers.Contains(edmProperty);
            }

            if (!toPropertiesAreFullyContainedInPK && toPropertiesContainedAtLeastOnePK)
            {
                var foreignKeys = MembersToCommaSeparatedString(constraint.ToProperties);
                var primaryKeys = MembersToCommaSeparatedString(toType.KeyMembers);

                errors.Add(
                    new EdmSchemaError(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            Resources_VersioningFacade.UnsupportedForeignKeyPattern,
                            associationTypeName,
                            foreignKeys,
                            primaryKeys,
                            toType.Name),
                        (int)ModelBuilderErrorCode.UnsupportedForeignKeyPattern,
                        EdmSchemaErrorSeverity.Warning));

                return true;
            }

            return false;
        }

        private static string MembersToCommaSeparatedString(IEnumerable<EdmMember> members)
        {
            Debug.Assert(members != null, "members != null");

            return "{" + string.Join(",", members.Select(m => m.Name)) + "}";
        }

        #endregion

        public static IList<MetadataProperty> CreateMetadataProperties(bool invalid, IList<EdmSchemaError> errors)
        {
            var properties = new List<MetadataProperty>();

            if (invalid)
            {
                var typeUsage = TypeUsage.CreateDefaultTypeUsage(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Boolean));
                var property = MetadataProperty.Create(
                    MetadataItemHelper.SchemaInvalidMetadataPropertyName, typeUsage, true);
                properties.Add(property);
            }

            if (errors != null
                && errors.Any())
            {
                var typeUsage = TypeUsage.CreateDefaultTypeUsage(
                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String).GetCollectionType());
                var property = MetadataProperty.Create(
                    MetadataItemHelper.SchemaErrorsMetadataPropertyName, typeUsage, errors);
                properties.Add(property);
            }

            return (properties.Count > 0) ? properties : null;
        }

        private static MetadataProperty[] CreateMetadataErrorProperty(IList<EdmSchemaError> errors)
        {
            return
                errors != null && errors.Any()
                    ? new[]
                        {
                            MetadataProperty.Create(
                                MetadataItemHelper.SchemaErrorsMetadataPropertyName,
                                TypeUsage.CreateDefaultTypeUsage(
                                    PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String).GetCollectionType()),
                                errors)
                        }
                    : null;
        }

        // Internal for test purposes only.
        internal class EntityRegister
        {
            public readonly List<EntityType> EntityTypes = new List<EntityType>();
            public readonly List<EntitySet> EntitySets = new List<EntitySet>();
            public readonly Dictionary<string, EntityType> EntityLookup = new Dictionary<string, EntityType>();
            public readonly Dictionary<EntityType, EntitySet> EntityTypeToSet = new Dictionary<EntityType, EntitySet>();

            public void AddEntityType(string key, EntityType type)
            {
                EntityTypes.Add(type);
                EntityLookup.Add(key, type);
            }

            public void AddEntitySet(EntitySet set)
            {
                EntitySets.Add(set);
                EntityTypeToSet.Add(set.ElementType, set);
            }

            public void AddEntitySets(IEnumerable<EntitySet> sets)
            {
                foreach (var set in sets)
                {
                    AddEntitySet(set);
                }
            }
        }
    }
}
