// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml.Linq;
    using EnvDTE;

    internal class ModelTransformContextImpl : ModelTransformExtensionContext, IDisposable
    {
        private readonly ProjectItem _projectItem;
        private XDocument _original;
        private XDocument _current;
        private readonly List<ExtensionError> _errors = new List<ExtensionError>();
        private readonly Version _targetSchemaVersion;
        private bool _isDisposed;

        internal ModelTransformContextImpl(ProjectItem projectItem, Version targetSchemaVersion, XDocument original)
        {
            Debug.Assert(projectItem != null, "projectItem should not be null");
            Debug.Assert(original != null, "original should not be null");

            _projectItem = projectItem;
            _targetSchemaVersion = targetSchemaVersion;
            _original = original;
            _current = XDocument.Parse(original.ToString(), LoadOptions.PreserveWhitespace);

            AddEventHandler();
        }

        ~ModelTransformContextImpl()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                RemoveEventHandler();
                _isDisposed = true;
            }
        }

        public override ProjectItem ProjectItem
        {
            get { return _projectItem; }
        }

        public override Project Project
        {
            get { return _projectItem.ContainingProject; }
        }

        public override Version EntityFrameworkVersion
        {
            get { return _targetSchemaVersion; }
        }

        public override XDocument OriginalDocument
        {
            get { return _original; }
        }

        internal void SetOriginalDocument(XDocument original)
        {
            RemoveEventHandler();
            _original = original;
            AddEventHandler();
        }

        public override XDocument CurrentDocument
        {
            get { return _current; }
            set { _current = value; }
        }

        internal void SetCurrentDocument(XDocument current)
        {
            _current = current;
        }

        public override List<ExtensionError> Errors
        {
            get { return _errors; }
        }

        private EventHandler<XObjectChangeEventArgs> _beforeEvent;

        private EventHandler<XObjectChangeEventArgs> BeforeEventHandler
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

        private void AddEventHandler()
        {
            if (_original != null)
            {
                _original.Changing += BeforeEventHandler;
            }
        }

        private void RemoveEventHandler()
        {
            if (_original != null)
            {
                _original.Changing -= BeforeEventHandler;
            }
        }

        private void OnBeforeChange(object sender, XObjectChangeEventArgs e)
        {
            throw new InvalidOperationException(Resources.Extensibility_CantEditOriginalOnSave);
        }
    }
}
