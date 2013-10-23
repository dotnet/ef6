// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class ChangeConditionColumnCommand : Command
    {
        internal Condition Condition { get; private set; }
        internal Property NewStorageProperty { get; private set; }
        internal string OriginalStoragePropertyName { get; private set; }
        internal string OriginalConceptualEntityName { get; private set; }
        internal string OriginalStorageEntityName { get; private set; }

        /// <summary>
        ///     Changes the table column used by a Condition.   This may cause the condition to move to the
        ///     other EntityTypeMapping, which is done via a delete and create.
        /// </summary>
        /// <param name="cond">A valid Condition; this cannot be null.</param>
        /// <param name="tableColumn">This must be a valid Property from the S-Model.</param>
        internal ChangeConditionColumnCommand(Condition cond, Property tableColumn)
        {
            CommandValidation.ValidateCondition(cond);
            CommandValidation.ValidateTableColumn(tableColumn);

            Condition = cond;
            NewStorageProperty = tableColumn;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // don't do anything if we are trying to set the same thing
            if (string.Compare(Condition.ColumnName.RefName, NewStorageProperty.LocalName.Value, StringComparison.CurrentCulture) != 0)
            {
                Condition.ColumnName.SetRefName(NewStorageProperty);
                XmlModelHelper.NormalizeAndResolve(Condition);

                EnforceEntitySetMappingRules.AddRule(cpc, Condition);
            }
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            // Save off the conceptual and storage entity names
            var conceptualEntityType = Condition.FirstBoundConceptualEntityType;
            var storageEntityType = Condition.BoundStorageEntityType;
            if (conceptualEntityType != null
                && storageEntityType != null
                && Condition.ColumnName.Target != null)
            {
                OriginalConceptualEntityName = conceptualEntityType.Name.Value;
                OriginalStorageEntityName = storageEntityType.Name.Value;
                OriginalStoragePropertyName = Condition.ColumnName.Target.Name.Value;
            }
            base.PreInvoke(cpc);
        }
    }
}
