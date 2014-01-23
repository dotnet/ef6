// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Data.Common;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Data.Core;
    using Microsoft.VisualStudio.Data.Services;

    internal abstract class DatabaseEngineBase
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected static ModelBuilderSettings SetupSettingsAndModeForDbPages(
            IServiceProvider serviceProvider,
            Project project,
            EFArtifact artifact,
            bool checkDatabaseConnection,
            ModelBuilderWizardForm.WizardMode noConnectionMode,
            ModelBuilderWizardForm.WizardMode existingConnectionMode,
            out ModelBuilderWizardForm.WizardMode startMode)
        {
            var conceptualEntityModel = artifact.ConceptualModel();
            Debug.Assert(conceptualEntityModel != null, "Null Conceptual Entity Model");
            var entityContainer = conceptualEntityModel.FirstEntityContainer as ConceptualEntityContainer;
            Debug.Assert(entityContainer != null, "Null Conceptual Entity Container");
            var entityContainerName = entityContainer.LocalName.Value;

            // set up ModelBuilderSettings for startMode=noConnectionMode
            startMode = noConnectionMode;
            var settings = new ModelBuilderSettings
            {
                VSApplicationType = VsUtils.GetApplicationType(serviceProvider, project),
                AppConfigConnectionPropertyName = entityContainerName,
                Artifact = artifact,
                UseLegacyProvider = ModelHelper.GetDesignerPropertyValueFromArtifactAsBool(
                    OptionsDesignerInfo.ElementName,
                    OptionsDesignerInfo.AttributeUseLegacyProvider,
                    OptionsDesignerInfo.UseLegacyProviderDefault,
                    artifact),
                TargetSchemaVersion = artifact.SchemaVersion,
                Project = project,
                ModelPath = artifact.Uri.LocalPath,
                ProviderManifestToken = artifact.GetProviderManifestToken()
            };

            // Get the provider manifest token from the existing SSDL.
            // We don't want to attempt to get it from provider services since this requires a connection
            // which will severely impact the performance of Model First in disconnected scenarios.

            // Change startMode and settings appropriately depending on whether there is an existing connection string and whether we can/should connect
            // to the database
            var connectionString = ConnectionManager.GetConnectionStringObject(project, entityContainerName);
            if (connectionString != null)
            {
                var ecsb = connectionString.Builder;
                var runtimeProviderName = ecsb.Provider;
                var runtimeProviderConnectionString = ecsb.ProviderConnectionString;
                var designTimeProviderConnectionString = connectionString.GetDesignTimeProviderConnectionString(project);
                var initialCatalog = String.Empty;

                if (checkDatabaseConnection)
                {
                    // This path will check to make sure that we can connect to an existing database before changing the start mode to 'existingConnection'
                    IVsDataConnection dataConnection = null;
                    try
                    {
                        var dataConnectionManager = serviceProvider.GetService(typeof(IVsDataConnectionManager)) as IVsDataConnectionManager;
                        Debug.Assert(dataConnectionManager != null, "Could not find IVsDataConnectionManager");

                        var dataProviderManager = serviceProvider.GetService(typeof(IVsDataProviderManager)) as IVsDataProviderManager;
                        Debug.Assert(dataProviderManager != null, "Could not find IVsDataProviderManager");

                        if (dataConnectionManager != null
                            && dataProviderManager != null)
                        {
                            // this will either get an existing connection or attempt to create a new one
                            dataConnection = DataConnectionUtils.GetDataConnection(
                                dataConnectionManager,
                                dataProviderManager,
                                connectionString.DesignTimeProviderInvariantName,
                                designTimeProviderConnectionString);
                            Debug.Assert(
                                dataConnection != null,
                                "Could not find the IVsDataConnection; an exception should have been thrown if this was the case");
                            if (dataConnection != null)
                            {
                                VsUtils.EnsureProvider(runtimeProviderName, settings.UseLegacyProvider, project, serviceProvider);

                                if (CanCreateAndOpenConnection(
                                    new StoreSchemaConnectionFactory(),
                                    runtimeProviderName,
                                    connectionString.DesignTimeProviderInvariantName,
                                    designTimeProviderConnectionString))
                                {
                                    startMode = existingConnectionMode;
                                    initialCatalog = DataConnectionUtils.GetInitialCatalog(dataProviderManager, dataConnection);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // do nothing - we will go to WizardPageDbConfig which is
                        // what we want if the DB connection fails
                    }
                    finally
                    {
                        // Close the IVsDataConnection
                        if (dataConnection != null)
                        {
                            try
                            {
                                dataConnection.Close();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                else
                {
                    // This path will just parse the existing connection string in order to change the start mode. This is ideal for features
                    // that do not need a database connection -- the information in the connection string is enough.
                    startMode = existingConnectionMode;
                    initialCatalog = DataConnectionUtils.GetInitialCatalog(
                        connectionString.DesignTimeProviderInvariantName, designTimeProviderConnectionString);
                }

                if (startMode == existingConnectionMode)
                {
                    // the invariant name and connection string came from app.config, so they are "runtime" invariant names and not "design-time"
                    // (Note: it is OK for InitialCatalog to be null at this stage e.g. from a provider who do not support the concept of Initial Catalog)
                    settings.SetInvariantNamesAndConnectionStrings(
                        serviceProvider,
                        project,
                        runtimeProviderName,
                        runtimeProviderConnectionString,
                        runtimeProviderConnectionString,
                        false);
                    settings.InitialCatalog = initialCatalog;
                    settings.AppConfigConnectionPropertyName = entityContainerName;
                    settings.SaveConnectionStringInAppConfig = false;

                    VsUtils.EnsureProvider(runtimeProviderName, settings.UseLegacyProvider, project, serviceProvider);
                }
            }

            return settings;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected static bool CanCreateAndOpenConnection(
            StoreSchemaConnectionFactory connectionFactory, string providerInvariantName, string designTimeInvariantName,
            string designTimeConnectionString)
        {
            Debug.Assert(connectionFactory != null, "connectionFactory != null");
            Debug.Assert(
                !string.IsNullOrWhiteSpace(designTimeInvariantName),
                "designTimeInvariantName must not be null or empty");
            Debug.Assert(
                !string.IsNullOrWhiteSpace(designTimeConnectionString),
                "designTimeConnectionString must not be null or empty");

            EntityConnection entityConnection = null;
            try
            {
                // attempt to create a DbConnection using the provider connection string we have. This will
                // throw an exception if the connection cannot be made, for example, if the credentials aren't
                // set. This has to be done using DDEX-based APIs since the SchemaGenerator is based off of
                // DbConnection, and DDEX will save the password whereas DbConnection will not.
                Version _;
                entityConnection = connectionFactory.Create(
                    DependencyResolver.Instance,
                    providerInvariantName,
                    designTimeConnectionString,
                    EntityFrameworkVersion.Latest,
                    out _);
                entityConnection.Open();
            }
            catch
            {
                return false;
            }
            finally
            {
                // Close the EntityConnection
                if (entityConnection != null)
                {
                    VsUtils.SafeCloseDbConnection(entityConnection, designTimeInvariantName, designTimeConnectionString);
                }
            }

            return true;
        }
    }
}