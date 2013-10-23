// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;

    internal class ModelGenerationExtensionDispatcher
    {
        private readonly WizardKind _wizardKind;
        private readonly XDocument _fromDatabaseDocument;
        private readonly XDocument _currentXDocument;
        private readonly Project _project;
        private bool _hasCurrentChanged;

        internal ModelGenerationExtensionDispatcher(WizardKind wizardKind, XDocument dbDocument, XDocument currentDocument, Project project)
        {
            _wizardKind = wizardKind;
            _fromDatabaseDocument = dbDocument;
            _currentXDocument = currentDocument;
            _project = project;
        }

        protected XDocument CurrentDocument
        {
            get { return _currentXDocument; }
        }

        protected XDocument FromDatabaseDocument
        {
            get { return _fromDatabaseDocument; }
        }

        protected Project Project
        {
            get { return _project; }
        }

        protected WizardKind WizardKind
        {
            get { return _wizardKind; }
        }

        internal bool HasCurrentChanged
        {
            get { return _hasCurrentChanged; }
        }

        protected virtual ModelGenerationExtensionContext CreateContext()
        {
            Debug.Assert(VsUtils.EntityFrameworkSupportedInProject(_project, PackageManager.Package, allowMiscProject: false));

            var targetSchemaVersion = EdmUtils.GetEntityFrameworkVersion(_project, PackageManager.Package);
            return new ModelGenerationExtensionContextImpl(
                _project, targetSchemaVersion, _currentXDocument, _fromDatabaseDocument, WizardKind);
        }

        protected virtual void DispatchToSingleExtension(IModelGenerationExtension extension, ModelGenerationExtensionContext context)
        {
            extension.OnAfterModelGenerated(context);
        }

        protected virtual void PreDispatch()
        {
            if (_fromDatabaseDocument != null)
            {
                _fromDatabaseDocument.Changing += BeforeEventHandler;
            }

            if (CurrentDocument != null)
            {
                CurrentDocument.Changing += BeforeChangingCurrentEventHandler;
            }
        }

        protected virtual void PostDispatch()
        {
            try
            {
                if (_fromDatabaseDocument != null)
                {
                    _fromDatabaseDocument.Changing -= BeforeEventHandler;
                }
            }
            finally
            {
                // be sure to unhook from the current document
                if (CurrentDocument != null)
                {
                    CurrentDocument.Changing -= BeforeChangingCurrentEventHandler;
                }
            }
        }

        internal void Dispatch()
        {
            PreDispatch();
            DispatchInternal();
            PostDispatch();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void DispatchInternal()
        {
            Debug.Assert(
                WizardKind == WizardKind.UpdateModel || WizardKind == WizardKind.Generate, "Unexpected value for WizardKind = " + WizardKind);

            var modelGenerationExtensions = EscherExtensionPointManager.LoadModelGenerationExtensions();
            if (modelGenerationExtensions.Length > 0) // don't create context if not needed
            {
                var modelGenerationExtensionContext = CreateContext();

                foreach (var exportInfo in modelGenerationExtensions)
                {
                    var extension = exportInfo.Value;
                    try
                    {
                        DispatchToSingleExtension(extension, modelGenerationExtensionContext);
                    }
                    catch (Exception e)
                    {
                        VsUtils.ShowErrorDialog(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Extensibility_ErrorOccurredDuringCallToExtension,
                                extension.GetType().FullName,
                                VsUtils.ConstructInnerExceptionErrorMessage(e)));
                    }
                }
            }
        }

        // event handler to ensure that the extension doesn't edit any 
        // document except the current one
        protected EventHandler<XObjectChangeEventArgs> _beforeEvent;

        protected EventHandler<XObjectChangeEventArgs> BeforeEventHandler
        {
            get
            {
                if (_beforeEvent == null)
                {
                    _beforeEvent = OnBeforeChange;
                }
                return _beforeEvent;
            }
        }

        protected void OnBeforeChange(object sender, XObjectChangeEventArgs e)
        {
            throw new InvalidOperationException(Resources.Extensibility_CantEditModel);
        }

        // event handler to record when an extension makes changes to the current document
        protected EventHandler<XObjectChangeEventArgs> _beforeChangingCurrentEvent;

        protected EventHandler<XObjectChangeEventArgs> BeforeChangingCurrentEventHandler
        {
            get
            {
                if (_beforeChangingCurrentEvent == null)
                {
                    _beforeChangingCurrentEvent = OnBeforeChangingCurrent;
                }
                return _beforeChangingCurrentEvent;
            }
        }

        protected void OnBeforeChangingCurrent(object sender, XObjectChangeEventArgs e)
        {
            _hasCurrentChanged = true;
        }
    }
}
