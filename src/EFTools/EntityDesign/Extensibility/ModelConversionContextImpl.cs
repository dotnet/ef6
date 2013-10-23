// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Xml.Linq;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.VisualStudio;

    internal class ModelConversionContextImpl : ModelConversionExtensionContext, IDisposable
    {
        private readonly Project _project;
        private readonly ProjectItem _projectItem;
        private readonly FileInfo _fileInfo;
        private XDocument _current;
        private string _original;
        private readonly Version _targetSchemaVersion;
        private readonly bool _protectCurrent;
        private readonly List<ExtensionError> _errors = new List<ExtensionError>();
        private bool _isDisposed;

        /// <summary>
        ///     Creates a concrete implementation of ModelConversionExtensionContext, use this ctor when saving
        /// </summary>
        internal ModelConversionContextImpl(
            Project project, ProjectItem projectItem, FileInfo fileInfo, Version targetSchemaVersion, XDocument current)
        {
            Debug.Assert(fileInfo != null, "fileInfo should not be null");
            Debug.Assert(targetSchemaVersion != null, "runtimeVersion should not be null");
            Debug.Assert(current != null, "current should not be null");

            _project = project;
            _projectItem = projectItem;
            _fileInfo = fileInfo;
            _targetSchemaVersion = targetSchemaVersion;
            _current = current;
            _protectCurrent = true;
            _original = string.Empty;

            AddEventHandler();
        }

        /// <summary>
        ///     Creates a concrete implementation of ModelConversionExtensionContext, use this ctor when loading
        /// </summary>
        internal ModelConversionContextImpl(
            Project project, ProjectItem projectItem, FileInfo fileInfo, Version runtimeVersion, string original)
        {
            Debug.Assert(fileInfo != null, "fileInfo should not be null");
            Debug.Assert(runtimeVersion != null, "runtimeVersion should not be null");
            Debug.Assert(original != null, "original should not be null");

            _project = project;
            _projectItem = projectItem;
            _fileInfo = fileInfo;
            _targetSchemaVersion = runtimeVersion;
            _current = XDocument.Parse(EdmUtils.CreateEdmxString(runtimeVersion, String.Empty, String.Empty, String.Empty));
            _protectCurrent = false;
            _original = original;

            AddEventHandler();
        }

        ~ModelConversionContextImpl()
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
            get { return _project; }
        }

        public override FileInfo FileInfo
        {
            get { return _fileInfo; }
        }

        public override Version EntityFrameworkVersion
        {
            get { return _targetSchemaVersion; }
        }

        public override XDocument CurrentDocument
        {
            get { return _current; }
        }

        internal void SetCurrentDocument(XDocument current)
        {
            RemoveEventHandler();
            _current = current;
            AddEventHandler();
        }

        public override string OriginalDocument
        {
            get { return _original; }
            set { _original = value; }
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
            if (_current != null && _protectCurrent)
            {
                _current.Changing += BeforeEventHandler;
            }
        }

        private void RemoveEventHandler()
        {
            if (_current != null && _protectCurrent)
            {
                _current.Changing -= BeforeEventHandler;
            }
        }

        private void OnBeforeChange(object sender, XObjectChangeEventArgs e)
        {
            throw new InvalidOperationException(Resources.Extensibility_CantEditOriginalOnSave);
        }
    }
}
