// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    /// <summary>
    ///     Enumeration of Boolean expression node types.
    /// </summary>
    internal enum ExprType
    {
        And,
        Not,
        Or,
        Term,
        True,
        False,
    }
}
