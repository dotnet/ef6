// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class DeleteFunctionImportCommand : DeleteEFElementCommand
    {
        internal string DeletedFunctionImportName { get; private set; }

        protected FunctionImport FunctionImport
        {
            get
            {
                var elem = EFElement as FunctionImport;
                Debug.Assert(elem != null, "EFElement is null");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        /// <summary>
        ///     Delete the passed in function
        /// </summary>
        /// <param name="fim"></param>
        internal DeleteFunctionImportCommand(FunctionImport fi)
            : base(fi)
        {
            CommandValidation.ValidateFunctionImport(fi);
        }

        public DeleteFunctionImportCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            // Save off the deleted function import name
            DeletedFunctionImportName = FunctionImport.Name.Value;

            base.PreInvoke(cpc);
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // if we can't find a FunctionImport, don't worry about it.
            // don't use local typed property as this may be null
            if (EFElement != null)
            {
                base.InvokeInternal(cpc);
            }
        }
    }
}
