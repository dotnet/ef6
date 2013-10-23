// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using System.ComponentModel.Design;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Modeling.Shell;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    internal static class Services
    {
        public static ServiceProvider OLEServiceProvider
        {
            get { return new ServiceProvider(IOleServiceProvider); }
        }

        private static IServiceProvider _serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                {
                    _serviceProvider = PackageManager.Package;
                }
                return _serviceProvider;
            }
            set { _serviceProvider = value; }
        }

        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetService(typeof(T)) as T;
        }

        public static DTE DTE
        {
            get { return GetService<DTE>(); }
        }

        public static IMonitorSelectionService IMonitorSelectionService
        {
            get { return GetService<IMonitorSelectionService>(); }
        }

        public static OleMenuCommandService OleMenuCommandService
        {
            get { return GetService<IMenuCommandService>() as OleMenuCommandService; }
        }

        public static IVsMonitorSelection IVsMonitorSelection
        {
            get { return GetService<IVsMonitorSelection>(); }
        }

        public static IMonitorSelectionService DslMonitorSelectionService
        {
            get { return GetService<IMonitorSelectionService>(); }
        }

        public static IVsSolution IVsSolution
        {
            get { return GetService<IVsSolution>(); }
        }

        public static IVsSolutionBuildManager IVsSolutionBuildManager
        {
            get { return GetService<SVsSolutionBuildManager>() as IVsSolutionBuildManager; }
        }

        public static IVsSolutionBuildManager2 IVsSolutionBuildManager2
        {
            get { return GetService<SVsSolutionBuildManager>() as IVsSolutionBuildManager2; }
        }

        public static IVsRunningDocumentTable IVsRunningDocumentTable
        {
            get { return GetService<SVsRunningDocumentTable>() as IVsRunningDocumentTable; }
        }

        public static IOleServiceProvider IOleServiceProvider
        {
            get { return GetService<IOleServiceProvider>(); }
        }

        public static IVsLinkedUndoTransactionManager IVsLinkedUndoTransactionManager
        {
            get { return GetService<SVsLinkedUndoTransactionManager>() as IVsLinkedUndoTransactionManager; }
        }
    }
}
