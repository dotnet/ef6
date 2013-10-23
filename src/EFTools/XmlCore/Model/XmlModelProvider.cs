// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml.Linq;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public abstract class XmlModelProvider : IDisposable
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        protected XmlModelProvider()
        {
            IsDisposed = false;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="disposing">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <remarks>
        ///     Return the VisualStudio TextSpan object for the given xobject.  If the given xobject is null, this
        ///     will return a new TextSpan with values of 0.
        ///     Virtual to allow mocking.
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "This is a desirable name")]
        public virtual TextSpan GetTextSpanForXObject(XObject xobject, Uri uri)
        {
            Debug.Assert(uri != null, "uri != null");

            return xobject != null
                       ? GetXmlModel(uri).GetTextSpan(xobject)
                       : new TextSpan();
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract XmlModel GetXmlModel(Uri source);

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="source">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public virtual void CloseXmlModel(Uri source)
        {
            // if the last xml-model is closed, dispose the model-provider as well.
            var isThereOpenXmlModel = false;
            foreach (var model in OpenXmlModels)
            {
                if (model.IsDisposed == false)
                {
                    isThereOpenXmlModel = true;
                    break;
                }
            }

            if (isThereOpenXmlModel == false)
            {
                Dispose();
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract IEnumerable<XmlModel> OpenXmlModels { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public event EventHandler<XmlTransactionEventArgs> TransactionCompleted;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="args">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected virtual void OnTransactionCompleted(XmlTransactionEventArgs args)
        {
            if (TransactionCompleted != null)
            {
                TransactionCompleted(this, args);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public event EventHandler<XmlTransactionEventArgs> UndoRedoCompleted;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="args">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected virtual void OnUndoRedoCompleted(XmlTransactionEventArgs args)
        {
            if (UndoRedoCompleted != null)
            {
                UndoRedoCompleted(this, args);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="name">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="userState">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public abstract XmlTransaction BeginTransaction(string name, object userState);

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract XmlTransaction CurrentTransaction { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="oldName">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="newName">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public abstract bool RenameXmlModel(Uri oldName, Uri newName);

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="name">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public virtual void BeginUndoScope(string name)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public virtual void EndUndoScope()
        {
        }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class XmlTransactionEventArgs : EventArgs
    {
        private readonly XmlTransaction _tx;
        private readonly bool _designerTransaction;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="transaction">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="designerTransaction">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public XmlTransactionEventArgs(XmlTransaction transaction, bool designerTransaction)
        {
            _tx = transaction;
            _designerTransaction = designerTransaction;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public XmlTransaction Transaction
        {
            get { return _tx; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public bool DesignerTransaction
        {
            get { return _designerTransaction; }
        }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public enum XmlTransactionStatus
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        Aborted,

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        Active,

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        Committed,
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public abstract class XmlTransaction : IDisposable
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="disposing">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract XmlModelProvider Provider { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract XmlTransaction Parent { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract XmlTransactionStatus Status { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public abstract IEnumerable<IXmlChange> Changes();

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="model">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public abstract IEnumerable<IXmlChange> Changes(XmlModel model);

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract void Commit();

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract void Rollback();

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract object UserState { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public abstract object UndoUserState { get; }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    [Serializable]
    public class XmlTransactionException : Exception
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public XmlTransactionException()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="message">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public XmlTransactionException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="message">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="inner">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        public XmlTransactionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="info">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="context">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected XmlTransactionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        // Satisfies rule ImplementISerializableCorrectly.
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="info">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="context">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public interface IXmlChange
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        XObject Node { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        XObjectChange Action { get; }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public interface IXmlNodeChange : IXmlChange
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        XObject NextNode { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        XContainer Parent { get; }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public interface IXmlAddNodeChange : IXmlNodeChange
    {
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public interface IXmlRemoveNodeChange : IXmlNodeChange
    {
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public interface IXmlNodeNameChange : IXmlChange
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        XName OldName { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        XName NewName { get; }
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public interface IXmlNodeValueChange : IXmlChange
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        string OldValue { get; }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        string NewValue { get; }
    }
}
