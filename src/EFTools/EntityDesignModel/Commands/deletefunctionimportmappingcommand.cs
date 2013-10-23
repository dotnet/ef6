// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class DeleteFunctionImportMappingCommand : DeleteEFElementCommand
    {
        /// <summary>
        ///     Delete the passed in function import mapping
        /// </summary>
        /// <param name="fim"></param>
        internal DeleteFunctionImportMappingCommand(FunctionImportMapping fim)
            : base(fim)
        {
            CommandValidation.ValidateFunctionImportMapping(fim);
        }

        protected FunctionImportMapping FunctionImportMapping
        {
            get
            {
                var elem = EFElement as FunctionImportMapping;
                Debug.Assert(elem != null, "underlying element does not exist or is not a FunctionImportMapping");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // if we can't find a FunctionImportMapping, don't worry about it.
            // don't use local typed property as this may be null
            if (EFElement != null)
            {
                base.InvokeInternal(cpc);
            }
        }
    }
}
