// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class InMemoryModelBuilderEngine : ModelBuilderEngine
    {
        private XDocument _xdocument;
        private readonly Uri _uri;
        private readonly List<EdmSchemaError> _errors = new List<EdmSchemaError>();
        private readonly IInitialModelContentsFactory _initialModelContentsFactory;

        /// <param name="hostContext">The context in which this model is being generated</param>
        /// <param name="settings">The ModelBuidlerSettings to use when generating the model</param>
        /// <param name="initialModelContentsFactory">A factory that creates the "basic" contents of an empty edmx file</param>
        /// <param name="uri">The uri of the file when it will be saved to disk.  Note that this Uri points to a file that may not yet exist on disk!</param>
        internal InMemoryModelBuilderEngine(
            ModelBuilderEngineHostContext hostContext, ModelBuilderSettings settings,
            IInitialModelContentsFactory initialModelContentsFactory, Uri uri)
            :
                base(hostContext, settings)
        {
            _uri = uri;
            _initialModelContentsFactory = initialModelContentsFactory;
        }

        protected override void AddErrors(IEnumerable<EdmSchemaError> errors)
        {
            _errors.AddRange(errors);
        }

        /// <summary>
        ///     This is the Uri of the file.  Note that it may not yet exist on disk!
        /// </summary>
        protected override Uri Uri
        {
            get { return _uri; }
        }

        /// <summary>
        ///     This is the XDocument of the model in memory.  No assumptions should be made that it exists on disk.
        /// </summary>
        internal override XDocument XDocument
        {
            get
            {
                if (_xdocument == null)
                {
                    _xdocument = XDocument.Parse(
                        _initialModelContentsFactory.GetInitialModelContents(Settings.TargetSchemaVersion));
                }

                Debug.Assert(
                    SchemaManager.GetSchemaVersion(_xdocument.Root.Name.Namespace) == Settings.TargetSchemaVersion,
                    "Schema version should not change or we should not cache the documen");

                return _xdocument;
            }
        }

        internal override IEnumerable<EdmSchemaError> Errors
        {
            get { return _errors; }
        }
    }
}
