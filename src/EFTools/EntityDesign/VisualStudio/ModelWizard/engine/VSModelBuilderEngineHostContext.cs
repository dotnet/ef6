// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.Xml.Linq;
    using EnvDTE;

    internal class VSModelBuilderEngineHostContext : ModelBuilderEngineHostContext
    {
        private readonly Project _project;
        private readonly ModelBuilderSettings _settings;

        internal VSModelBuilderEngineHostContext(Project project, ModelBuilderSettings settings)
        {
            _project = project;
            _settings = settings;
        }

        internal override void LogMessage(string statusMessage)
        {
            VsUtils.LogOutputWindowPaneMessage(_project, statusMessage);
        }

        internal override void DispatchToModelGenerationExtensions()
        {
            var fromDBDocument = new XDocument(_settings.ModelBuilderEngine.XDocument);
            var dispatcher =
                new ModelGenerationExtensionDispatcher(
                    _settings.WizardKind,
                    fromDBDocument,
                    _settings.ModelBuilderEngine.XDocument,
                    _project);

            dispatcher.Dispatch();
            _settings.HasExtensionChangedModel = dispatcher.HasCurrentChanged;
        }
    }
}
