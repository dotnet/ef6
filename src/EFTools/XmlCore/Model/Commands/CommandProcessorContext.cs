// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Integrity;

    internal class CommandProcessorContext
    {
        private readonly EditingContext _editingContext;
        private readonly string _transactionName;
        private readonly string _originatorId;
        private readonly EFArtifact _artifact;
        private readonly Queue<IIntegrityCheck> _integrityChecks = new Queue<IIntegrityCheck>();
        private EfiTransaction _transaction;
        private readonly HashSet<ItemBinding> _itemsToRebind = new HashSet<ItemBinding>();
        private readonly EfiTransactionContext _transactionContext;
        private readonly List<Command> _enqueuedCommands = new List<Command>();

        internal List<Command> EnqueuedCommands
        {
            get { return _enqueuedCommands; }
        }

        // TODO:  enter a bug to remove this ctor.
        internal CommandProcessorContext(EditingContext editingContext, string originatorId, string transactionName)
            : this(editingContext, originatorId, transactionName, null, null)
        {
        }

        internal CommandProcessorContext(EditingContext editingContext, string originatorId, string transactionName, EFArtifact artifact)
            : this(editingContext, originatorId, transactionName, artifact, null)
        {
        }

        internal CommandProcessorContext(
            EditingContext editingContext, string originatorId, string transactionName, EFArtifact artifact,
            EfiTransactionContext transactionContext)
            : this(editingContext, originatorId, transactionName, artifact, transactionContext, false)
        {
        }

        /// <summary>
        ///     Note:  If createTransactionImmediately is set to true, the caller is responsible for calling CommandProcessor's FinalizeTransaction().  This is a bit wonky.  User can spin up a CommandProcessor
        ///     with this context, and then invoke that method on that command processor.
        /// </summary>
        /// <param name="editingContext"></param>
        /// <param name="originatorId"></param>
        /// <param name="transactionName"></param>
        /// <param name="artifact"></param>
        /// <param name="transactionContext"></param>
        /// <param name="createTransactionImmediately"></param>
        internal CommandProcessorContext(
            EditingContext editingContext, string originatorId, string transactionName, EFArtifact artifact,
            EfiTransactionContext transactionContext, bool createTransactionImmediately)
        {
            _editingContext = editingContext;
            _originatorId = originatorId;
            _transactionName = transactionName;
            _artifact = artifact;
            _transactionContext = transactionContext;
            if (createTransactionImmediately)
            {
                _transaction = CreateTransaction();
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1821:RemoveEmptyFinalizers")]
        ~CommandProcessorContext()
        {
            Debug.Assert(_integrityChecks.Count == 0, "There are still checks to invoke");
            Debug.Assert(
                _transaction != null ? _transaction.Status != EfiTransaction.EfiTxStatus.Open : true,
                "A transaction is being GC'd that is still open.  Why is this?");
        }

        internal EfiTransaction CreateTransaction()
        {
            Debug.Assert(!HasOpenTransaction, "We are opening a second transaction for this Context.");

            return _transaction = new EfiTransaction(Artifact, _originatorId, _transactionName, _transactionContext);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal void DisposeTransaction()
        {
            if (_transaction != null)
            {
                try
                {
                    _transaction.Dispose();
                }
                catch
                {
                }
            }
        }

        internal EfiTransaction EfiTransaction
        {
            get { return _transaction; }
        }

        internal EditingContext EditingContext
        {
            get { return _editingContext; }
        }

        internal string OriginatorId
        {
            get { return _originatorId; }
        }

        internal string TransactionName
        {
            get { return _transactionName; }
        }

        internal bool HasOpenTransaction
        {
            get
            {
                if (_transaction != null
                    &&
                    _transaction.Status == EfiTransaction.EfiTxStatus.Open)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal EFArtifact Artifact
        {
            get
            {
                if (_artifact != null)
                {
                    return _artifact;
                }
                else
                {
                    // TODO:  remove this else block after removing the ctor that does not take artifact
                    var context = EditingContext;
                    var service = context.GetEFArtifactService();
                    return service.Artifact;
                }
            }
        }

        internal Queue<IIntegrityCheck> IntegrityChecks
        {
            get { return _integrityChecks; }
        }

        internal void AddIntegrityCheck(IIntegrityCheck newCheck)
        {
            foreach (var check in _integrityChecks)
            {
                // don't add duplicate checks
                if (check.IsEqual(newCheck))
                {
                    return;
                }
            }

            _integrityChecks.Enqueue(newCheck);
        }

        internal void AddBindingForRebind(ItemBinding itemBinding)
        {
            _itemsToRebind.Add(itemBinding);
        }

        internal IEnumerable<ItemBinding> GetBindingsForRebind()
        {
            foreach (var ib in _itemsToRebind)
            {
                yield return ib;
            }
        }

        internal void ClearBindingsForRebind()
        {
            _itemsToRebind.Clear();
        }

        internal void ClearIntegrityChecks()
        {
            _integrityChecks.Clear();
        }
    }
}
