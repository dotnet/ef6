// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class ChangeConditionPredicateCommand : Command
    {
        internal Condition Condition { get; set; }
        internal bool? _isNull { get; set; }
        internal string _conditionValue { get; set; }

        /// <summary>
        ///     Changes the value portion of a condition.  Both isNull and conditionValue cannot be non-null.
        ///     Valid combinations are:
        ///     1. Send true or false for isNull, and null for conditionValue
        ///     2. Send null for isNull, and a non-empty string for conditionValue
        ///     3. Send null for isNull, and null for conditionValue
        ///     You cannot send non-null values to both arguments.
        /// </summary>
        /// <param name="cond">A valid Condition; this cannot be null.</param>
        /// <param name="isNull">Change the isNull condition; send null to clear this out.</param>
        /// <param name="conditionValue">Change the Value; send an empty string to clear this out.</param>
        internal ChangeConditionPredicateCommand(Condition cond, bool? isNull, string conditionValue)
        {
            CommandValidation.ValidateCondition(cond);

            Condition = cond;
            _isNull = isNull;
            _conditionValue = conditionValue;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            ModelHelper.SetConditionPredicate(Condition, _isNull, _conditionValue);
            XmlModelHelper.NormalizeAndResolve(Condition);
        }
    }
}
