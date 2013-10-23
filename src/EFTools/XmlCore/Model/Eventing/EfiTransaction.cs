// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Eventing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    /// <summary>
    ///     Represents a transaction that encapsulates a set of changes.
    /// </summary>
    internal class EfiTransaction : IDisposable, ITransaction
    {
        internal enum EfiTxStatus
        {
            Open,
            Committed,
            Rolledback,
            Inactive
        };

        private readonly EFArtifact _artifact;
        private readonly string _originatorId;
        private readonly EfiChangeGroup _changes;
        private readonly bool _createdXmlTxn;
        private readonly XmlTransaction _xmlTx;
        private readonly EfiTransactionContext _efiTransactionContext;

        /// <summary>
        ///     Start a new transaction in order to make changes in the artifact
        ///     encapsulated by this service.
        ///     The expectation is that transactions are not kept active for a long
        ///     time but a set of changes is made to them and then they internally
        ///     update the state of the model (compile) and communicate those
        ///     changes as well as any model state changes to whoever is registered
        ///     for change events on this service.
        /// </summary>
        internal EfiTransaction(EFArtifact artifact, string originatorId, string txName)
            : this(artifact, originatorId, true, null)
        {
            _xmlTx = artifact.XmlModelProvider.BeginTransaction(txName, this);
        }

        internal EfiTransaction(EFArtifact artifact, string originatorId, string txName, EfiTransactionContext context)
            : this(artifact, originatorId, true, context)
        {
            _xmlTx = artifact.XmlModelProvider.BeginTransaction(txName, this);
        }

        internal EfiTransaction(EFArtifact artifact, string originatorId, XmlTransaction xmltxn)
            : this(artifact, originatorId, false, null)
        {
            Debug.Assert(xmltxn != null, "Can't pass null in as XmlTransaction");
            _xmlTx = xmltxn;
        }

        internal EfiTransaction(EFArtifact artifact, string originatorId, XmlTransaction xmltxn, EfiTransactionContext context)
            : this(artifact, originatorId, false, context)
        {
            Debug.Assert(xmltxn != null, "Can't pass null in as XmlTransaction");
            _xmlTx = xmltxn;
        }

        private EfiTransaction(EFArtifact artifact, string originatorId, bool createdXmlTxn, EfiTransactionContext context)
        {
            _artifact = artifact;
            _changes = new EfiChangeGroup(this);
            _createdXmlTxn = createdXmlTxn;
            _originatorId = originatorId;
            _efiTransactionContext = context != null ? context : new EfiTransactionContext();
        }

        public string Name
        {
            get { return _xmlTx.Name; }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _createdXmlTxn)
            {
                _xmlTx.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     The only thing that this method does is signal the underlying
        ///     XML transaction to 'complete', which will push changes from the XLinq
        ///     tree in the XML model to the XML Editor's parse tree and the VS buffer.
        ///     This *DOES NOT* commit changes to disk and *DOES NOT* modify the XLinq
        ///     tree.
        ///     An appropriate exception is thrown if this
        ///     operation fails and the transaction is left in Open state to allow
        ///     for the possibility of corrections or retries.  If there are no
        ///     failures, the transaction will be in committed state when this
        ///     operation returns.
        /// </summary>
        public void Commit(bool dirtyArtifact)
        {
            if (_createdXmlTxn)
            {
                try
                {
                    _xmlTx.Commit();
                    if (dirtyArtifact)
                    {
                        _artifact.IsDirty = true;
                    }
                }
                catch
                {
                    // an error ocurred while commiting transaction, and if it is not caused by ReloadArtifact, we need to reload artifact
                    if (!_artifact.IsArtifactReloading)
                    {
                        _artifact.ReloadArtifact();
                    }

                    throw;
                }
            }
        }

        /// <summary>
        ///     Rolls back this transaction, leaving it in rolled back state
        ///     and undo all changes made within the transaction.
        ///     ***WARNING*** This will roll back *only* the XML transaction.
        ///     This will put the EFObject model in an unstable state, prone to crashing.
        /// </summary>
        internal void Rollback()
        {
            if (_createdXmlTxn)
            {
                _xmlTx.Rollback();
            }

            // TODO: Undo and notify
        }

        /// <summary>
        ///     The set of changes encapsulated in this transaction.  This include
        ///     only changes made to EFElement objects in the transaction, as any
        ///     resulting compile/state changes from propagating those changes are
        ///     reported separately.
        /// </summary>
        internal EfiChangeGroup ChangeGroup
        {
            get { return _changes; }
        }

        /// <summary>
        ///     The set of changes that will be made/have been made to the XML Model
        ///     as calculated by the XML Editor. This will be get populated even if the
        ///     set of EFObject changes within an EfiTransaction are not
        ///     populated.
        /// </summary>
        internal IEnumerable<IXmlChange> XmlChanges
        {
            get { return _xmlTx.Changes(); }
        }

        /// <summary>
        ///     An enum indicating the status of the transaction, either
        ///     - Open = ready for more changes,
        ///     - Committed = changes successfully committed to disk, or
        ///     - Rolledback = transaction was not successful and any changes in it were undone.
        /// </summary>
        internal EfiTxStatus Status
        {
            get
            {
                var status = EfiTxStatus.Inactive;
                switch (_xmlTx.Status)
                {
                    case XmlTransactionStatus.Aborted:
                        status = EfiTxStatus.Rolledback;
                        break;
                    case XmlTransactionStatus.Active:
                        status = EfiTxStatus.Open;
                        break;
                    case XmlTransactionStatus.Committed:
                        status = EfiTxStatus.Committed;
                        break;
                    default:
                        status = EfiTxStatus.Inactive;
                        break;
                }

                return status;
            }
        }

        /// <summary>
        ///     The identifier provided by the originator of a transaction.
        /// </summary>
        public string OriginatorId
        {
            get { return _originatorId; }
        }

        /// <summary>
        ///     Given a key retrieve the associated context value.
        /// </summary>
        public T GetContextValue<T>(string key) where T : ITransactionContextItem
        {
            T returnValue;
            if (_efiTransactionContext.TryGetValue(key, out returnValue))
            {
                return returnValue;
            }
            return default(T);
        }

        /// <summary>
        ///     Add context value with given key. The value will be ignored if the key already exists.
        /// </summary>
        public void AddContextValue(string key, ITransactionContextItem value)
        {
            Debug.Assert(_efiTransactionContext.Contains(key) == false, "Key: " + key + " already exists in the context.");
            if (_efiTransactionContext.Contains(key) == false)
            {
                _efiTransactionContext.Add(key, value);
            }
        }
    }
}
