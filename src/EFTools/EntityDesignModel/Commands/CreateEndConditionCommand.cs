// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class CreateEndConditionCommand : Command
    {
        private readonly AssociationSetMapping _associationSetMapping;
        private readonly Property _tableColumn;
        private readonly bool? _isNull;
        private readonly string _conditionValue;
        private Condition _created;

        /// <summary>
        ///     Creates a Condition in the given AssociationSetMapping.
        ///     Valid combinations are:
        ///     1. Send true or false for isNull, and null for conditionValue
        ///     2. Send null for isNull, and a non-empty string for conditionValue
        ///     3. Send null for isNull, and null for conditionValue
        ///     You cannot send non-null values to both arguments.
        /// </summary>
        /// <param name="mappingFragment">The AssociationSetMapping to place this Condition; cannot be null.</param>
        /// <param name="tableColumn">This must be a valid Property from the S-Model.</param>
        internal CreateEndConditionCommand(
            AssociationSetMapping associationSetMapping, Property tableColumn, bool? isNull, string conditionValue)
        {
            CommandValidation.ValidateAssociationSetMapping(associationSetMapping);
            CommandValidation.ValidateTableColumn(tableColumn);

            _associationSetMapping = associationSetMapping;
            _tableColumn = tableColumn;
            _isNull = isNull;
            _conditionValue = conditionValue;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var cond = new Condition(_associationSetMapping, null);
            cond.ColumnName.SetRefName(_tableColumn);
            _associationSetMapping.AddCondition(cond);

            ModelHelper.SetConditionPredicate(cond, _isNull, _conditionValue);

            XmlModelHelper.NormalizeAndResolve(cond);

            _created = cond;
        }

        internal Condition Condition
        {
            get { return _created; }
        }
    }
}
