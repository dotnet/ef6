// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;

    public interface IDbConvention<TMetadataItem> : IConvention
        where TMetadataItem : MetadataItem
    {
        void Apply(TMetadataItem dbDataModelItem, EdmModel model);
    }
}
