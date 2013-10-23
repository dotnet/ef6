// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure.EFDesigner
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    // TODO: internal methods on this type are internal only 
    // because they take parameters of types that are internal
    internal class EFArtifactHelper
    {
        protected readonly ModelManager _modelManager;

        internal EFArtifactHelper(ModelManager modelManager)
        {
            Debug.Assert(modelManager != null, "modelManager must not be null.");

            _modelManager = modelManager;
        }

        internal virtual EFArtifact GetNewOrExistingArtifact(Uri artifactUri)
        {
            Debug.Assert(artifactUri != null, "artifactUri must not be null.");

            return _modelManager.GetArtifact(artifactUri);
        }

        internal EFArtifact GetNewOrExistingArtifact(Uri artifactUri, XmlModelProvider modelProvider)
        {
            Debug.Assert(artifactUri != null, "artifactUri must not be null.");
            Debug.Assert(modelProvider != null, "modelProvider must not be null.");

            return _modelManager.GetNewOrExistingArtifact(artifactUri, modelProvider);
        }

        internal void ClearArtifact(Uri artifactUri)
        {
            _modelManager.ClearArtifact(artifactUri);
        }

        // TODO: move this to a better place (HIGH PRIORITY)
        internal static ModelManager GetEntityDesignModelManager(IServiceProvider serviceProvider)
        {
            Debug.Assert(serviceProvider != null, "serviceProvider must not be null.");

            PackageManager.LoadEDMPackage(serviceProvider);
            return PackageManager.Package.ModelManager;
        }
    }
}
