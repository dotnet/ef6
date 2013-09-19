// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    internal abstract class FunctionImportReturnTypePropertyMapping
    {
        internal readonly string CMember;
        internal readonly string SColumn;
        internal readonly LineInfo LineInfo;

        internal FunctionImportReturnTypePropertyMapping(string cMember, string sColumn, LineInfo lineInfo)
        {
            CMember = cMember;
            SColumn = sColumn;
            LineInfo = lineInfo;
        }
    }
}
