// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Globalization;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal class UpdateDefaultableValueCommand<T> : Command
    {
        internal DefaultableValue<T> DefaultableValue { get; set; }
        internal T Value { get; set; }

        public UpdateDefaultableValueCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal UpdateDefaultableValueCommand(DefaultableValue<T> defaultableValue, T value)
        {
            DefaultableValue = defaultableValue;
            Value = value;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // if the _value is null it means that we are removing underlying XAttribute so we don't validate that
            if (Value != null
                && DefaultableValue.IsValidValue(Value) == false)
            {
                var msg = string.Format(CultureInfo.CurrentCulture, Resources.INVALID_FORMAT, Value);
                throw new CommandValidationFailedException(msg);
            }

            DefaultableValue.Value = Value;
        }
    }
}
