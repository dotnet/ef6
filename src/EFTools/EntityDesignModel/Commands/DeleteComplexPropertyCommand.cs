// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class DeleteComplexPropertyCommand : DeleteEFElementCommand
    {
        /// <summary>
        ///     Deletes the passed in ComplexProperty
        /// </summary>
        /// <param name="sp"></param>
        internal DeleteComplexPropertyCommand(ComplexProperty cp)
            : base(cp)
        {
            CommandValidation.ValidateComplexProperty(cp);
        }

        protected ComplexProperty ComplexProperty
        {
            get
            {
                var elem = EFElement as ComplexProperty;
                Debug.Assert(elem != null, "underlying element does not exist or is not a ComplexProperty");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var complexProperty = ComplexProperty.Parent as ComplexProperty;
            if (complexProperty != null
                && complexProperty.ScalarProperties().Count == 0
                && complexProperty.ComplexProperties().Count == 1)
            {
                // if we are about to remove the last item from this ComplexProperty, just remove it
                Debug.Assert(
                    complexProperty.ComplexProperties()[0] == ComplexProperty,
                    "complexProperty.ComplexProperties()[0] should be the same as this.ComplexProperty");
                DeleteInTransaction(cpc, complexProperty);
            }
            else
            {
                base.InvokeInternal(cpc);
            }
        }
    }
}
