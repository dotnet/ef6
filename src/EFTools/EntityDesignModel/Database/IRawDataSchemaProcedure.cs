// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Database
{
    using System.Collections.Generic;

    internal interface IRawDataSchemaProcedure
    {
        IList<IRawDataSchemaParameter> RawParameters { get; }

        /// <summary>
        ///     Used to determine the shape of the resultset of non-Functions
        /// </summary>
        IList<IRawDataSchemaColumn> RawColumns { get; }

        /// <summary>
        ///     Used to determine the return type of Functions
        /// </summary>
        IRawDataSchemaParameter RawReturnValue { get; }

        bool HasRows { get; }

        bool IsFunction { get; }

        string Name { get; }

        string Schema { get; }
    }
}
