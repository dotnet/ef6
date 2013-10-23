// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal interface IChangeScopeContainer
    {
        void OnScopeDisposed();
    }

    internal class ChangeScopeImpl : EntityDesignerChangeScope, IDisposable
    {
        private readonly EfiTransaction _efiTransaction;
        private readonly EditingContext _editingContext;
        private readonly bool _trustedExtension;
        private readonly IChangeScopeContainer _container;
        private HashSet<string> _namespaces;
        private bool _isComplete;

        public ChangeScopeImpl(
            EfiTransaction efiTransaction, EditingContext editingContext, byte[] extensionToken, IChangeScopeContainer container)
        {
            _editingContext = editingContext;
            _efiTransaction = efiTransaction;
            _container = container;

            if (extensionToken != null)
            {
                _trustedExtension = IsExtensionTrusted(extensionToken);
            }

            AddEventHandler();
        }

        ~ChangeScopeImpl()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                Debug.Assert(disposing, "A ChangeScope instance was not disposed of correctly");
                if (disposing)
                {
                    if (_efiTransaction.Status == EfiTransaction.EfiTxStatus.Open)
                    {
                        _efiTransaction.Rollback();
                    }
                    _container.OnScopeDisposed();
                }
            }
            finally
            {
                if (disposing)
                {
                    RemoveEventHandler();
                }
            }

            base.Dispose(disposing);
        }

        public override void Complete()
        {
            _editingContext.GetEFArtifactService().Artifact.XmlModelProvider.BeginUndoScope(_efiTransaction.Name);
            try
            {
                _efiTransaction.Commit(true);
                _isComplete = true;
            }
            finally
            {
                _editingContext.GetEFArtifactService().Artifact.XmlModelProvider.EndUndoScope();
            }
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

        private EventHandler<XObjectChangeEventArgs> _afterEvent;

        private EventHandler<XObjectChangeEventArgs> AfterEventHandler
        {
            get
            {
                if (_afterEvent == null)
                {
                    _afterEvent = OnAfterChange;
                }
                return _afterEvent;
            }
        }

        // checks the extension's public key token against our own
        private static bool IsExtensionTrusted(byte[] extensionToken)
        {
            var thisToken = typeof(ChangeScopeImpl).Assembly.GetName().GetPublicKeyToken();
            var match = true;

            if (thisToken.Length == extensionToken.Length)
            {
                for (var i = 0; i < thisToken.Length; i++)
                {
                    if (thisToken[i] != extensionToken[i])
                    {
                        match = false;
                        break;
                    }
                }
            }
            else
            {
                match = false;
            }

            return match;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void OnBeforeChange(object sender, XObjectChangeEventArgs e)
        {
            if (_isComplete)
            {
                throw new InvalidOperationException(Resources.Extensibility_ChangeScope_AlreadyComplete);
            }

            var valid = false;

            if (_trustedExtension)
            {
                valid = true;
            }
            else
            {
                var node = sender as XObject;
                if (node is XElement)
                {
                    valid = VerifyUserEdit(node as XElement);
                }
                else if (node is XAttribute)
                {
                    valid = VerifyUserEdit(node as XAttribute);
                }
                else
                {
                    if (node.Parent == null
                        && e == XObjectChangeEventArgs.Add)
                    {
                        // if we aren't an element or attribute, and we don't have
                        // a parent yet, assume we are valid for now ('after' event will check
                        // since it will be hooked up to its parent then)
                        valid = true;
                    }
                    else
                    {
                        // might be a text node, whitespace node, etc., so find its first XElement parent
                        while (node.Parent != null)
                        {
                            node = node.Parent;
                            if (node is XElement)
                            {
                                valid = VerifyUserEdit(node as XElement);
                                break;
                            }
                        }
                    }
                }
            }

            if (!valid)
            {
                throw new InvalidOperationException(Resources.Extensibility_ChangeScope_EditingWrongNamespace);
            }
        }

        private void OnAfterChange(object sender, XObjectChangeEventArgs e)
        {
            var valid = false;

            if (_trustedExtension)
            {
                valid = true;
            }
            else
            {
                var node = sender as XObject;
                if (node is XElement
                    || node is XAttribute)
                {
                    // this was validated by the 'before' event
                    valid = true;
                }
                else
                {
                    if (node.Parent == null
                        && e == XObjectChangeEventArgs.Remove)
                    {
                        // we are removing something besides an element or attribute
                        // and we don't have a parent so assume we are ok ('before' event
                        // would have caught a bad edit)
                        valid = true;
                    }
                    else
                    {
                        // might be a text node, whitespace node, etc., so find its first XElement parent
                        while (node.Parent != null)
                        {
                            node = node.Parent;
                            var xElement = node as XElement;
                            if (xElement != null)
                            {
                                valid = VerifyUserEdit(xElement);
                                break;
                            }
                        }
                    }
                }
            }

            if (!valid)
            {
                throw new InvalidOperationException(Resources.Extensibility_ChangeScope_EditingWrongNamespace);
            }
        }

        private bool VerifyUserEdit(XElement xe)
        {
            return VerifyUserEdit(xe.Name);
        }

        private bool VerifyUserEdit(XAttribute xa)
        {
            return VerifyUserEdit(xa.Name);
        }

        private bool VerifyUserEdit(XName xn)
        {
            if (_namespaces == null)
            {
                _namespaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var v in EntityFrameworkVersion.GetAllVersions())
                {
                    foreach (var s in SchemaManager.GetAllNamespacesForVersion(v))
                    {
                        _namespaces.Add(s);
                    }
                }
            }

            if (_namespaces.Contains(xn.NamespaceName))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private void AddEventHandler()
        {
            _editingContext.GetEFArtifactService().Artifact.XDocument.Changing += BeforeEventHandler;
            _editingContext.GetEFArtifactService().Artifact.XDocument.Changed += AfterEventHandler;
        }

        private void RemoveEventHandler()
        {
            _editingContext.GetEFArtifactService().Artifact.XDocument.Changing -= BeforeEventHandler;
            _editingContext.GetEFArtifactService().Artifact.XDocument.Changed -= AfterEventHandler;
        }
    }
}
