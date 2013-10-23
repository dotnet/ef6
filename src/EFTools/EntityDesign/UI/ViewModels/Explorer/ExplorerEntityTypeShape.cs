// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;

    internal class ExplorerEntityTypeShape : EntityDesignExplorerEFElement
    {
        public ExplorerEntityTypeShape(EditingContext context, EntityTypeShape entityTypeShape, ExplorerEFElement parent)
            : base(context, entityTypeShape, parent)
        {
            // do nothing.
        }

        protected override void LoadChildrenFromModel()
        {
            // do nothing.
        }

        protected override void LoadWpfChildrenCollection()
        {
            // do nothing
        }

        internal override string ExplorerImageResourceKeyName
        {
            get
            {
                // Placeholder icon.
                return "EntityTypePngIcon";
            }
        }

        // the name of Entity Types are editable inline in the Explorer
        public override bool IsEditableInline
        {
            get { return true; }
        }

        public override string Name
        {
            get
            {
                var entityTypeShape = ModelItem as EntityTypeShape;
                Debug.Assert(entityTypeShape != null, "The underlying entity type shape is null.");
                if (entityTypeShape != null)
                {
                    var entityType = entityTypeShape.EntityType.Target;
                    Debug.Assert(entityType != null, "The EntityTypeShape does not contain an instance of EntityType.");
                    if (entityType != null)
                    {
                        return entityType.DisplayName;
                    }
                }
                return String.Empty;
            }
        }

        protected override bool RenameModelElement(CommandProcessorContext cpc, string newName)
        {
            // EntityTypeShape is not a nameable item so it could not be renamed.
            // When the object is renamed, we actually rename its underlying entity-type.            
            var entityTypeShape = ModelItem as EntityTypeShape;
            var entityType = entityTypeShape.EntityType.Target;
            Debug.Assert(entityType != null, "EntityType is null.");

            if (entityType != null
                && entityType.Artifact != null
                && entityType.Artifact.ModelManager != null)
            {
                CommandProcessor.InvokeSingleCommand(
                    cpc
                    , entityType.Artifact.ModelManager.CreateRenameCommand(entityType, newName, true));
                return true;
            }
            return false;
        }
    }
}
