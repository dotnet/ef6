// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    internal class EFArtifactFactory : IEFArtifactFactory
    {
        /// <summary>
        ///     Factory method for EntityDesignArtifact.
        ///     Note that this method will not create DiagramArtifact.
        ///     Please use VSArtifactFactory instead if DiagramArtifact needs to be created and loaded.
        /// </summary>
        public IList<EFArtifact> Create(ModelManager modelManager, Uri uri, XmlModelProvider xmlModelProvider)
        {
            return new List<EFArtifact> { new EntityDesignArtifact(modelManager, uri, xmlModelProvider) };
        }
    }
}
