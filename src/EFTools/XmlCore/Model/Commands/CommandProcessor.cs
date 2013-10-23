// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     This class is able to invoke a queue of commands with the required "context" around them, i.e.,
    ///     creating a transaction, running IntegrityChecks, routing to views, etc.
    /// </summary>
    internal class CommandProcessor
    {
        private readonly CommandProcessorContext _cpc;
        private readonly bool _shouldNotifyObservers;
        private readonly Queue<Command> _commands = new Queue<Command>();

        /// <summary>
        ///     Creates a CommandProcessor given a context and shouldNotifyObservers flag. You must later call EnqueueCommand(), or else there won't be any work.
        /// </summary>
        /// <param name="cpc">CommandProcessorContext</param>
        /// <param name="shouldNotifyObservers">Whether observers should be notified.</param>
        internal CommandProcessor(CommandProcessorContext cpc, bool shouldNotifyObservers)
        {
            Debug.Assert(cpc != null, "cpc cannot be null");
            if (cpc == null)
            {
                throw new ArgumentNullException("cpc");
            }

            _cpc = cpc;
            _shouldNotifyObservers = shouldNotifyObservers;
        }

        /// <summary>
        ///     Creates a new instance of CommandProcessor.
        /// </summary>
        /// <param name="cpc">CommandProcessorContext</param>
        /// <param name="shouldNotifyObservers">Whether observers should be notified.</param>
        /// <param name="commands">The commands are placed in a queue</param>
        internal CommandProcessor(CommandProcessorContext cpc, bool shouldNotifyObservers, params Command[] commands)
            : this(cpc, shouldNotifyObservers)
        {
            if (commands != null)
            {
                foreach (var command in commands)
                {
                    _commands.Enqueue(command);
                }
            }
        }

        /// <summary>
        ///     Initializes a new instance of CommandProcessor.
        /// </summary>
        /// <param name="cpc">CommandProcessorContext.</param>
        /// <param name="commands">The commands are placed in the queue</param>
        internal CommandProcessor(CommandProcessorContext cpc, params Command[] commands)
            : this(cpc, true, commands)
        {
        }

        /// <summary>
        ///     Creates a CommandProcessor.
        /// </summary>
        /// <param name="cpc">CommandProcessorContext</param>
        /// <param name="commands">These commands are placed in the queue (order is based on the enumerator for the collection)</param>
        internal CommandProcessor(CommandProcessorContext cpc, ICollection<Command> commands)
            : this(cpc)
        {
            if (commands != null)
            {
                foreach (var command in commands)
                {
                    _commands.Enqueue(command);
                }
            }
        }

        /// <summary>
        ///     Creates a CommandProcessor, and uses the passed in information to create a CommandProcessorContext.
        ///     You must later call EnqueueCommand(), or else there won't be any work.
        /// </summary>
        internal CommandProcessor(EditingContext context, string originatorId, string transactionName)
            : this(new CommandProcessorContext(context, originatorId, transactionName))
        {
        }

        /// <summary>
        ///     Creates a CommandProcessor, and uses the passed in information to create a CommandProcessorContext.
        /// </summary>
        /// <param name="command">This command is placed in the queue</param>
        internal CommandProcessor(EditingContext context, string originatorId, string transactionName, params Command[] commands)
            : this(context, originatorId, transactionName)
        {
            if (commands != null)
            {
                foreach (var c in commands)
                {
                    _commands.Enqueue(c);
                }
            }
        }

        /// <summary>
        ///     Creates a CommandProcessor, and uses the passed in information to create a CommandProcessorContext.
        /// </summary>
        /// <param name="commands">These commands are placed in the queue (order is based on the enumerator for the collection)</param>
        internal CommandProcessor(EditingContext context, string originatorId, string transactionName, ICollection<Command> commands)
            : this(context, originatorId, transactionName)
        {
            if (commands != null)
            {
                foreach (var command in commands)
                {
                    _commands.Enqueue(command);
                }
            }
        }

        /// <summary>
        ///     Access the CommandProcessorContext
        /// </summary>
        internal CommandProcessorContext CommandProcessorContext
        {
            get { return _cpc; }
        }

        /// <summary>
        ///     Returns the number of enqueued commands
        /// </summary>
        internal int CommandCount
        {
            get { return _commands.Count; }
        }

        /// <summary>
        ///     Adds a command to the queue
        /// </summary>
        /// <param name="command"></param>
        internal void EnqueueCommand(Command command)
        {
            Debug.Assert(command != null, "command cannot be null");
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            _commands.Enqueue(command);
        }

        internal void EnqueueCommands(IEnumerable<Command> commands)
        {
            foreach (var command in commands)
            {
                EnqueueCommand(command);
            }
        }

        /// <summary>
        ///     Invokes the commands in the queue.  If any command throws an exception, the transaction
        ///     will be rolled back and this will rethrow.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly",
            Justification = @"Risky to change to InvalidOperationException since it has many upstream callers")]
        internal void Invoke()
        {
            // just to be safe, the c'tor should have caught this
            Debug.Assert(_cpc != null, "cpc cannot be null");
            if (_cpc == null)
            {
                throw new ArgumentNullException("_cpc");
            }

            // if there is nothing to do, just return
            if (_commands.Count == 0
                && _cpc.IntegrityChecks.Count == 0)
            {
                return;
            }

            // Note down whether or not the artifact was originally dirty, since we don't want hydration commands marking the file
            // as clean if there are uncommited diagram layout edits for instance.
            var artifactInitiallyDirty = _cpc.Artifact.IsDirty;

            // First add the commands to the command processor context so that the top-level processor can determine
            // how to post-process based on them.
            _cpc.EnqueuedCommands.AddRange(_commands);

            // We need to set the command processor since the behavior methods query this.
            foreach (var command in _commands)
            {
                command.CommandProcessor = this;
            }

            // since the Invoke() method is the gateway towards editing the model, we should first
            // ask ourselves if we can perform edits. This query is propagated down to the XML model level
            // where we base our answer on what the SCC provider tells us if we are in VS.
            // But we should only do this for the top-level transaction (otherwise we end up calling 
            // VsXmlModel.CanEditXmlModel() for _every_ nested Command which causes perf problems
            // for transactions which use a large number of nested commands e.g. Update Model on a 
            // large database)
            // Note: this means we do not support multi-artifact transactions. If we started doing wanting
            // to support multi-artifact transactions then the approach below would need to be expanded to
            // call CanEditArtifact() for each artifact in the transaction
            if (!_cpc.HasOpenTransaction)
            {
                if (!_cpc.Artifact.CanEditArtifact())
                {
                    var documentPath = String.Empty;
                    var uri = _cpc.Artifact.Uri;
                    try
                    {
                        documentPath = uri.LocalPath;
                    }
                    catch (InvalidOperationException)
                    {
                        Debug.Fail("Could not parse the LocalPath from the URI for this particular Artifact");
                    }
                    throw new FileNotEditableException(
                        String.Format(CultureInfo.CurrentCulture, Resources.FileNotEditableErrorMessage, documentPath));
                }
            }

            var tx = _cpc.EfiTransaction;
            var isOuterMostTransaction = false;

            try
            {
                // open a local transaction if the cpc doesn't have one
                if (_cpc.HasOpenTransaction == false)
                {
                    tx = _cpc.CreateTransaction();
                    isOuterMostTransaction = true;
                }
                Debug.Assert(isOuterMostTransaction ? tx != null : true, "Why is the transaction null & isOuterMostTransaction true?");

                // invoke all of the commands in our queue
                while (_commands.Count > 0)
                {
                    var command = _commands.Dequeue();
                    command.Invoke(this);
                }

                // if we are managing the outer most transaction, do our post-processing
                if (isOuterMostTransaction)
                {
                    PostProcessUpdate(_cpc, tx, artifactInitiallyDirty);
                }
            }
            catch (Exception e)
            {
                if (isOuterMostTransaction
                    &&
                    tx != null
                    &&
                    tx.Status == EfiTransaction.EfiTxStatus.Open)
                {
                    tx.Rollback();
                }

                // BUGBUG: 557416

                if (isOuterMostTransaction)
                {
                    _cpc.ClearBindingsForRebind();
                    _cpc.ClearIntegrityChecks();
                }

                if (!(e is CommandValidationFailedException))
                {
                    Debug.Fail("Exception thrown inside CommandProcessor. " + e.Message);
                }

                throw;
            }
            finally
            {
                if (isOuterMostTransaction)
                {
                    _cpc.DisposeTransaction();
                }
            }
        }

        internal void FinalizeTransaction(bool artifactInitiallyDirty)
        {
            try
            {
                PostProcessUpdate(_cpc, _cpc.EfiTransaction, artifactInitiallyDirty);
            }
            finally
            {
                _cpc.DisposeTransaction();
            }
        }

        private void PostProcessUpdate(CommandProcessorContext cpc, EfiTransaction tx, bool artifactInitiallyDirty)
        {
            var setUndoScope = false;
            try
            {
                // process those checks that need to run in the originating xact
                while (cpc.IntegrityChecks.Count > 0)
                {
                    // peek for the next check and invoke it, don't dequeue it so we
                    // won't add dupes and recurse forever
                    var check = cpc.IntegrityChecks.Peek();
                    check.Invoke();

                    // now pop it off the queue
                    cpc.IntegrityChecks.Dequeue();
                }

                if (cpc.EditingContext.ParentUndoUnitStarted == false)
                {
                    cpc.EditingContext.ParentUndoUnitStarted = true;
                    cpc.Artifact.XmlModelProvider.BeginUndoScope(cpc.EfiTransaction.Name);
                    setUndoScope = true;
                }

                if (_shouldNotifyObservers)
                {
                    cpc.Artifact.ModelManager.BeforeCommitChangeGroups(cpc);
                }

                // Do not mark the artifact as clean if the artifact was initially dirty before commands
                // were executed... otherwise we may lose information like diagram layout and configurations.
                // Also, translation rules can perform immediate changes to configurations which will dirty the artifact
                // but are not recorded through the enqueued commands. So we should not set the artifact to clean in this case.
                tx.Commit(!artifactInitiallyDirty);
                cpc.DisposeTransaction();

#if DEBUG
                var visitor = cpc.Artifact.GetVerifyModelIntegrityVisitor(true, true, true, true, true);
                visitor.Traverse(cpc.Artifact);

                if (visitor.ErrorCount > 0)
                {
                    Debug.WriteLine("Model Integrity Verifier found " + visitor.ErrorCount + " error(s):");
                    Debug.WriteLine(visitor.AllSerializedErrors);
                    Debug.Assert(
                        false, "Model Integrity Verifier found " + visitor.ErrorCount + " error(s). See the Debug console for details.");
                }
#endif

                if (_shouldNotifyObservers)
                {
                    cpc.Artifact.ModelManager.RouteChangeGroups();
                }
                else
                {
                    // Changegroups have been recorded in the model manager;
                    // if we don't clear them they will be routed on the next observable transaction.
                    cpc.Artifact.ModelManager.ClearChangeGroups();
                }
            }
            finally
            {
                if (setUndoScope)
                {
                    cpc.Artifact.XmlModelProvider.EndUndoScope();
                    cpc.EditingContext.ParentUndoUnitStarted = false;
                }
            }
        }

        /// <summary>
        ///     Invokes a single command against the CommandProcessor.
        /// </summary>
        internal static void InvokeSingleCommand(CommandProcessorContext cpc, Command cmd)
        {
            var cp = new CommandProcessor(cpc, cmd);
            cp.Invoke();
        }
    }
}
