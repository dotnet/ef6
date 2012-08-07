// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Db
{
    using System.Data.Entity.Edm.Common;

    /// <summary>
    ///     DbDataModelItem is the base for all types in the Database Metadata construction and modification API.
    /// </summary>
    internal abstract class DbDataModelItem
        : DataModelItem
    {
        internal abstract DbItemKind GetMetadataKind();
    }
}
