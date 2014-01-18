// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Globalization;
    using System.Xml;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties;
    using Microsoft.VSDesigner.Data;
    using Microsoft.VisualStudio.Data.Core;
    using Microsoft.VisualStudio.Data.Services;
    using Microsoft.VisualStudio.Shell;

    // <summary>
    //     A wrapper around IVsDataConnectionDialog to share functionality for
    //     filtering against ADO.NET providers, etc.
    // </summary>
    internal class EntityDataConnectionDialog
    {
        internal event EventHandler BeforeAddSources;
        internal event EventHandler AfterAddSources;
        internal event EventHandler BeforeShowDialog;
        internal event EventHandler AfterShowDialog;
        internal event EventHandler BeforeCheckIsAdoNetProviderEvent;
        internal event EventHandler AfterCheckIsAdoNetProviderEvent;
        private readonly IVsDataConnectionDialogFactory _dialogFactory;
        private readonly IVsDataProviderManager _dataProviderManager;
        private readonly IVsDataExplorerConnectionManager _dataExplorerConnectionManager;
        private readonly Project _appProject;

        internal IVsDataConnection SelectedConnection { get; private set; }

        internal IVsDataExplorerConnection SelectedExplorerConnection { get; private set; }

        internal EntityDataConnectionDialog(Project appProject)
        {
            _appProject = appProject;

            _dialogFactory = Package.GetGlobalService(typeof(IVsDataConnectionDialogFactory)) as IVsDataConnectionDialogFactory;
            if (_dialogFactory == null)
            {
                throw new InvalidOperationException(Resources.EntityDataConnectionDialog_NoDataConnectionDialogFactory);
            }

            _dataProviderManager = Package.GetGlobalService(typeof(IVsDataProviderManager)) as IVsDataProviderManager;
            if (_dataProviderManager == null)
            {
                throw new InvalidOperationException(Resources.EntityDataConnectionDialog_NoDataProviderManager);
            }

            _dataExplorerConnectionManager =
                Package.GetGlobalService(typeof(IVsDataExplorerConnectionManager)) as IVsDataExplorerConnectionManager;
            if (_dataExplorerConnectionManager == null)
            {
                throw new InvalidOperationException(Resources.EntityDataConnectionDialog_NoDataExplorerConnectionManager);
            }
        }

        internal void ShowDialog()
        {
            var dialog = _dialogFactory.CreateConnectionDialog();
            if (dialog == null)
            {
                throw new InvalidOperationException(Resources.EntityDataConnectionDialog_NoDataConnectionDialog);
            }

            RaiseBeforeAddSourcesEvent();
            dialog.AddSources(IsSupportedProvider);
            RaiseAfterAddSourcesEvent();

            dialog.LoadSourceSelection();

            RaiseBeforeShowDialogEvent();
            var dc = dialog.ShowDialog(true);
            RaiseAfterShowDialogEvent();

            if (dialog.SaveSelection
                && dc != null)
            {
                dialog.SaveProviderSelections();
                dialog.SaveSourceSelection();
            }

            if (dc != null)
            {
                try
                {
                    SelectedExplorerConnection = _dataExplorerConnectionManager.AddConnection(
                        null, dc.Provider, dc.EncryptedConnectionString, true);
                    SelectedConnection = dc;
                }
                catch (XmlException xmlException)
                {
                    // AddConnection() call above can throw an XmlException if the connection cannot be made
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.CurrentCulture, Resources.EntityDataConnectionDialog_DataConnectionInvalid, xmlException.Message),
                        xmlException);
                }
            }
        }

        private bool IsSupportedProvider(Guid source, Guid provider)
        {
            RaiseBeforeIsAdoNetProviderEvent();

            var result = DataConnectionUtils.HasEntityFrameworkProvider(
                _dataProviderManager, provider, _appProject, Services.ServiceProvider)
                         && DataProviderProjectControl.IsProjectSupported(provider, _appProject);

            RaiseAfterIsAdoNetProviderEvent();

            return result;
        }

        private void RaiseAfterShowDialogEvent()
        {
            var handlers = AfterShowDialog;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void RaiseBeforeShowDialogEvent()
        {
            var handlers = BeforeShowDialog;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void RaiseAfterAddSourcesEvent()
        {
            var handlers = AfterAddSources;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void RaiseBeforeAddSourcesEvent()
        {
            var handlers = BeforeAddSources;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void RaiseAfterIsAdoNetProviderEvent()
        {
            var handlers = AfterCheckIsAdoNetProviderEvent;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }

        private void RaiseBeforeIsAdoNetProviderEvent()
        {
            var handlers = BeforeCheckIsAdoNetProviderEvent;
            if (handlers != null)
            {
                handlers(this, EventArgs.Empty);
            }
        }
    }
}
