// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     TODO 565946: This implements some temporary functionality to allow us to delete FKs between PKs from the entity designer model,
    ///     since IModelElements for FKs between PKs currently have no information (not even a name) once they are deleted.
    /// </summary>
    internal class DeleteStaleFKsCommand : Command
    {
        internal ConceptualEntityModel ConceptualModel { get; set; }
        internal StorageEntityModel StorageModel { get; set; }
        internal HashSet<string> ConceptualFkNames { get; private set; }
        internal HashSet<string> StorageFkNames { get; private set; }

        internal DeleteStaleFKsCommand(
            HashSet<string> conceptualFkNames, HashSet<string> storageFkNames, Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
            ConceptualFkNames = conceptualFkNames;
            StorageFkNames = storageFkNames;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            DeleteStaleAssociations(cpc, StorageModel, StorageFkNames);
            DeleteStaleAssociations(cpc, ConceptualModel, ConceptualFkNames);
        }

        private static void DeleteStaleAssociations(CommandProcessorContext cpc, BaseEntityModel model, HashSet<string> associationNames)
        {
            Debug.Assert(model != null, "Model was null, could not delete stale Fks between Pks");
            if (model != null)
            {
                var associationsToDelete = new List<Association>();

                foreach (var association in model.Associations())
                {
                    // If this association is longer present in the latest SqlSchema model, delete it
                    if (associationNames.Contains(association.Name.Value) == false)
                    {
                        associationsToDelete.Add(association);
                    }
                }

                foreach (var association in associationsToDelete)
                {
                    var deleteAssociationSetCommand = new DeleteEFElementCommand(
                        (c, subCpc) =>
                            {
                                var cmd = c as DeleteEFElementCommand;
                                cmd.EFElement = ModelHelper.FindAssociationSet(model, association.Name.Value);
                                return cmd.EFElement != null;
                            });

                    CommandProcessor.InvokeSingleCommand(cpc, deleteAssociationSetCommand);

                    var deleteAssociationCmd = new DeleteAssociationCommand(
                        (c, subCpc) =>
                            {
                                var cmd = c as DeleteAssociationCommand;
                                // Checking the model again for the association in case it's deleted already.
                                cmd.EFElement = ModelHelper.FindAssociation(model, association.Name.Value);
                                return cmd.EFElement != null;
                            });

                    CommandProcessor.InvokeSingleCommand(cpc, deleteAssociationCmd);
                }
            }
        }
    }
}
