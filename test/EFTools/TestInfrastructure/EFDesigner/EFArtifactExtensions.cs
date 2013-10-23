// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure.EFDesigner
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    internal static class EFArtifactExtensions
    {
        public static EditingContext GetEditingContext(this EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "artifact != null");

            var service = new EFArtifactService(artifact);
            var editingContext = new EditingContext();
            editingContext.SetEFArtifactService(service);
            return editingContext;
        }

        public static string LocalPath(this EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "artifact != null");

            return artifact.Uri.LocalPath;
        }
    }
}
