// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     This command will create a new EntityType in either the conceptual or the storage model.  This
    ///     command also stores the initial name passed into it and can be retrieved using the
    ///     ProposedNameProperty property.  This can help dependent commands know if the name of the entity
    ///     has changed during creation from the one sent in.
    /// </summary>
    internal class CreateEntityTypeCommand : Command
    {
        internal static readonly string PrereqId = "CreateEntityTypeCommand";
        internal static readonly string ProposedNameProperty = "ProposedName";

        internal string Name { get; set; }
        internal ModelSpace ModelSpaceValue { get; set; }
        internal bool UniquifyName { get; set; }
        internal EntityType CreatedEntityType { get; set; }
        internal bool CreateWithDefaultName { get; set; }

        internal CreateEntityTypeCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Creates a new entity type in the conceptual model.
        /// </summary>
        /// <param name="name">The name to use for this type</param>
        /// <param name="uniquifyName">Flag whether the name should be checked for uniqueness and then changed as required</param>
        internal CreateEntityTypeCommand(string name, bool uniquifyName)
            : this(name, ModelSpace.Conceptual, uniquifyName)
        {
        }

        /// <summary>
        ///     Creates a new entity type in either the conceptual model or the storage model.
        /// </summary>
        /// <param name="name">The name to use for this type</param>
        /// <param name="modelSpace">Either Conceptual or Storage</param>
        /// <param name="uniquifyName">Flag whether the name should be checked for uniqueness and then changed as required</param>
        internal CreateEntityTypeCommand(string name, ModelSpace modelSpace, bool uniquifyName)
            : base(PrereqId)
        {
            ValidateString(name);

            Name = name;
            ModelSpaceValue = modelSpace;
            UniquifyName = uniquifyName;

            WriteProperty(ProposedNameProperty, name);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // the model that we want to add the entity to
            var model = ModelHelper.GetEntityModel(artifact, ModelSpaceValue);
            if (model == null)
            {
                throw new CannotLocateParentItemException();
            }

            // check for uniqueness
            if (UniquifyName)
            {
                Name = ModelHelper.GetUniqueName(typeof(EntityType), model, Name);
            }
            else
            {
                string msg = null;
                if (ModelHelper.IsUniqueName(typeof(EntityType), model, Name, false, out msg) == false)
                {
                    throw new CommandValidationFailedException(msg);
                }
            }

            // create the new item in our model
            EntityType entity = null;
            if (model.IsCSDL)
            {
                entity = new ConceptualEntityType(model as ConceptualEntityModel, null);
            }
            else
            {
                entity = new StorageEntityType(model as StorageEntityModel, null);
            }
            Debug.Assert(entity != null, "entity should not be null");
            if (entity == null)
            {
                throw new ItemCreationFailureException();
            }

            // set the name, add it to the parent item
            entity.LocalName.Value = Name;
            model.AddEntityType(entity);

            XmlModelHelper.NormalizeAndResolve(entity);

            CreatedEntityType = entity;
        }

        /// <summary>
        ///     The EntityType that this command created
        /// </summary>
        internal EntityType EntityType
        {
            get { return CreatedEntityType; }
        }

        /// <summary>
        ///     This helper function will create an entity type and set, along with one key property
        ///     in the entity type, using default names.  For instance, the second time this is called you'll
        ///     get an entity called "EntityType2", etc.
        ///     This only creates these in the conceptual model.
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes committed.
        /// </summary>
        /// <param name="cpc"></param>
        /// <returns>The new EntityType</returns>
        internal static EntityType CreateEntityTypeAndEntitySetWithDefaultNames(CommandProcessorContext cpc)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // the model that we want to add the entity to
            var model = artifact.ConceptualModel();
            if (model == null)
            {
                throw new CannotLocateParentItemException();
            }

            // derive some default names
            var entityName = ModelHelper.GetUniqueNameWithNumber(typeof(EntityType), model, Resources.Model_DefaultEntityTypeName);
            var entitySetName = ModelHelper.GetUniqueName(
                typeof(EntitySet), model.FirstEntityContainer, ModelHelper.ConstructProposedEntitySetName(artifact, entityName));

            // go create it
            EntityType entityType = CreateConceptualEntityTypeAndEntitySetAndProperty(
                cpc, entityName, entitySetName, true,
                Resources.Model_IdPropertyName, ModelConstants.Int32PropertyType, ModelConstants.StoreGeneratedPattern_Identity, false, true);

            return entityType;
        }

        /// <summary>
        ///     This helper function creates a new entity in the conceptual model that is derived from the
        ///     passed in entity.
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes committed.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="name">The name of the new, derived entity</param>
        /// <param name="baseType">The entity that this new type should derive from</param>
        /// <param name="uniquifyName">Flag whether the name should be checked for uniqueness and then changed as required</param>
        /// <returns>The new EntityType</returns>
        internal static ConceptualEntityType CreateDerivedEntityType(
            CommandProcessorContext cpc, string name, ConceptualEntityType baseType, bool uniquifyName)
        {
            var cet = new CreateEntityTypeCommand(name, uniquifyName);
            var inh = new CreateInheritanceCommand(cet, baseType);

            var cp = new CommandProcessor(cpc, cet, inh);
            cp.Invoke();

            var derivedType = cet.EntityType as ConceptualEntityType;
            Debug.Assert(derivedType != null, "EntityType is not ConceptualEntityType");
            return derivedType;
        }

        /// <summary>
        ///     This helper function is an easy way to get a new entity type, entity set and key property in the
        ///     new entity type created in the conceptual model.
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes committed.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="name">The name of the new entity</param>
        /// <param name="setName">The name of the new set</param>
        /// <param name="createKeyProperty">A flag whether to create a new key property or not (sending false creates no new property)</param>
        /// <param name="propertyName">The name of the new property</param>
        /// <param name="propertyType">The type of the new property</param>
        /// <param name="uniquifyName">Flag whether the name should be checked for uniqueness and then changed as required</param>
        /// <param name="isDefaultName">Flag whether the name is the default for new entity types/sets</param>
        /// <returns>The new EntityType</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static ConceptualEntityType CreateConceptualEntityTypeAndEntitySetAndProperty(
            CommandProcessorContext cpc,
            string name, string setName, bool createKeyProperty, string propertyName,
            string propertyType, string propertyStoreGeneratedPattern, bool uniquifyNames, bool isDefaultName = false)
        {
            var cet = CreateEntityTypeAndEntitySetAndProperty(
                cpc, name, setName, createKeyProperty,
                propertyName, propertyType, propertyStoreGeneratedPattern, ModelSpace.Conceptual, uniquifyNames, isDefaultName) as
                      ConceptualEntityType;
            Debug.Assert(cet != null, "EntityType is not ConceptualEntityType");
            return cet;
        }

        /// <summary>
        ///     This helper function is an easy way to get a new entity type, entity set and key property in the
        ///     new entity type created in the conceptual or storage model.
        ///     NOTE: If the cpc already has an active transaction, these changes will be in that transaction
        ///     and the caller of this helper method must commit it to see these changes committed.
        /// </summary>
        /// <param name="cpc"></param>
        /// <param name="name">The name of the new entity</param>
        /// <param name="setName">The name of the new set</param>
        /// <param name="createKeyProperty">A flag whether to create a new key property or not (sending false creates no new property)</param>
        /// <param name="propertyName">The name of the new property</param>
        /// <param name="propertyType">The type of the new property</param>
        /// <param name="modelSpace">Either Conceptual or Storage</param>
        /// <param name="uniquifyName">Flag whether the name should be checked for uniqueness and then changed as required</param>
        /// <param name="isDefaultName">Flag whether the name is the default for new entity types/sets</param>
        /// <returns>The new EntityType</returns>
        internal static EntityType CreateEntityTypeAndEntitySetAndProperty(
            CommandProcessorContext cpc,
            string name, string setName, bool createKeyProperty, string propertyName,
            string propertyType, string propertyStoreGeneratedPattern, ModelSpace modelSpace, bool uniquifyNames, bool isDefaultName = false)
        {
            var cp = new CommandProcessor(cpc);

            var cet = new CreateEntityTypeCommand(name, modelSpace, uniquifyNames);
            cet.CreateWithDefaultName = isDefaultName;
            cp.EnqueueCommand(cet);

            var ces = new CreateEntitySetCommand(setName, cet, modelSpace, uniquifyNames);
            cp.EnqueueCommand(ces);

            if (createKeyProperty)
            {
                var cpcd = new CreatePropertyCommand(propertyName, cet, propertyType, false);
                cpcd.IsIdProperty = true;
                cp.EnqueueCommand(cpcd);

                var skpc = new SetKeyPropertyCommand(cpcd, true);
                cp.EnqueueCommand(skpc);

                var ssgpc = new SetStoreGeneratedPatternCommand(cpcd, propertyStoreGeneratedPattern);
                cp.EnqueueCommand(ssgpc);
            }

            cp.Invoke();

            return cet.EntityType;
        }

        internal void SetCreatedEntityType(EntityType createdEntityType)
        {
            CreatedEntityType = createdEntityType;
        }
    }
}
