// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.TestHelpers
{
    using System;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using ModelChangeEventArgs = Microsoft.Data.Entity.Design.VisualStudio.Package.ModelChangeEventArgs;

    internal class MockPackage : IEdmPackage, IDisposable
    {
        private readonly EntityDesignModelManager _manager;

        internal MockPackage(EntityDesignModelManager manager)
        {
            _manager = manager;
            PackageManager.Package = this;
        }

        public IEntityDesignCommandSet CommandSet
        {
            get { return null; }
        }

        public ExplorerWindow ExplorerWindow
        {
            get { return null; }
        }

        public MappingDetailsWindow MappingDetailsWindow
        {
            get { return null; }
        }

        public DocumentFrameMgr DocumentFrameMgr
        {
            get { return null; }
        }

        public ConnectionManager ConnectionManager
        {
            get { return null; }
        }

        public AggregateProjectTypeGuidCache AggregateProjectTypeGuidCache
        {
            get { return null; }
        }

        public ModelGenErrorCache ModelGenErrorCache
        {
            get { return null; }
        }

        public ModelChangeEventListener ModelChangeEventListener
        {
            get { return null; }
        }

        public EntityDesignModelManager ModelManager
        {
            get { return _manager; }
        }

        ModelManager IXmlDesignerPackage.ModelManager
        {
            get { return _manager; }
        }

        public string GetResourceString(string resourceName)
        {
            return string.Empty;
        }

        public event ModelChangeEventHandler FileNameChanged;

        public void OnFileNameChanged(string oldFileName, string newFileName)
        {
            var args = new ModelChangeEventArgs();
            args.OldFileName = oldFileName;
            args.NewFileName = newFileName;
            FileNameChanged(this, args);
        }

        public bool IsBuildingFromCommandLine
        {
            get
            {
                // Command-line builds exclusively instantiate packages, so if we are instantiating this
                // we're not building from the command line
                return false;
            }
        }

        public void SetToolWindowCmdsEnabled(bool enabled)
        {
        }

        public object GetService(Type serviceType)
        {
            return null;
        }

        public bool IsForegroundThread
        {
            get { return true; }
        }

        public void InvokeOnForeground(SimpleDelegateClass.SimpleDelegate simpleDelegate)
        {
        }

        public void Dispose()
        {
            if (_manager != null)
            {
                _manager.Dispose();
            }
            PackageManager.Package = null;
        }
    }
}
