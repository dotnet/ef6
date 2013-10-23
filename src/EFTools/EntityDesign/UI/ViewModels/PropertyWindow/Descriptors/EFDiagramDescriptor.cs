// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using Microsoft.Data.Entity.Design.Model.Designer;

    internal class EFDiagramDescriptor : EFAnnotatableElementDescriptor<Diagram>
    {
        [LocDescription("PropertyWindow_Description_DiagramName")]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "Diagram";
        }
    }
}
