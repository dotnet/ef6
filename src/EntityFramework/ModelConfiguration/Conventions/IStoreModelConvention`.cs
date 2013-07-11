// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;

    public interface IStoreModelConvention<T> : IConvention
        where T : MetadataItem
    {
        void Apply(T item, DbModel model);
    }
}
