// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Integrity;

    internal class CreateOrUpdateAssociationSetMappingCommand : CreateAssociationSetMappingCommand
    {
        internal CreateOrUpdateAssociationSetMappingCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var associationSetMapping = ModelHelper.FindAssociationSetMappingForConceptualAssociation(Association);

            if (associationSetMapping == null)
            {
                // This AssociationSetMapping does not exist, create it
                base.InvokeInternal(cpc);
                associationSetMapping = AssociationSetMapping;
                Debug.Assert(associationSetMapping != null, "Could not create AssociationSetMapping");
            }
            else
            {
                // The AssociationSetMapping already exists, update it
                associationSetMapping.Name.SetRefName(AssociationSet);
                associationSetMapping.TypeName.SetRefName(Association);
                associationSetMapping.StoreEntitySet.SetRefName(StorageEntitySet);

                XmlModelHelper.NormalizeAndResolve(associationSetMapping);

                Debug.Assert(associationSetMapping.Name.Target != null, "Could not resolve association set reference");
                Debug.Assert(associationSetMapping.TypeName.Target != null, "Could not resolve association type reference");
                Debug.Assert(associationSetMapping.StoreEntitySet.Target != null, "Could not resolve table reference");

                InferReferentialConstraints.AddRule(cpc, Association);
            }
        }
    }
}
