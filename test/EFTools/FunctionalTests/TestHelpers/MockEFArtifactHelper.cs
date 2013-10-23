// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.TestHelpers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using EFDesignerTestInfrastructure.EFDesigner;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;
    using Microsoft.Data.Tools.XmlDesignerBase.Model.StandAlone;

    internal class MockEFArtifactHelper : EFArtifactHelper, IDisposable
    {
        private readonly XmlModelProvider _modelProvider;

        internal MockEFArtifactHelper()
            :
                base(new EntityDesignModelManager(new EFArtifactFactory(), new EFArtifactSetFactory()))
        {
            _modelProvider = new VanillaXmlModelProvider();
        }

        internal override EFArtifact GetNewOrExistingArtifact(Uri uri)
        {
            Debug.Assert(uri != null, "uri must not be null.");

            return GetNewOrExistingArtifact(uri, _modelProvider);
        }

        // TODO: do we need dispose pattern here, throw for disposed objects
        public void Dispose()
        {
            foreach (var uri in _modelManager.Artifacts.Select(a => a.Uri).ToArray())
            {
                ClearArtifact(uri);
            }

            _modelManager.Dispose();
        }
    }
}
