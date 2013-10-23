// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class SetRowsAffectedParameterCommand : Command
    {
        private readonly ModificationFunction _modificationFunction;
        private readonly Parameter _param;

        internal SetRowsAffectedParameterCommand(ModificationFunction modificationFunction, Parameter param)
        {
            Debug.Assert(
                modificationFunction != null,
                typeof(SetRowsAffectedParameterCommand).Name + ": constructor cannot operate on null ModificationFunction");

            _modificationFunction = modificationFunction;
            _param = param;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // validate parameter
            if (null != _param)
            {
                // check that the Parameter is suitable for use as a RowsAffectedParameter
                if (!_param.CanBeUsedAsRowsAffectedParameter())
                {
                    var errMsg = string.Format(
                        CultureInfo.CurrentCulture, Resources.SetRowsAffectedParameterErrorMessage_CannotUse, _param.NormalizedNameExternal,
                        _param.InOut.ToString(), _param.Type.Value);
                    throw new CommandValidationFailedException(errMsg);
                }
            }

            // if _param is null this will delete the RowsAffectedParameter (which is as expected)
            _modificationFunction.RowsAffectedParameter.SetRefName(_param);

            XmlModelHelper.NormalizeAndResolve(_modificationFunction);
        }
    }
}
