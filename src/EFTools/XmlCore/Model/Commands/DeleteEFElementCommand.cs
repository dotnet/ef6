// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Integrity;

    /// <summary>
    ///     A generic command that can delete an EFElement
    /// </summary>
    internal class DeleteEFElementCommand : Command
    {
        private EFElement _element;
        private bool _rebindAllBindings = true;
        private readonly bool _removeAntiDeps = true;

        internal DeleteEFElementCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Deletes the passed in EFElement
        /// </summary>
        /// <param name="element">You can pass NULL to this, but if you do you must set the EFElement before calling InvokeInternal (or override it for specific functionality)</param>
        internal DeleteEFElementCommand(EFElement element)
        {
            _element = element;
        }

        /// <summary>
        ///     Deletes the passed in EFElement
        /// </summary>
        /// <param name="element">You can pass NULL to this, but if you do you must set the EFElement before calling InvokeInternal (or override it for specific functionality)</param>
        /// <param name="rebindAllBindings">Passing true (the default) causes all bindings in the artifact to rebind</param>
        internal DeleteEFElementCommand(EFElement element, bool rebindAllBindings)
        {
            _element = element;
            _rebindAllBindings = rebindAllBindings;
        }

        /// <summary>
        ///     Deletes the passed in EFElement
        /// </summary>
        /// <param name="element">You can pass NULL to this, but if you do you must set the EFElement before calling InvokeInternal (or override it for specific functionality)</param>
        /// <param name="rebindAllBindings">Passing true (the default) causes all bindings in the artifact to rebind</param>
        /// <param name="removeAntiDeps">Passing true (the default) causes anti-dependencies to be deleted</param>
        internal DeleteEFElementCommand(EFElement element, bool rebindAllBindings, bool removeAntiDeps)
        {
            _element = element;
            _rebindAllBindings = rebindAllBindings;
            _removeAntiDeps = removeAntiDeps;
        }

        /// <summary>
        ///     Gets or sets the EFElement to delete
        /// </summary>
        internal EFElement EFElement
        {
            get { return _element; }
            set { _element = value; }
        }

        /// <summary>
        ///     Determines whether all of the bindings in the artifact will be rebound when this item is deleted
        /// </summary>
        internal bool RebindAllBindings
        {
            get { return _rebindAllBindings; }
            set { _rebindAllBindings = value; }
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            base.PreInvoke(cpc);
            if (_removeAntiDeps)
            {
                RemoveAntiDeps(cpc);
            }
        }

        protected virtual void RemoveAntiDeps(CommandProcessorContext cpc)
        {
            foreach (var antiDep in _element.GetAntiDependenciesOfType<EFElement>())
            {
                DeleteInTransaction(cpc, antiDep);
            }

#if DEBUG
            var antiDeps = _element.GetAntiDependencies();
            Debug.Assert(antiDeps.Count == 0, "The object being deleted still has antiDeps");
#endif
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(_element != null, "InvokeInternal is called when _element is null");

            if (_element == null)
            {
                throw new InvalidOperationException();
            }

            if (_rebindAllBindings)
            {
                CheckArtifactBindings.ScheduleChildAntiDependenciesForRebinding(cpc, _element);
            }

            // delete the item
            _element.Delete();
        }

        /// <summary>
        ///     This will create a transaction if there isn't one already. If the CommandProcessorContext is already
        ///     tracking a transaction, then a new one is NOT created. This specific overload allows modifying the
        ///     command before invoking it (i.e. adding a PostInvokeEvent)
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="cmd"></param>
        internal static void DeleteInTransaction(CommandProcessorContext cpc, DeleteEFElementCommand cmd)
        {
            DeleteInTransaction(cpc, cmd, true);
        }

        /// <summary>
        ///     This will create a transaction if there isn't one already.  If the CommandProcessorContext is already
        ///     tracking a transaction, then a new one is NOT created.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="element">The EFElement to delete</param>
        internal static void DeleteInTransaction(CommandProcessorContext cpc, EFElement element)
        {
            var cmd = element.GetDeleteCommand();
            DeleteInTransaction(cpc, cmd, true);
        }

        /// <summary>
        ///     This will create a transaction if there isn't one already.  If the CommandProcessorContext is already
        ///     tracking a transaction, then a new one is NOT created.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="element">The EFElement to delete</param>
        /// <param name="rebindAllBindings">Control whether all bindings in the artifact should be rebound</param>
        internal static void DeleteInTransaction(CommandProcessorContext cpc, DeleteEFElementCommand cmd, bool rebindAllBindings)
        {
            cmd.RebindAllBindings = rebindAllBindings;
            CommandProcessor.InvokeSingleCommand(cpc, cmd);
        }
    }
}
