// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Resources = Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal abstract class EdmxModelBuilderEngine
    {
        private static readonly XmlWriterSettings WriterSettings = new XmlWriterSettings();

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static EdmxModelBuilderEngine()
        {
            WriterSettings.Indent = true;
            WriterSettings.ConformanceLevel = ConformanceLevel.Fragment;
            // this is needed for correct indenting
            WriterSettings.NewLineChars += "      ";
        }

        // <summary>
        //     This is the XDocument of the model in memory.  No assumptions should be made that it exists on disk.
        // </summary>
        internal abstract XDocument Model { get; }

        protected abstract void InitializeModelContents(Version targetSchemaVersion);

        // <summary>
        //     Generates EDMX file.
        // </summary>
        public void GenerateModel(ModelBuilderSettings settings)
        {
            if (settings.GenerationOption == ModelGenerationOption.GenerateFromDatabase
                && String.IsNullOrEmpty(settings.DesignTimeConnectionString))
            {
                throw new ArgumentOutOfRangeException(Resources.Engine_EmptyConnStringErrorMsg);
            }

            InitializeModelContents(settings.TargetSchemaVersion);

            GenerateModel(new EdmxHelper(Model), settings, new VSModelBuilderEngineHostContext(settings));
        }

        // internal virtual to allow mocking
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal virtual void GenerateModel(EdmxHelper edmxHelper, ModelBuilderSettings settings, ModelBuilderEngineHostContext hostContext)
        {
            var generatingModelWatch = Stopwatch.StartNew();

            // Clear out the ModelGenErrorCache before ModelGen begins
            PackageManager.Package.ModelGenErrorCache.RemoveErrors(settings.ModelPath);

            var errors = new List<EdmSchemaError>();
            try
            {
                var storeModelNamespace = GetStoreNamespace(settings);
                var model = GenerateModels(storeModelNamespace, settings, errors);

                edmxHelper.UpdateEdmxFromModel(model, storeModelNamespace, settings.ModelNamespace, errors);

                // load extensions that want to update model after the wizard has run. 
                hostContext.DispatchToModelGenerationExtensions();

                UpdateDesignerInfo(edmxHelper, settings);

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
            }

            generatingModelWatch.Stop();

            hostContext.LogMessage(FormatMessage(Resources.LoadingDBMetadataTimeMsg, settings.LoadingDBMetatdataTime));
            hostContext.LogMessage(FormatMessage(Resources.GeneratingModelTimeMsg, generatingModelWatch.Elapsed));
        }

        // internal virtual to allow mocking
        internal virtual DbModel GenerateModels(string storeModelNamespace, ModelBuilderSettings settings, List<EdmSchemaError> errors)
        {
            return new ModelGenerator(settings, storeModelNamespace).GenerateModel(errors);
        }

        private static string FormatMessage(string resourcestringName, params object[] args)
        {
            return
                String.Format(
                    CultureInfo.CurrentCulture,
                    resourcestringName,
                    args);
        }

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

        protected virtual void UpdateDesignerInfo(EdmxHelper edmxHelper, ModelBuilderSettings settings)
        {
            Debug.Assert(edmxHelper != null);

            edmxHelper.UpdateDesignerOptionProperty(
                OptionsDesignerInfo.AttributeEnablePluralization, settings.UsePluralizationService);
            edmxHelper.UpdateDesignerOptionProperty(
                OptionsDesignerInfo.AttributeIncludeForeignKeysInModel, settings.IncludeForeignKeysInModel);
            edmxHelper.UpdateDesignerOptionProperty(
                OptionsDesignerInfo.AttributeUseLegacyProvider, settings.UseLegacyProvider);
        }
    }
}
