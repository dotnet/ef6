// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Model
{
    using Microsoft.Data.Entity.Design.Model;

    internal class VSArtifactSetFactory : IEFArtifactSetFactory
    {
        public EFArtifactSet CreateArtifactSet(EFArtifact artifact)
        {
            return new VSArtifactSet(artifact);
        }
    }
}
