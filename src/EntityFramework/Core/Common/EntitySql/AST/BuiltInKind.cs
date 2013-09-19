// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    /// <summary>
    /// Defines the function class of builtin expressions.
    /// </summary>
    internal enum BuiltInKind
    {
        And,
        Or,
        Not,

        Cast,
        OfType,
        Treat,
        IsOf,

        Union,
        UnionAll,
        Intersect,
        Overlaps,
        AnyElement,
        Element,
        Except,
        Exists,
        Flatten,
        In,
        NotIn,
        Distinct,

        IsNull,
        IsNotNull,

        Like,

        Equal,
        NotEqual,
        LessEqual,
        LessThan,
        GreaterThan,
        GreaterEqual,

        Plus,
        Minus,
        Multiply,
        Divide,
        Modulus,
        UnaryMinus,
        UnaryPlus,

        Between,
        NotBetween
    }
}
