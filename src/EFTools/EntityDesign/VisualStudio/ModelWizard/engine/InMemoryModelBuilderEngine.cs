// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using System;
    using System.Diagnostics;
    using System.Xml.Linq;

    internal class InMemoryModelBuilderEngine : EdmxModelBuilderEngine
    {
        private XDocument _model;
        private readonly IInitialModelContentsFactory _initialModelContentsFactory;

        // <param name="initialModelContentsFactory">A factory that creates the "basic" contents of an empty edmx file</param>
        internal InMemoryModelBuilderEngine(IInitialModelContentsFactory initialModelContentsFactory)
        {
            _initialModelContentsFactory = initialModelContentsFactory;
        }

        protected override void InitializeModelContents(Version targetSchemaVersion)
        {
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(targetSchemaVersion));
            Debug.Assert(_model == null, "overwriting already initialized edmx???");

            _model = XDocument.Parse(
                _initialModelContentsFactory.GetInitialModelContents(targetSchemaVersion));

            Debug.Assert(
                SchemaManager.GetSchemaVersion(_model.Root.Name.Namespace) == targetSchemaVersion,
                "Schema version should not change or we should not cache the document");            
        }

        // <summary>
        //     This is the XDocument of the model in memory.  No assumptions should be made that it exists on disk.
        // </summary>
        internal override XDocument Model
        {
            get
            {
                Debug.Assert(_model != null, "Model document has not been initialized.");
                return _model;
            }
        }
    }
}
