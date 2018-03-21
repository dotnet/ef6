// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Pluralization;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery;

    internal class ModelGenerator
    {
        private const string SqlServerInvariantName = "System.Data.SqlClient";
        private readonly ModelBuilderSettings _settings;
        private readonly string _storeModelNamespace;

        public ModelGenerator(ModelBuilderSettings settings, string storeModelNamespace)
        {
            Debug.Assert(settings != null, "settings != null");
            Debug.Assert(
                !string.IsNullOrWhiteSpace(storeModelNamespace),
                "namespace must not be null or empty string.");

            _settings = settings;
            _storeModelNamespace = storeModelNamespace;
        }

        public DbModel GenerateModel(List<EdmSchemaError> errors)
        {
            Debug.Assert(errors != null, "errors != null");

            var storeModel = CreateStoreModel();

            var mappingContext = CreateMappingContext(storeModel);

            errors.AddRange(CollectStoreModelErrors(storeModel));
            errors.AddRange(mappingContext.Errors);

            return DbDatabaseMappingBuilder.Build(mappingContext);
        }

        // virtual for testing
        internal virtual EdmModel CreateStoreModel()
        {
            var storeModel =
                new StoreModelBuilder(
                    _settings.RuntimeProviderInvariantName,
                    _settings.ProviderManifestToken,
                    _settings.TargetSchemaVersion,
                    _storeModelNamespace,
                    DependencyResolver.Instance,
                    _settings.IncludeForeignKeysInModel)
                    .Build(GetStoreSchemaDetails(new StoreSchemaConnectionFactory()));

            return storeModel;
        }

        // internal virtual for testing
        internal virtual StoreSchemaDetails GetStoreSchemaDetails(StoreSchemaConnectionFactory connectionFactory)
        {
            Version storeSchemaModelVersion;
            var connection =
                connectionFactory
                    .Create(
                        DependencyResolver.Instance,
                        _settings.RuntimeProviderInvariantName,
                        _settings.DesignTimeConnectionString,
                        _settings.TargetSchemaVersion,
                        out storeSchemaModelVersion);

            var facadeFilters =
                _settings.DatabaseObjectFilters ?? Enumerable.Empty<EntityStoreSchemaFilterEntry>();

            return
                CreateDbSchemaLoader(connection, storeSchemaModelVersion)
                    .LoadStoreSchemaDetails(facadeFilters.ToList());
        }

        internal virtual EntityStoreSchemaGeneratorDatabaseSchemaLoader CreateDbSchemaLoader(
            EntityConnection connection, Version storeSchemaModelVersion)
        {
            return
                new EntityStoreSchemaGeneratorDatabaseSchemaLoader(
                    connection,
                    storeSchemaModelVersion,
                    string.Equals(_settings.RuntimeProviderInvariantName, SqlServerInvariantName, StringComparison.Ordinal));
        }

        // internal virtual for testing
        internal virtual SimpleMappingContext CreateMappingContext(EdmModel storeModel)
        {
            Debug.Assert(storeModel != null, "storeModel != null");

            return
                new OneToOneMappingBuilder(
                    _settings.ModelNamespace,
                    _settings.ModelEntityContainerName,
                    _settings.UsePluralizationService
                        ? DependencyResolver.GetService<IPluralizationService>()
                        : null,
                    _settings.IncludeForeignKeysInModel).Build(storeModel);
        }

        // internal for testing
        internal static List<EdmSchemaError> CollectStoreModelErrors(EdmModel model)
        {
            Debug.Assert(model != null, "model != null");

            var errors = new List<EdmSchemaError>();

            var rowTypeReturnTypes =
                model.Functions
                    .SelectMany(f => f.ReturnParameters).Select(p => p.TypeUsage.EdmType).OfType<RowType>();

            foreach (var item in model.GlobalItems.Concat(rowTypeReturnTypes))
            {
                var schemaErrorsMetadataProperty =
                    item.MetadataProperties.FirstOrDefault(
                        p => p.Name == "EdmSchemaErrors");

                if (schemaErrorsMetadataProperty != null)
                {
                    errors.AddRange((IList<EdmSchemaError>)schemaErrorsMetadataProperty.Value);
                }
            }

            return errors;
        }
    }
}
