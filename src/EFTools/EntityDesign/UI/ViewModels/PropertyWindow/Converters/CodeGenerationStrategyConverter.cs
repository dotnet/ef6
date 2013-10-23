// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;

    internal class CodeGenerationStrategyConverter : DynamicListConverter<string, EFEntityModelDescriptor>
    {
        protected override void PopulateMappingForSelectedObject(EFEntityModelDescriptor selectedObject)
        {
            AddMapping(Resources.None, Resources.CodeGenerationStrategy_T4);
            AddMapping(Resources.Default, Resources.CodeGenerationStrategy_LegacyObjectContext);
        }
    }
}
