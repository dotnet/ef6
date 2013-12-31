// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;

    internal interface ITypeConfigurationDiscoverer
    {
        IConfiguration Discover(EntitySet entitySet, DbModel model);
    }
}
