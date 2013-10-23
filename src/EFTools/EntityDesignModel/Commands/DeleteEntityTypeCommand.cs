// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal class DeleteEntityTypeCommand : DeleteEFElementCommand
    {
        internal string DeletedEntityTypeName { get; private set; }
        internal IEnumerable<EntityQualifiedPropertyName> DeletedMappedStoragePropertyNames { get; private set; }

        public DeleteEntityTypeCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Deletes the passed in EntityType
        /// </summary>
        /// <param name="entityType"></param>
        internal DeleteEntityTypeCommand(EntityType entityType)
            : base(entityType)
        {
            CommandValidation.ValidateEntityType(entityType);

            SaveDeletedInformation();
        }

        private void SaveDeletedInformation()
        {
            DeletedEntityTypeName = EntityType.Name.Value;

            // Collect all of the mapped storage property names in case we need to 
            var deletedStoragePropertyNames = new HashSet<EntityQualifiedPropertyName>();
            foreach (var cProp in EntityType.Properties())
            {
                var scalarProperties = cProp.GetAntiDependenciesOfType<ScalarProperty>();
                foreach (var scalarProperty in scalarProperties)
                {
                    var storageProperty = (StorageProperty)scalarProperty.ColumnName.Target;
                    if (storageProperty != null
                        && storageProperty.EntityType != null)
                    {
                        var entityToPropertyName = new EntityQualifiedPropertyName(
                            storageProperty.EntityType.Name.Value, storageProperty.Name.Value, false);
                        if (!deletedStoragePropertyNames.Contains(entityToPropertyName))
                        {
                            deletedStoragePropertyNames.Add(entityToPropertyName);
                        }
                    }
                }
            }
            DeletedMappedStoragePropertyNames = deletedStoragePropertyNames;
        }

        protected internal EntityType EntityType
        {
            get
            {
                var elem = EFElement as EntityType;
                Debug.Assert(elem != null, "underlying element does not exist or is not an EntityType");
                if (elem == null)
                {
                    throw new InvalidModelItemException();
                }
                return elem;
            }
        }

        /// <summary>
        ///     We override this method because we need to do some extra things before
        ///     the normal PreInvoke gets called and our antiDeps are removed
        /// </summary>
        /// <param name="cpc"></param>
        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            // save off the deleted entity type name
            SaveDeletedInformation();

            // enforce our mapping rules for C-side entities
            if (EntityType.EntityModel.IsCSDL)
            {
                EnforceEntitySetMappingRules.AddRule(cpc, EntityType);
                // remove base type for all derived EntityTypes
                var cet = EntityType as ConceptualEntityType;
                Debug.Assert(cet != null, "EntityType is not a ConceptualEntityType");

                foreach (var derivedType in cet.ResolvableDirectDerivedTypes)
                {
                    var cmd = new DeleteInheritanceCommand(derivedType);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
            base.PreInvoke(cpc);
        }

        /// <summary>
        ///     We override this method to do some specialized processing of AssociationEnd antiDeps
        /// </summary>
        /// <param name="cpc"></param>
        protected override void RemoveAntiDeps(CommandProcessorContext cpc)
        {
            // if this entity is used in an AssociationEnd, remove the entire Association
            // Note: have to convert to array first to prevent exceptions due to
            // editing the collection while iterating over it
            var associations = GetListOfAssociationsToBeDeleted(EntityType).ToArray();
            foreach (var association in associations)
            {
                var deleteAssociationCommand = association.GetDeleteCommand();
                DeleteInTransaction(cpc, deleteAssociationCommand);
            }

            if (EntityType.EntityModel.IsCSDL)
            {
                var cModel = EntityType.RuntimeModelRoot() as ConceptualEntityModel;

                if (cModel != null)
                {
                    // If there is a FunctionImport which returns the entity type, set the FunctionImport return type to null.
                    foreach (var fi in EntityType.GetAntiDependenciesOfType<FunctionImport>())
                    {
                        CommandProcessor.InvokeSingleCommand(
                            cpc, new ChangeFunctionImportCommand(
                                cModel.FirstEntityContainer as ConceptualEntityContainer,
                                fi, fi.Function, fi.DisplayName, fi.IsComposable.Value, true,
                                Resources.NoneDisplayValueUsedForUX));
                    }
                }
            }

            // process the remaining antiDeps normally
            base.RemoveAntiDeps(cpc);
        }

        /// <summary>
        ///     Constructs a list of the Associations to be deleted given a to-be-deleted EntityType
        /// </summary>
        /// <param name="entityType">the EntityType</param>
        /// <returns>list of the Associations to be deleted</returns>
        internal static List<Association> GetListOfAssociationsToBeDeleted(EntityType entityType)
        {
            var assocsToDelete = new List<Association>();
            if (entityType == null)
            {
                Debug.Fail("argument entityType must not be null");
                return assocsToDelete;
            }

            // if this entity is used in an AssociationEnd, the entire Association should be deleted
            foreach (var associationEnd in entityType.GetAntiDependenciesOfType<AssociationEnd>())
            {
                var association = associationEnd.Parent as Association;
                Debug.Assert(association != null, " null association for AssociationEnd" + associationEnd.ToPrettyString());
                if (association != null
                    && association.XObject != null)
                {
                    assocsToDelete.Add(association);
                }
            }

            return assocsToDelete;
        }
    }
}
