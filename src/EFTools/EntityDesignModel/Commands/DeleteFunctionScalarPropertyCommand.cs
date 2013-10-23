// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to delete a ScalarProperty inside a function mapping.
    /// </summary>
    internal class DeleteFunctionScalarPropertyCommand : DeleteEFElementCommand
    {
        /// <summary>
        ///     Deletes the passed in FunctionScalarProperty
        /// </summary>
        /// <param name="sp"></param>
        internal DeleteFunctionScalarPropertyCommand(FunctionScalarProperty sp)
            : base(sp)
        {
            CommandValidation.ValidateFunctionScalarProperty(sp);
        }

        protected FunctionScalarProperty FunctionScalarProperty
        {
            get
            {
                var elem = EFElement as FunctionScalarProperty;
                Debug.Assert(elem != null, "underlying element does not exist or is not a FunctionScalarProperty");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var fae = FunctionScalarProperty.AssociationEnd;
            var fcp = FunctionScalarProperty.FunctionComplexProperty;
            if (fae != null
                && fae.ScalarProperties().Count == 1)
            {
                // we are the last one, so remove the entire AssociationEnd
                DeleteInTransaction(cpc, FunctionScalarProperty.AssociationEnd);
            }
            else if (fcp != null
                     && fcp.ScalarProperties().Count == 1
                     && fcp.ComplexProperties().Count == 0)
            {
                //  we are about to remove the last item from this FunctionComplexProperty, so remove the entire FunctionComplexProperty
                Debug.Assert(
                    fcp.ScalarProperties()[0] == FunctionScalarProperty,
                    "fcp.ScalarProperties()[0] should be the same as this.FunctionScalarProperty");
                DeleteInTransaction(cpc, fcp);
            }
            else
            {
                // all other cases, just remove the ScalarProperty
                base.InvokeInternal(cpc);
            }
        }
    }
}
