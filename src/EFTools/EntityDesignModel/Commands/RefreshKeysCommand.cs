// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    /// <summary>
    ///     TODO 565946: This implements some temporary functionality to allow us to delete PKs from the entity designer model,
    ///     since IModelElements for PKs currently have no information (not even a name) once they are deleted.
    /// </summary>
    internal class RefreshKeysCommand : Command
    {
        internal BaseEntityModel Model { get; set; }
        internal Dictionary<SchemaQualifiedName, IList<string>> EntityTypePks { get; set; }

        internal RefreshKeysCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        internal RefreshKeysCommand(Dictionary<string, IList<string>> entityPks, Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
            EntityTypePks = entityPks.ToDictionary(kvp => new SchemaQualifiedName(kvp.Key), kvp => kvp.Value);
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(Model != null, "Model was null, could not refresh PKs");
            if (Model != null)
            {
                foreach (var entityType in Model.EntityTypes())
                {
                    var storageEntityType = entityType as StorageEntityType;
                    var conceptualEntityType = entityType as ConceptualEntityType;

                    SchemaQualifiedName entityNameToLookFor = null;
                    if (storageEntityType != null)
                    {
                        var entitySet = storageEntityType.EntitySet as StorageEntitySet;
                        entityNameToLookFor = new SchemaQualifiedName(entitySet.Schema.Value, entitySet.Table.Value);
                    }
                    else if (conceptualEntityType != null)
                    {
                        // If this EntityType has a base type, it should not have any keys.
                        if (conceptualEntityType.BaseType.Target != null)
                        {
                            continue;
                        }
                        entityNameToLookFor = new SchemaQualifiedName(entityType.Name.Value);
                    }

                    Debug.Assert(
                        entityNameToLookFor != null, "Should have created an entity name to look for based off of the current EntityType");
                    if (entityNameToLookFor != null)
                    {
                        var entityNameToRefresh = EntityTypePks.Keys.FirstOrDefault(name => name.Equals(entityNameToLookFor));
                        if (entityNameToRefresh != null)
                        {
                            // Update primary key properties that have changed
                            foreach (var prop in entityType.Properties())
                            {
                                SetKeyPropertyCommand cmd = null;

                                if (prop.IsKeyProperty
                                    && EntityTypePks[entityNameToRefresh].All(
                                        c => string.Compare(c, prop.Name.Value, StringComparison.Ordinal) != 0))
                                {
                                    // This primary key was removed
                                    cmd = new SetKeyPropertyCommand(prop, false);
                                }
                                else if (prop.IsKeyProperty == false
                                         && EntityTypePks[entityNameToRefresh].Any(
                                             c => string.Compare(c, prop.Name.Value, StringComparison.Ordinal) == 0))
                                {
                                    // This primary key was added
                                    cmd = new SetKeyPropertyCommand(prop, true);
                                }

                                if (cmd != null)
                                {
                                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                                }
                            }
                        }
                        else
                        {
                            // If we couldn't find the entity type in the dictionary, then it has no keys. Remove any existing ones.
                            foreach (var keyProp in entityType.ResolvableKeys)
                            {
                                CommandProcessor.InvokeSingleCommand(cpc, new SetKeyPropertyCommand(keyProp, false));
                            }
                        }
                    }
                }
            }
        }
    }
}
