// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;

    public interface IEdmConvention<TEdmDataModelItem> : IConvention
        where TEdmDataModelItem : MetadataItem
    {
        void Apply(TEdmDataModelItem edmDataModelItem, EdmModel model);
    }
}
