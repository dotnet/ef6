// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Model.Mapping.ChildCollectionBuilders;

    internal class CreateAssociationSetMappingCommand : Command
    {
        internal static readonly string PrereqId = "CreateAssociationSetMappingCommand";

        internal EntityContainerMapping EntityContainerMapping { get; set; }
        internal AssociationSet AssociationSet { get; set; }
        internal Association Association { get; set; }
        internal StorageEntitySet StorageEntitySet { get; set; }
        private AssociationSetMapping _created;

        internal CreateAssociationSetMappingCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal CreateAssociationSetMappingCommand(Association association, EntityType storageEntityType)
            : base(PrereqId)
        {
            CommandValidation.ValidateAssociation(association);
            CommandValidation.ValidateStorageEntityType(storageEntityType);

            EntityContainerMapping = association.Artifact.MappingModel().FirstEntityContainerMapping;
            AssociationSet = association.AssociationSet;
            Association = association;
            StorageEntitySet = storageEntityType.EntitySet as StorageEntitySet;
        }

        internal CreateAssociationSetMappingCommand(
            EntityContainerMapping entityContainerMapping, AssociationSet associationSet, Association association,
            StorageEntitySet storageEntitySet)
            : base(PrereqId)
        {
            CommandValidation.ValidateEntityContainerMapping(entityContainerMapping);
            CommandValidation.ValidateAssociationSet(associationSet);
            CommandValidation.ValidateAssociation(association);
            CommandValidation.ValidateStorageEntitySet(storageEntitySet);

            EntityContainerMapping = entityContainerMapping;
            AssociationSet = associationSet;
            Association = association;
            StorageEntitySet = storageEntitySet;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // if we don't have an ECM yet, go create one
            if (EntityContainerMapping == null)
            {
                var createECM = new CreateEntityContainerMappingCommand(AssociationSet.Artifact);
                CommandProcessor.InvokeSingleCommand(cpc, createECM);
                EntityContainerMapping = createECM.EntityContainerMapping;
            }

            Debug.Assert(EntityContainerMapping != null, "EntityContainerMapping should not be null");
            if (EntityContainerMapping == null)
            {
                throw new CannotLocateParentItemException();
            }

            // create the ETM
            var asm = new AssociationSetMapping(EntityContainerMapping, null);
            asm.Name.SetRefName(AssociationSet);
            asm.TypeName.SetRefName(Association);
            asm.StoreEntitySet.SetRefName(StorageEntitySet);
            EntityContainerMapping.AddAssociationSetMapping(asm);

            XmlModelHelper.NormalizeAndResolve(asm);

            Debug.Assert(asm.Name.Target != null, "Could not resolve association set reference");
            Debug.Assert(asm.TypeName.Target != null, "Could not resolve association type reference");
            Debug.Assert(asm.StoreEntitySet.Target != null, "Could not resolve table reference");

            var assoc = asm.TypeName.Target;
            Debug.Assert(assoc != null, "Could not resolve association reference");
            if (assoc != null)
            {
                InferReferentialConstraints.AddRule(cpc, assoc);
            }

            _created = asm;
        }

        /// <summary>
        ///     Returns the AssociationSetMapping created by this command
        /// </summary>
        internal AssociationSetMapping AssociationSetMapping
        {
            get { return _created; }
        }

        /// <summary>
        ///     A helper method that creates the AssociationSetMapping and also tries to "Intelli-Match" some mappings, that is, if there are
        ///     columns and properties that match by name, we create mappings for the user.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="association"></param>
        /// <param name="storageEntityType"></param>
        /// <returns></returns>
        internal static AssociationSetMapping CreateAssociationSetMappingAndIntellimatch(
            CommandProcessorContext cpc, Association association, StorageEntityType storageEntityType)
        {
            var associationSet = association.AssociationSet;
            Debug.Assert(associationSet != null, "An association found that doesn't have an association set");

            var cmd = new CreateAssociationSetMappingCommand(association, storageEntityType);
            CommandProcessor.InvokeSingleCommand(cpc, cmd);

            foreach (var setEnd in associationSet.AssociationSetEnds())
            {
                var builder = new AssociationSetEndMappingBuilderForCommand(setEnd, storageEntityType);
                builder.Build(cpc);
            }

            return cmd.AssociationSetMapping;
        }

        /// <summary>
        ///     This private class uses the logic found inside the AssociationSetEndMappingBuilder class to determine
        ///     what the potential end property mappings are.  We don't care if there are any existing (there shouldn't
        ///     be actually since we are creating the asm), but override the BuildNew() method and create a mapping if the
        ///     property name and column name match.
        /// </summary>
        private class AssociationSetEndMappingBuilderForCommand : AssociationSetEndMappingBuilder
        {
            internal AssociationSetEndMappingBuilderForCommand(AssociationSetEnd setEnd, StorageEntityType storeEntityType)
                : base(setEnd, storeEntityType)
            {
            }

            protected override void BuildNew(CommandProcessorContext cpc, string propertyName, string propertyType)
            {
                if (StorageEntityType == null
                    || ConceptualEntityType == null)
                {
                    Debug.Fail("The AssociationSetEndMappingBuilder does not have references to everything it needs");
                    return;
                }

                // try to find the column with this name
                var tableColumn = StorageEntityType.GetFirstNamedChildByLocalName(propertyName, true) as Property;
                if (tableColumn != null)
                {
                    // now see if there is also a property with this name
                    var entityProperty = ConceptualEntityType.GetFirstNamedChildByLocalName(propertyName) as Property;
                    if (entityProperty == null)
                    {
                        // they might be trying to map a key from the base class
                        EntityType topMostBaseType = ConceptualEntityType.ResolvableTopMostBaseType;
                        entityProperty = topMostBaseType.GetFirstNamedChildByLocalName(propertyName) as Property;
                    }

                    // if we have both, create a mapping
                    if (entityProperty != null)
                    {
                        CreateEndScalarPropertyCommand cmd = null;
                        var end = AssociationSetEnd.EndProperty;
                        if (end == null)
                        {
                            // we don't have an end yet, this version will create an end as well as the scalar property
                            cmd = new CreateEndScalarPropertyCommand(
                                AssociationSet.AssociationSetMapping, AssociationSetEnd, entityProperty, tableColumn);
                        }
                        else
                        {
                            cmd = new CreateEndScalarPropertyCommand(end, entityProperty, tableColumn);
                        }

                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
            }
        }
    }
}
