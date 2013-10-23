// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     This command lets you add a new EntitySet to either the conceptual or the storage model.  You
    ///     can either send the EntityType that the set will be for to the constructor or, if both type and
    ///     set are being created together, you can send the CreateEntityTypeCommand object and this command
    ///     will use the results of it.
    /// </summary>
    internal class CreateEntitySetCommand : Command
    {
        internal string Name { get; set; }
        internal EntityType EntityType { get; set; }
        internal ModelSpace ModelSpaceValue { get; set; }
        internal bool UniquifyName { get; set; }
        internal string DefiningQueryContent { get; set; }

        /// <summary>
        ///     The EntitySet created by this command.
        /// </summary>
        internal EntitySet EntitySet { get; set; }

        public CreateEntitySetCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Creates an EntitySet of the passed in EntityType in the conceptual model.
        /// </summary>
        /// <param name="name">The name to use for this set</param>
        /// <param name="entityType">The EntityType that this set will contain</param>
        /// <param name="uniquifyName">Flag whether the name should be checked for uniqueness and then changed as required</param>
        internal CreateEntitySetCommand(string name, EntityType entityType, bool uniquifyName)
            : this(name, entityType, ModelSpace.Conceptual, uniquifyName)
        {
        }

        /// <summary>
        ///     Creates an EntitySet of the passed in EntityType in either the conceptual or the storage model.
        /// </summary>
        /// <param name="name">The name to use for this set</param>
        /// <param name="entityType">The EntityType that this set will contain</param>
        /// <param name="modelSpace">Either Conceptual or Storage</param>
        /// <param name="uniquifyName">Flag whether the name should be checked for uniqueness and then changed as required</param>
        internal CreateEntitySetCommand(string name, EntityType entityType, ModelSpace modelSpace, bool uniquifyName)
        {
            CommandValidation.ValidateEntityType(entityType);
            ValidateString(name);

            Name = name;
            EntityType = entityType;
            ModelSpaceValue = modelSpace;
            UniquifyName = uniquifyName;
        }

        /// <summary>
        ///     Creates an EntitySet of a new type being created in the conceptual model.
        /// </summary>
        /// <param name="name">The name to use for this set</param>
        /// <param name="prereq">The CreateEntityTypeCommand that is creating the new type</param>
        /// <param name="uniquifyName">Flag whether the name should be checked for uniqueness and then changed as required</param>
        internal CreateEntitySetCommand(string name, CreateEntityTypeCommand prereq, bool uniquifyName)
            : this(name, prereq, ModelSpace.Conceptual, uniquifyName)
        {
        }

        /// <summary>
        ///     Creates an EntitySet of a new type being created in either the conceptual or the storage model.
        /// </summary>
        /// <param name="name">The name to use for this set</param>
        /// <param name="prereq">The CreateEntityTypeCommand that is creating the new type</param>
        /// <param name="modelSpace">Either Conceptual or Storage</param>
        /// <param name="uniquifyName">Flag whether the name should be checked for uniqueness and then changed as required</param>
        internal CreateEntitySetCommand(string name, CreateEntityTypeCommand prereq, ModelSpace modelSpace, bool uniquifyName)
        {
            ValidatePrereqCommand(prereq);
            ValidateString(name);

            Name = name;
            ModelSpaceValue = modelSpace;
            UniquifyName = uniquifyName;

            AddPreReqCommand(prereq);
        }

        protected override void ProcessPreReqCommands()
        {
            if (EntityType == null)
            {
                var prereq = GetPreReqCommand(CreateEntityTypeCommand.PrereqId) as CreateEntityTypeCommand;
                if (prereq != null)
                {
                    EntityType = prereq.EntityType;

                    var proposedEntityName = prereq.ReadProperty<string>(CreateEntityTypeCommand.ProposedNameProperty);

                    // see if the entity type name was 'uniquified' during its creation, that is, its different
                    // now then the proposed name
                    if (string.Compare(proposedEntityName, EntityType.LocalName.Value, StringComparison.CurrentCulture) != 0)
                    {
                        // if the EntityType name has changed, also change the proposed name for the EntitySet
                        Name = ModelHelper.ConstructProposedEntitySetName(EntityType.Artifact, EntityType.LocalName.Value);
                    }
                }

                Debug.Assert(EntityType != null, "We didn't get a good entity type out of the Command");
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EntityType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(EntityType != null, "InvokeInternal is called when EntityType is null");
            if (EntityType == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when EntityType is null");
            }

            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            // locate the entity container we want to add it to
            BaseEntityContainer entityContainer = null;
            EntitySet entitySet = null;

            switch (ModelSpaceValue)
            {
                case ModelSpace.Conceptual:
                    Debug.Assert(artifact.ConceptualModel() != null, "artifact.ConceptualModel() should not be null");
                    entityContainer = artifact.ConceptualModel().FirstEntityContainer;
                    break;
                case ModelSpace.Storage:
                    Debug.Assert(artifact.StorageModel() != null, "artifact.StorageModel() should not be null");
                    entityContainer = artifact.StorageModel().FirstEntityContainer;
                    break;
                default:
                    Debug.Fail("Unknown model space");
                    break;
            }
            Debug.Assert(entityContainer != null, "entityContainer should not be null");
            if (entityContainer == null)
            {
                throw new CannotLocateParentItemException();
            }

            // check for uniqueness of the name within the chosen EC
            if (UniquifyName)
            {
                Name = ModelHelper.GetUniqueName(typeof(EntitySet), entityContainer, Name);
            }
            else
            {
                string msg = null;
                if (ModelHelper.IsUniqueName(typeof(EntitySet), entityContainer, Name, false, out msg) == false)
                {
                    throw new CommandValidationFailedException(msg);
                }
            }

            // create the entity set; don't need to assert on enum since that has happened above
            if (ModelSpaceValue == ModelSpace.Conceptual)
            {
                entitySet = new ConceptualEntitySet(entityContainer, null);
            }
            else
            {
                entitySet = new StorageEntitySet(entityContainer, null);

                // DefiningQuery creation
                if (DefiningQueryContent != null)
                {
                    var definingQuery = new DefiningQuery(entitySet, null);
                    definingQuery.XElement.SetValue(DefiningQueryContent);
                    ((StorageEntitySet)entitySet).DefiningQuery = definingQuery;
                }
            }
            Debug.Assert(entitySet != null, "entitySet should not be null");
            if (entitySet == null)
            {
                throw new ItemCreationFailureException();
            }

            // set name and add to the parent
            entitySet.LocalName.Value = Name;
            entityContainer.AddEntitySet(entitySet);

            // set the entity type binding
            if (EntityType != null)
            {
                entitySet.EntityType.SetRefName(EntityType);
            }

            XmlModelHelper.NormalizeAndResolve(entitySet);

            EntitySet = entitySet;
        }
    }
}
