// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Xml.Linq;
    using EnvDTE;

    internal class UpdateModelFromDatabaseExtensionContextImpl : UpdateModelExtensionContext
    {
        private readonly Project _project;
        private readonly ProjectItem _projectItem;
        private readonly Version _runtimeVersion;
        private readonly XDocument _currentXDocument;
        private readonly XDocument _documentFromDatabase;
        private readonly XDocument _originalDocument;
        private readonly XDocument _updateModelFromDBDocument;

        internal UpdateModelFromDatabaseExtensionContextImpl(
            Project project,
            ProjectItem projectItem,
            Version runtimeVersion,
            XDocument currentXDocument,
            XDocument documentFromDatabase,
            XDocument originalDocument,
            XDocument updateModelFromDBDocument)
        {
            _project = project;
            _projectItem = projectItem;
            _runtimeVersion = runtimeVersion;
            _currentXDocument = currentXDocument;
            _documentFromDatabase = documentFromDatabase;
            _originalDocument = originalDocument;
            _updateModelFromDBDocument = updateModelFromDBDocument;
        }

        public override Project Project
        {
            get { return _project; }
        }

        public override Version EntityFrameworkVersion
        {
            get { return _runtimeVersion; }
        }

        public override XDocument CurrentDocument
        {
            get { return _currentXDocument; }
        }

        public override XDocument GeneratedDocument
        {
            get { return _documentFromDatabase; }
        }

        public override ProjectItem ProjectItem
        {
            get { return _projectItem; }
        }

        public override XDocument OriginalDocument
        {
            get { return _originalDocument; }
        }

        public override XDocument UpdateModelDocument
        {
            get { return _updateModelFromDBDocument; }
        }

        public override WizardKind WizardKind
        {
            get { return WizardKind.UpdateModel; }
        }
    }
}
