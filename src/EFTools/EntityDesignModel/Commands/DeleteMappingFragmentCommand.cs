// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class DeleteMappingFragmentCommand : DeleteEFElementCommand
    {
        internal string ConceptualEntityTypeName { get; private set; }
        internal string StorageEntitySetName { get; private set; }

        /// <summary>
        ///     Deletes the passed in MappingFragment
        /// </summary>
        /// <param name="fragment"></param>
        internal DeleteMappingFragmentCommand(MappingFragment fragment)
            : base(fragment)
        {
            CommandValidation.ValidateMappingFragment(fragment);
        }

        protected MappingFragment MappingFragment
        {
            get
            {
                var elem = EFElement as MappingFragment;
                Debug.Assert(elem != null, "underlying element does not exist or is not a MappingFragment");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            if (MappingFragment.EntityTypeMapping != null
                && MappingFragment.EntityTypeMapping.EntitySetMapping != null)
            {
                ConceptualEntityTypeName = MappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType != null
                                               ? MappingFragment.EntityTypeMapping.FirstBoundConceptualEntityType.Name.Value
                                               : null;
                StorageEntitySetName = MappingFragment.StoreEntitySet.Target != null
                                           ? MappingFragment.StoreEntitySet.Target.Name.Value
                                           : null;

                EnforceEntitySetMappingRules.AddRule(cpc, MappingFragment.EntityTypeMapping.EntitySetMapping);
            }

            base.PreInvoke(cpc);
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var etm = MappingFragment.EntityTypeMapping;
            if (etm.MappingFragments().Count == 1)
            {
                // if we are about to remove the last fragment from this ETM, just remove it
                Debug.Assert(
                    etm.MappingFragments()[0] == MappingFragment, "end.MappingFragments()[0] should be the same as this.MappingFragment");
                DeleteInTransaction(cpc, etm);
            }
            else
            {
                base.InvokeInternal(cpc);
            }
        }
    }
}
