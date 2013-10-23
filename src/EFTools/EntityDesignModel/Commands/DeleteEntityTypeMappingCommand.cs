// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class DeleteEntityTypeMappingCommand : DeleteEFElementCommand
    {
        internal ConceptualEntityType UnmappedEntityType { get; private set; }

        /// <summary>
        ///     Deletes the passed in EntityTypeMapping
        /// </summary>
        /// <param name="etm"></param>
        internal DeleteEntityTypeMappingCommand(EntityTypeMapping etm)
            : base(etm)
        {
            CommandValidation.ValidateEntityTypeMapping(etm);
        }

        protected EntityTypeMapping EntityTypeMapping
        {
            get
            {
                var elem = EFElement as EntityTypeMapping;
                Debug.Assert(elem != null, "underlying element does not exist or is not an EntityTypeMapping");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        private void SaveDeletedInformation()
        {
            UnmappedEntityType = EntityTypeMapping.FirstBoundConceptualEntityType;
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            // save off the deleted entity type name
            SaveDeletedInformation();
            EnforceEntitySetMappingRules.AddRule(cpc, EntityTypeMapping.EntitySetMapping);
            base.PreInvoke(cpc);
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var esm = EntityTypeMapping.EntitySetMapping;
            if (esm.EntityTypeMappings().Count == 1)
            {
                // if are about to remove the last etm from this ESM, just remove it
                Debug.Assert(
                    esm.EntityTypeMappings()[0] == EntityTypeMapping,
                    "esm.EntityTypeMappings()[0] should be the same as this.EntityTypeMapping");
                DeleteInTransaction(cpc, esm);
            }
            else
            {
                base.InvokeInternal(cpc);
            }
        }
    }
}
