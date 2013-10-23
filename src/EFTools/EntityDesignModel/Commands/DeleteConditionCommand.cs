// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class DeleteConditionCommand : DeleteEFElementCommand
    {
        internal string OriginalConceptualEntityName { get; private set; }
        internal string OriginalStorageEntityName { get; private set; }
        internal string OriginalStoragePropertyName { get; private set; }

        protected Condition Condition
        {
            get
            {
                var elem = EFElement as Condition;
                Debug.Assert(elem != null, "underlying element does not exist or is not a Condition");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        /// <summary>
        ///     Deletes the passed in Condition
        /// </summary>
        /// <param name="cond"></param>
        internal DeleteConditionCommand(Condition cond)
            : base(cond)
        {
            CommandValidation.ValidateCondition(cond);
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            var fragment = Condition.MappingFragment;
            if (fragment != null)
            {
                EnforceEntitySetMappingRules.AddRule(cpc, Condition);
            }

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
