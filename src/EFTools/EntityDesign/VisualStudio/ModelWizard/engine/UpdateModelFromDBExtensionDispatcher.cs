// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.Diagnostics;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;

    internal class UpdateModelFromDBExtensionDispatcher : ModelGenerationExtensionDispatcher
    {
        private readonly ProjectItem _projectItem;
        private readonly XDocument _updateModelDocument;
        private readonly XDocument _originalDocument;

        internal UpdateModelFromDBExtensionDispatcher(
            WizardKind wizardKind, XDocument dbDocument, XDocument currentDocument, ProjectItem projectItem, XDocument originalDocument,
            XDocument updateModelDocument)
            :
                base(wizardKind, dbDocument, currentDocument, projectItem.ContainingProject)
        {
            _projectItem = projectItem;
            _originalDocument = originalDocument;
            _updateModelDocument = updateModelDocument;
        }

        private ProjectItem ProjectItem
        {
            get { return _projectItem; }
        }

        private XDocument OriginalDocument
        {
            get { return _originalDocument; }
        }

        private XDocument UpdateModelDocument
        {
            get { return _updateModelDocument; }
        }

        protected override ModelGenerationExtensionContext CreateContext()
        {
            Debug.Assert(VsUtils.EntityFrameworkSupportedInProject(Project, PackageManager.Package, allowMiscProject: false));

            var targetSchemaVersion = EdmUtils.GetEntityFrameworkVersion(Project, PackageManager.Package);
            return new UpdateModelFromDatabaseExtensionContextImpl(
                Project, ProjectItem, targetSchemaVersion,
                CurrentDocument, FromDatabaseDocument, OriginalDocument, UpdateModelDocument);
        }

        protected override void DispatchToSingleExtension(IModelGenerationExtension extension, ModelGenerationExtensionContext context)
        {
            var umfdbContext = context as UpdateModelExtensionContext;
            Debug.Assert(umfdbContext != null, "Unexpected type of context!");
            if (context != null)
            {
                extension.OnAfterModelUpdated(umfdbContext);
            }
        }

        protected override void PreDispatch()
        {
            base.PreDispatch();

            if (_originalDocument != null)
            {
                _originalDocument.Changing += BeforeEventHandler;
            }

            if (_updateModelDocument != null)
            {
                _updateModelDocument.Changing += BeforeEventHandler;
            }
        }

        protected override void PostDispatch()
        {
            if (_originalDocument != null)
            {
                _originalDocument.Changing -= BeforeEventHandler;
            }

            if (_updateModelDocument != null)
            {
                _updateModelDocument.Changing -= BeforeEventHandler;
            }

            base.PostDispatch();
        }
    }
}
