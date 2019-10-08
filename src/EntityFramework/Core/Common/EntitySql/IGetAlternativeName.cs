// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    internal interface IGetAlternativeName
    {
        // <summary>
        // If current scope entry represents an alternative group key name (see SemanticAnalyzer.ProcessGroupByClause(...) for more info)
        // then this property returns the alternative name, otherwise null.
        // </summary>
        string[] AlternativeName { get; }
    }
}
