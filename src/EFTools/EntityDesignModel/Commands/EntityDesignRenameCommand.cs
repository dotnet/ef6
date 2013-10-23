// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class EntityDesignRenameCommand : RenameCommand
    {
        internal EntityDesignRenameCommand(EFNormalizableItem element, string newName, bool uniquenessIsCaseSensitive)
            : base(element, newName, uniquenessIsCaseSensitive)
        {
        }

        public EntityDesignRenameCommand(Func<Command, CommandProcessorContext, bool> bindingAction, bool uniquenessIsCaseSensitive)
            : base(bindingAction, uniquenessIsCaseSensitive)
        {
        }

        protected override bool IsUniqueNameForExistingItem(out string errorMessage)
        {
            return ModelHelper.IsUniqueNameForExistingItem(Element, NewName, UniquenessIsCaseSensitive, out errorMessage);
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // Per spec: diagram name can contain any characters but cannot consist solely of spaces and cannot be an empty string.
            // Ideally, we want put this restriction in the XSD but we have defined the diagram name XSD type in V1 and V2 XSDs as 'string' (and the files have shipped).
            // Adding the restriction in the XSD could potentially cause safe-mode when opening an existing edmx file with empty diagram name.
            if (Element is Diagram
                && String.IsNullOrWhiteSpace(NewName))
            {
                // not valid content
                var msg = string.Format(CultureInfo.CurrentCulture, Resources.BAD_DIAGRAM_NAME);
                throw new CommandValidationFailedException(msg);
            }
            else
            {
                base.InvokeInternal(cpc);
            }
        }

        protected override RenameCommand CloneCommand(EFNormalizableItem itemToRename)
        {
            return new EntityDesignRenameCommand(itemToRename, NewName, UniquenessIsCaseSensitive);
        }

        protected override void RenameRelatedElements(CommandProcessorContext cpc)
        {
            // if we are renaming EntityType, we should also rename it's EntitySet (if applicable)
            if (Element is EntityType)
            {
                RenameEntitySet(cpc);
            }
        }

        private void RenameEntitySet(CommandProcessorContext cpc)
        {
            var entity = Element as EntityType;
            Debug.Assert(entity != null, "Element for rename was not EntityType");

            // rename EntitySet only if the entity has no base type
            if (entity != null)
            {
                var cet = entity as ConceptualEntityType;
                if (cet != null
                    && cet.HasResolvableBaseType)
                {
                    return;
                }
            }

            if (entity.EntitySet != null)
            {
                var entitySetName = entity.EntitySet.LocalName.Value;

                // check if EntitySet name is of auto-generated form (which differs depending on 
                // the setting of the pluralization flag)                    
                var autoEntitySetName = ModelHelper.ConstructProposedEntitySetName(entity.Artifact, entity.LocalName.Value);
                if (entitySetName.StartsWith(autoEntitySetName, StringComparison.CurrentCulture))
                {
                    // the actual EntitySet name may differ from the auto-generated name by an integer suffix
                    // if there were clashes with existing EntitySet names - so check this here
                    var suffix = entitySetName.Substring(autoEntitySetName.Length);
                    int i;
                    if (suffix.Length == 0
                        || int.TryParse(suffix, out i))
                    {
                        var proposedEntitySetName = ModelHelper.ConstructProposedEntitySetName(entity.Artifact, NewName);
                        var newEntitySetName = ModelHelper.GetUniqueName(
                            typeof(EntitySet), (entity.EntityModel.EntityContainer), proposedEntitySetName);
                        RenameCommand cmd = new EntityDesignRenameCommand(entity.EntitySet, newEntitySetName, UniquenessIsCaseSensitive);
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
            }
        }
    }
}
