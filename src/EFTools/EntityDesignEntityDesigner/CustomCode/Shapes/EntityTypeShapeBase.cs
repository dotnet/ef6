// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.View
{
    using System.Drawing;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.VisualStudio.Modeling;

    internal abstract partial class EntityTypeShapeBase
    {
        private static string GetDisplayPropertyFromEntityTypeForProperties(ModelElement element)
        {
            var property = element as Property;
            if (property == null)
            {
                return string.Empty;
            }

            if (property.EntityType.EntityDesignerViewModel.GetDiagram().DisplayNameAndType)
            {
                return property.Name + " : " + property.Type;
            }

            return property.Name;
        }

        /// <summary>
        ///     EntityTypeShape method to receive notification of changes to FillColor.
        /// </summary>
        protected abstract void OnFillColorChanged(Color newValue);

        internal sealed partial class FillColorPropertyHandler
        {
            /// <summary>
            ///     Hookup to value handler method to get notified when FillColor value has changed.
            /// </summary>
            protected override void OnValueChanged(EntityTypeShapeBase element, Color oldValue, Color newValue)
            {
                base.OnValueChanged(element, oldValue, newValue);
                element.OnFillColorChanged(newValue);
            }
        }
    }
}
