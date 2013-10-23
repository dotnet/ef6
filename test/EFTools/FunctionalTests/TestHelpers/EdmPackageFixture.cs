// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.TestHelpers
{
    using System;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;

    public class EdmPackageFixture : IDisposable
    {
        private readonly EntityDesignModelManager _modelManager;
        private IEdmPackage _package;

        public EdmPackageFixture()
        {
            _modelManager = new EntityDesignModelManager(
                new EFArtifactFactory(),
                new EFArtifactSetFactory());

            _package = new MockPackage(_modelManager);
        }

        public void Dispose()
        {
            if (_modelManager != null)
            {
                _modelManager.Dispose();
            }

            PackageManager.Package = null;
        }
    }
}
