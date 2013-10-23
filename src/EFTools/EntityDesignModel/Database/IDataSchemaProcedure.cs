// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Database
{
    using System.Collections.Generic;

    internal interface IDataSchemaProcedure : IRawDataSchemaProcedure
    {
        IList<IDataSchemaParameter> Parameters { get; }
        IList<IDataSchemaColumn> Columns { get; }
        IDataSchemaParameter ReturnValue { get; }
    }
}
