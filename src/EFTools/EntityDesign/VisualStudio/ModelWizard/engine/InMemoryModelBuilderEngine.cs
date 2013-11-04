// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.VisualStudio.OLE.Interop;

    internal class InMemoryModelBuilderEngine : ModelBuilderEngine
    {
        private XDocument _model;
        private readonly List<EdmSchemaError> _errors = new List<EdmSchemaError>();
        private readonly IInitialModelContentsFactory _initialModelContentsFactory;

        /// <param name="initialModelContentsFactory">A factory that creates the "basic" contents of an empty edmx file</param>
        internal InMemoryModelBuilderEngine(IInitialModelContentsFactory initialModelContentsFactory)
        {
            _initialModelContentsFactory = initialModelContentsFactory;
        }

        protected override void AddErrors(IEnumerable<EdmSchemaError> errors)
        {
            _errors.AddRange(errors);
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

        /// <summary>
        ///     This is the XDocument of the model in memory.  No assumptions should be made that it exists on disk.
        /// </summary>
        internal override XDocument Model
        {
            get
            {
                Debug.Assert(_model != null, "Model document has not been initialized.");
                return _model;
            }
        }

        internal override IEnumerable<EdmSchemaError> Errors
        {
            get { return _errors; }
        }
    }
}
