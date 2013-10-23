// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Database
{
    using System.Data;

    internal interface IDataSchemaColumn : IRawDataSchemaColumn
    {
        DbType DbType { get; }
    }
}
