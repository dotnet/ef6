// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters
{
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;

    internal class MetadataArtifactProcessingConverter : DynamicListConverter<string, EFEntityModelDescriptor>
    {
        protected override void PopulateMappingForSelectedObject(EFEntityModelDescriptor selectedObject)
        {
            var documentPath = selectedObject.EditingContext.GetEFArtifactService().Artifact.Uri.LocalPath;
            var project = VSHelpers.GetProjectForDocument(documentPath, PackageManager.Package);
            if (project != null)
            {
                var appType = VsUtils.GetApplicationType(Services.ServiceProvider, project);
                if (appType != VisualStudioProjectSystem.Website)
                {
                    AddMapping(
                        ConnectionDesignerInfo.MAP_CopyToOutputDirectory, Resources.PropertyWindow_DisplayName_MAP_CopyToOutputDirectory);
                }
            }
            else
            {
                AddMapping(ConnectionDesignerInfo.MAP_CopyToOutputDirectory, Resources.PropertyWindow_DisplayName_MAP_CopyToOutputDirectory);
            }
            AddMapping(ConnectionDesignerInfo.MAP_EmbedInOutputAssembly, Resources.PropertyWindow_DisplayName_MAP_EmbedInOutputAssembly);
        }
    }
}
