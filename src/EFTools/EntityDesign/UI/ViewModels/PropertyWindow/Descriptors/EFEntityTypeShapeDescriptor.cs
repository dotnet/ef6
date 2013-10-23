// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;

    // We need to differentiate EFEntityTypeShapeDescriptor from EFEntityTypeDescriptor because
    // There is additional property that we want to show for Entity type shape (for example: color).
    internal class EFEntityTypeShapeDescriptor : EFEntityTypeDescriptor
    {
        private EntityTypeShape _entityTypeShape;

        internal override void Initialize(EFObject obj, EditingContext editingContext, bool runningInVS)
        {
            _entityTypeShape = obj as EntityTypeShape;

            Debug.Assert(_entityTypeShape != null, "EFObject is null or is not a type of EntityTypeShape.");

            if (_entityTypeShape != null)
            {
                var entityType = _entityTypeShape.EntityType.Target;
                Debug.Assert(entityType != null, "EntityTypeShape does not contain instance of an entity type.");
                if (entityType != null)
                {
                    base.Initialize(entityType, editingContext, runningInVS);
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Diagram")]
        [LocDisplayName("PropertyWindow_DisplayName_EntityShapeColor")]
        [LocDescription("PropertyWindow_Description_EntityShapeColor")]
        [Editor(typeof(ColorEditor), typeof(UITypeEditor))]
        public Color FillColor
        {
            get { return _entityTypeShape.FillColor.Value; }
            set
            {
                // When the user clear out the property value, set it back to default color.
                if (value.IsEmpty)
                {
                    value = EntityDesignerDiagramConstant.EntityTypeShapeDefaultFillColor;
                }

                if (value == FillColor)
                {
                    return;
                }

                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                CommandProcessor.InvokeSingleCommand(cpc, new UpdateDefaultableValueCommand<Color>(_entityTypeShape.FillColor, value));
            }
        }
    }
}
