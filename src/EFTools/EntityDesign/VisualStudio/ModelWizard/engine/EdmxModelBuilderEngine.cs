// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using Microsoft.Data.Entity.Design.Model.Designer;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;
    using System.Xml.Linq;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class EdmxModelBuilderEngine : ModelBuilderEngine
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

        private readonly IInitialModelContentsFactory _initialModelContentsFactory;

        public EdmxModelBuilderEngine(IInitialModelContentsFactory initialModelContentsFactory)
        {
            Debug.Assert(initialModelContentsFactory != null, "initialModelContentsFactory is null");

            _initialModelContentsFactory = initialModelContentsFactory;
        }

        // <summary>
        //     This is the XDocument of the model in memory.  No assumptions should be made that it exists on disk.
        // </summary>
        public XDocument Edmx { get; private set; }

        // internal virtual to allow mocking
        protected override void ProcessModel(DbModel model, string storeModelNamespace, ModelBuilderSettings settings, 
            ModelBuilderEngineHostContext hostContext, List<EdmSchemaError> errors)
        {
            Edmx = XDocument.Parse(_initialModelContentsFactory.GetInitialModelContents(settings.TargetSchemaVersion));

            var edmxHelper = new EdmxHelper(Edmx);

            edmxHelper.UpdateEdmxFromModel(model, storeModelNamespace, settings.ModelNamespace, errors);

            // load extensions that want to update model after the wizard has run. 
            hostContext.DispatchToModelGenerationExtensions();

            UpdateDesignerInfo(edmxHelper, settings);
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
