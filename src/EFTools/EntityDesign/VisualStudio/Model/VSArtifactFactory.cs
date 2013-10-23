// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    internal class VSArtifactFactory : IEFArtifactFactory
    {
        /// <summary>
        ///     The factory that creates VSArtifact and the corresponding DiagramArtifact if diagram file is available.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public IList<EFArtifact> Create(ModelManager modelManager, Uri uri, XmlModelProvider xmlModelProvider)
        {
            var artifact = new VSArtifact(modelManager, uri, xmlModelProvider);

            var artifacts = new List<EFArtifact> { artifact };

            var diagramArtifact = GetDiagramArtifactIfAvailable(modelManager, uri, xmlModelProvider);
            if (diagramArtifact != null)
            {
                artifact.DiagramArtifact = diagramArtifact;
                artifacts.Add(diagramArtifact);
            }

            return artifacts;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static DiagramArtifact GetDiagramArtifactIfAvailable(
            ModelManager modelManager, Uri modelUri, XmlModelProvider xmlModelProvider)
        {
            var diagramFileName = modelUri.OriginalString + EntityDesignArtifact.EXTENSION_DIAGRAM;
            return File.Exists(diagramFileName)
                       ? new VSDiagramArtifact(modelManager, new Uri(diagramFileName), xmlModelProvider)
                       : null;
        }
    }
}
