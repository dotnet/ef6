// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Xml.Linq;
    using EnvDTE;

    internal class ModelGenerationExtensionContextImpl : ModelGenerationExtensionContext
    {
        private readonly Project _project;
        private readonly Version _targetSchemaVersion;
        private readonly XDocument _currentXDocument;
        private readonly XDocument _documentFromDatabase;
        private readonly WizardKind _wizardKind;

        internal ModelGenerationExtensionContextImpl(
            Project project, Version targetSchemaVersion, XDocument currentXDocument, XDocument documentFromDatabase, WizardKind wizardKind)
        {
            _project = project;
            _targetSchemaVersion = targetSchemaVersion;
            _currentXDocument = currentXDocument;
            _documentFromDatabase = documentFromDatabase;
            _wizardKind = wizardKind;
        }

        public override Project Project
        {
            get { return _project; }
        }

        public override Version EntityFrameworkVersion
        {
            get { return _targetSchemaVersion; }
        }

        public override XDocument CurrentDocument
        {
            get { return _currentXDocument; }
        }

        public override XDocument GeneratedDocument
        {
            get { return _documentFromDatabase; }
        }

        public override WizardKind WizardKind
        {
            get { return _wizardKind; }
        }
    }
}
