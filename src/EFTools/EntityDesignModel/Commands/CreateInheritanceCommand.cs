// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     This command sets up an inheritance relationship between two entities.  This is done by adding a BaseType
    ///     attribute to the derived entity's definition.
    ///     &lt;EntityType Name=&quot;User&quot; BaseType=&quot;TPH.Person&quot; &gt;
    ///     &lt;Property Name=&quot;Login&quot; Type=&quot;String&quot; Nullable=&quot;true&quot; MaxLength=&quot;20&quot; Unicode=&quot;false&quot; FixedLength=&quot;false&quot; /&gt;
    ///     &lt;Property Name=&quot;Password&quot; Type=&quot;String&quot; Nullable=&quot;true&quot; MaxLength=&quot;20&quot; Unicode=&quot;false&quot; FixedLength=&quot;false&quot; /&gt;
    ///     &lt;/EntityType&gt;
    ///     Other work is needed when inheritance is created.
    ///     1. Only the base-most class in an inheritance hierarchy can have keys, so any keys must be removed from the derived class
    ///     2. The entity set for the newly derived class is removed since it will now use the base class's entity set
    ///     3. Due to this change in entity set, we must also change any references to this set in an AssociationSet ends
    ///     4. We register to run the EnforceEntitySetMappingRules check so rewrite any MSL as neededo
    /// </summary>
    internal class CreateInheritanceCommand : Command
    {
        internal ConceptualEntityType EntityToBeDerived { get; set; }
        internal IEnumerable<Property> EntityToBeDerivedKeyProperties { get; private set; }
        internal ConceptualEntityType BaseType { get; set; }

        internal CreateInheritanceCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Creates an inheritance so that 'baseType' becomes the base type of 'entityToBeDerived'
        /// </summary>
        /// <param name="entityToBeDerived">Must be non-null and a c-space entity, it cannot already be a derived type</param>
        /// <param name="baseType">Must be non-null and a c-space entity</param>
        internal CreateInheritanceCommand(ConceptualEntityType entityToBeDerived, ConceptualEntityType baseType)
        {
            CommandValidation.ValidateConceptualEntityType(entityToBeDerived);
            CommandValidation.ValidateConceptualEntityType(baseType);

            Debug.Assert(entityToBeDerived.EntityModel == baseType.EntityModel, "Inheritance only works in the same model");

            EntityToBeDerived = entityToBeDerived;
            EntityToBeDerivedKeyProperties = entityToBeDerived.ResolvableKeys;
            BaseType = baseType;
        }

        /// <summary>
        ///     Creates an inheritance so that 'baseType' becomes the base type of the new entity being
        ///     created by the CreateEntityTypeCommand.
        /// </summary>
        /// <param name="prereqCommand">Must be non-null</param>
        /// <param name="baseType">Must be non-null and a c-space entity</param>
        internal CreateInheritanceCommand(CreateEntityTypeCommand prereqCommand, ConceptualEntityType baseType)
        {
            ValidatePrereqCommand(prereqCommand);
            CommandValidation.ValidateConceptualEntityType(baseType);

            BaseType = baseType;

            AddPreReqCommand(prereqCommand);
        }

        protected override void ProcessPreReqCommands()
        {
            if (EntityToBeDerived == null)
            {
                var prereq = GetPreReqCommand(CreateEntityTypeCommand.PrereqId) as CreateEntityTypeCommand;
                if (prereq != null)
                {
                    Debug.Assert(prereq.EntityType is ConceptualEntityType, "EntityType is not ConceptualEntityType");

                    EntityToBeDerived = prereq.EntityType as ConceptualEntityType;

                    // now that we have our entity, run our validation on it
                    CommandValidation.ValidateConceptualEntityType(EntityToBeDerived);
                    Debug.Assert(EntityToBeDerived.EntityModel == BaseType.EntityModel, "Inheritance only works in the same model");
                }

                Debug.Assert(EntityToBeDerived != null, "We didn't get a good entity type out of the Command");
            }
        }

        /// <summary>
        ///     We override this to register an integrity check, and to deal with association set ends that reference
        ///     the existing entity set (which is going to be deleted)
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            base.PreInvoke(cpc);

            // you cannot use this command with a type that already has a base class; throwing here
            // because its not a good idea to throw an exception in a c'tor
            if (EntityToBeDerived == null
                || EntityToBeDerived.BaseType.Target != null)
            {
                throw new InvalidOperationException();
            }

            EnforceEntitySetMappingRules.AddRule(cpc, BaseType.EntitySet);

            // see if this type is used by association ends; since we are creating an inheritance
            // we need to change any AssociationSetEnd references from the current EntitySet to the
            // new baseType's EntitySet (the derived type's EntitySet will be deleted)
            var associationsToUpdate = new HashSet<Association>();
            if (EntityToBeDerived.EntitySet != null)
            {
                var associationSetEndsToUpdate = EntityToBeDerived.EntitySet.GetAntiDependenciesOfType<AssociationSetEnd>();
                foreach (var setEnd in associationSetEndsToUpdate)
                {
                    setEnd.EntitySet.SetRefName(BaseType.EntitySet);
                    XmlModelHelper.NormalizeAndResolve(setEnd);

                    var aSet = setEnd.Parent as AssociationSet;
                    if (aSet != null)
                    {
                        if (aSet.Association.Target != null)
                        {
                            associationsToUpdate.Add(aSet.Association.Target);
                        }
                    }
                }
            }

            foreach (var association in associationsToUpdate)
            {
                // try to recreate the AssociationSetMapping if one exists
                if (association != null
                    && association.AssociationSet != null
                    && association.AssociationSet.AssociationSetMapping != null
                    && association.AssociationSet.AssociationSetMapping.XObject != null)
                {
                    // store off the entity set for later
                    var storeEntitySet = association.AssociationSet.AssociationSetMapping.StoreEntitySet.Target;

                    // delete it
                    DeleteEFElementCommand.DeleteInTransaction(cpc, association.AssociationSet.AssociationSetMapping);

                    // create a new one (if we can)
                    if (storeEntitySet != null
                        && storeEntitySet.EntityType.Target != null)
                    {
                        var set = storeEntitySet.EntityType.Target as StorageEntityType;
                        Debug.Assert(set != null, "EntityType is not StorageEntityType");
                        CreateAssociationSetMappingCommand.CreateAssociationSetMappingAndIntellimatch(cpc, association, set);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "BaseType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EntityToBeDerived")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(
                EntityToBeDerived != null && BaseType != null, "InvokeInternal is called when EntityToBeDerived or BaseType is null");

            if (EntityToBeDerived == null
                || BaseType == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when EntityToBeDerived or BaseType is null");
            }

            if (EntityToBeDerived.EntitySet != null)
            {
                // since we are creating an inheritance, we need to delete EntitySet(s) for entityToBeDerived, 
                // before we do this, move any EntityTypeMappings to the base type's EntitySetMapping
                var entitySetToDelete = EntityToBeDerived.EntitySet as ConceptualEntitySet;
                var entitySetMappingToDelete = entitySetToDelete.EntitySetMapping;

                // if there isn't an ESM, there won't be anything to move
                if (entitySetMappingToDelete != null)
                {
                    var entitySetOfBaseType = BaseType.EntitySet as ConceptualEntitySet;
                    if (entitySetOfBaseType != null)
                    {
                        // get the base type's ESM (if it doesn't exist, create one)
                        var entitySetMappingOfBaseType = entitySetOfBaseType.EntitySetMapping;
                        if (entitySetMappingOfBaseType == null)
                        {
                            var entityContainer = EntityToBeDerived.EntityModel.EntityContainer;
                            Debug.Assert(entityContainer != null, "EntityToBeDerived should have an Entity Container");

                            var createESM = new CreateEntitySetMappingCommand(entityContainer.EntityContainerMapping, entitySetOfBaseType);
                            CommandProcessor.InvokeSingleCommand(cpc, createESM);
                            entitySetMappingOfBaseType = createESM.EntitySetMapping;
                        }

                        // move all of the ETMs
                        var etms = new List<EntityTypeMapping>();
                        etms.AddRange(entitySetMappingToDelete.EntityTypeMappings());

                        foreach (var etm in etms)
                        {
                            // here, to work around an xml editor bug, we clone the entity type mapping, instead of re-parenting it
                            etm.Clone(entitySetMappingOfBaseType);

                            // The old EntityTypeMapping will be deleted when we delete the entity set below.  
                        }
                    }
                }

                // now we can delete the entity set, which will delete the ESM too
                DeleteEFElementCommand.DeleteInTransaction(cpc, entitySetToDelete);
            }

            // remove all properties from derived entity's key (it will inherit the base type's keys now)
            if (EntityToBeDerived.Key != null)
            {
                var propertyRefs = new List<PropertyRef>(EntityToBeDerived.Key.PropertyRefs);
                foreach (var propertyRef in propertyRefs)
                {
                    var property = propertyRef.Name.Target;
                    if (property != null)
                    {
                        var setKey = new SetKeyPropertyCommand(property, false, false, true);
                        CommandProcessor.InvokeSingleCommand(cpc, setKey);
                    }
                }
            }

            // set the base type
            EntityToBeDerived.BaseType.SetRefName(BaseType);

            //
            // if there is a referential constraint, then update any principals in the ref constraint to 
            // point to properties in the new entity type.
            //
            foreach (var end in EntityToBeDerived.GetAntiDependenciesOfType<AssociationEnd>())
            {
                foreach (var role in end.GetAntiDependenciesOfType<ReferentialConstraintRole>())
                {
                    var rc = role.Parent as ReferentialConstraint;
                    if (rc != null
                        && rc.Principal == role)
                    {
                        //
                        // this is the principal, so we want to update any keys in RC to reflect new keys 
                        // in the new base type.  If the number of keys don't match, we'll delete any leftovers
                        //
                        var keys = BaseType.ResolvableTopMostBaseType.ResolvableKeys.GetEnumerator();
                        foreach (var pr in rc.Principal.PropertyRefs)
                        {
                            if (keys.MoveNext())
                            {
                                // update this property ref to reflect the new key in the derived type
                                pr.Name.SetRefName(keys.Current);
                                ItemBinding[] bindings = { pr.Name };
                                CheckArtifactBindings.ScheduleBindingsForRebind(cpc, bindings);
                            }
                            else
                            {
                                // no more keys in the new base type, so delete this property ref & it's peer
                                // in the dependent section  
                                Command cmd = new DeleteReferentialConstraintPropertyRefCommand(pr);
                                // don't invoke this command now, as it will modify the collection we're iterating over
                                CommandProcessor.EnqueueCommand(cmd);
                            }
                        }
                    }
                }
            }

            // rebind and verify
            EntityToBeDerived.BaseType.Rebind();
            Debug.Assert(
                EntityToBeDerived.BaseType.Status == BindingStatus.Known,
                "EntityToBeDerived.BaseType.Status should be BindingStatus.Known, instead it is "
                + EntityToBeDerived.BaseType.Status.ToString());
        }
    }
}
