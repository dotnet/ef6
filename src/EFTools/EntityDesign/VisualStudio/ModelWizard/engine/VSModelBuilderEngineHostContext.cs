// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.Xml.Linq;
    using EnvDTE;

    internal class VSModelBuilderEngineHostContext : ModelBuilderEngineHostContext
    {
        private readonly ModelBuilderSettings _settings;

        internal VSModelBuilderEngineHostContext(ModelBuilderSettings settings)
        {
            _settings = settings;
        }

        internal override void LogMessage(string statusMessage)
        {
            VsUtils.LogOutputWindowPaneMessage(_settings.Project, statusMessage);
        }

        internal override void DispatchToModelGenerationExtensions()
        {
            var fromDBDocument = new XDocument(_settings.ModelBuilderEngine.Model);
            var dispatcher =
                new ModelGenerationExtensionDispatcher(
                    _settings.WizardKind,
                    fromDBDocument,
                    _settings.ModelBuilderEngine.Model,
                    _settings.Project);

            dispatcher.Dispatch();
            _settings.HasExtensionChangedModel = dispatcher.HasCurrentChanged;
        }
    }
}
