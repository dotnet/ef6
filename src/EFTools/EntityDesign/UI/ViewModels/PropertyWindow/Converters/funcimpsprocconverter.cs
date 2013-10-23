// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;
    using Microsoft.Data.Tools.XmlDesignerBase;
    using EFExtensions = Microsoft.Data.Entity.Design.Model.EFExtensions;

    internal class FuncImpSprocConverter : DynamicListConverter<Function, EFFunctionImportDescriptor>
    {
        protected override void PopulateMappingForSelectedObject(EFFunctionImportDescriptor selectedObject)
        {
            Debug.Assert(selectedObject != null, "selectedObject should not be null");

            if (selectedObject != null)
            {
                AddMapping(null, Resources.NoneDisplayValueUsedForUX);
                var currentType = selectedObject.WrappedItem as FunctionImport;
                if (currentType != null
                    && currentType.Artifact != null
                    && EFExtensions.StorageModel(currentType.Artifact) != null)
                {
                    var functions = EFExtensions.StorageModel(currentType.Artifact).Functions();
                    foreach (var function in functions)
                    {
                        AddMapping(function, function.LocalName.Value);
                    }
                }
            }
        }
    }
}
