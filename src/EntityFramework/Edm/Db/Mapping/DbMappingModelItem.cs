// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db.Mapping
{
    using System.Data.Entity.Edm.Common;

    /// <summary>
    ///     DbMappingModelItem is the base for all types in the EDM-to-Database Mapping construction and modification API.
    /// </summary>
    public abstract class DbMappingModelItem : DataModelItem
    {
        internal abstract DbMappingItemKind GetItemKind();
    }
}
