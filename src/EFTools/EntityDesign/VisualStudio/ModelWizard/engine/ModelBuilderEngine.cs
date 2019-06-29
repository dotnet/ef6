// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Resources = Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources;

    internal abstract class ModelBuilderEngine
    {
        public DbModel Model { get; private set; }

        // virtual for testing
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual void GenerateModel(ModelBuilderSettings settings, IVsUtils vsUtils = null, 
            ModelBuilderEngineHostContext hostContext = null)
        {
            if (settings.GenerationOption == ModelGenerationOption.GenerateFromDatabase
                && String.IsNullOrEmpty(settings.DesignTimeConnectionString))
            {
                throw new ArgumentOutOfRangeException(Resources.Engine_EmptyConnStringErrorMsg);
            }

            var generatingModelWatch = Stopwatch.StartNew();

            hostContext = hostContext ?? new VSModelBuilderEngineHostContext(settings);
            vsUtils = vsUtils ?? new VsUtilsWrapper();

            // Clear out the ModelGenErrorCache before ModelGen begins
            PackageManager.Package.ModelGenErrorCache.RemoveErrors(settings.ModelPath);

            var errors = new List<EdmSchemaError>();

            try
            {
                var storeModelNamespace = GetStoreNamespace(settings);
                Model = GenerateModels(storeModelNamespace, settings, errors);

                ProcessModel(Model, storeModelNamespace, settings, hostContext, errors);

                hostContext.LogMessage(
                    FormatMessage(
                        errors.Any()
                            ? Resources.Engine_ModelGenErrors
                            : Resources.Engine_ModelGenSuccess,
                        Path.GetFileName(settings.ModelPath)));

                if (errors.Any())
                {
                    PackageManager.Package.ModelGenErrorCache.AddErrors(settings.ModelPath, errors);
                }
            }
            catch (Exception e)
            {
                hostContext.LogMessage(FormatMessage(Resources.Engine_ModelGenException, e));
                // The exception we re-throw will get swallowed in the message pump and therefore we need to show the message box here.
                // It will also prevent the form.WizardFinished from being set to true which will cause cancelling the wizard and 
                // therefore block adding new project items to the project as well as ModelObjectItemWizardFrom.RunFinished method.
                vsUtils.ShowErrorDialog(FormatMessage(Resources.Engine_ModelGenExceptionMessageBox, e.GetType().Name, e.Message));
                throw;
            }
            finally
            {
                generatingModelWatch.Stop();

                hostContext.LogMessage(FormatMessage(Resources.LoadingDBMetadataTimeMsg, settings.LoadingDBMetadataTime));
                hostContext.LogMessage(FormatMessage(Resources.GeneratingModelTimeMsg, generatingModelWatch.Elapsed));
            }
        }

        // internal virtual to allow mocking
        internal virtual DbModel GenerateModels(string storeModelNamespace, ModelBuilderSettings settings, List<EdmSchemaError> errors)
        {
            return new ModelGenerator(settings, storeModelNamespace).GenerateModel(errors);
        }

        protected abstract void ProcessModel(
            DbModel model, string storeModelNamespace, ModelBuilderSettings settings,
            ModelBuilderEngineHostContext hostContext, List<EdmSchemaError> errors);

        private static string GetStoreNamespace(ModelBuilderSettings settings)
        {
            return
                string.IsNullOrEmpty(settings.StorageNamespace)
                    ? String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.SelectTablesPage_DefaultStorageNamespaceFormat,
                        settings.ModelNamespace)
                    : settings.StorageNamespace;
        }

        private static string FormatMessage(string resourcestringName, params object[] args)
        {
            return
                String.Format(
                    CultureInfo.CurrentCulture,
                    resourcestringName,
                    args);
        }
    }
}
