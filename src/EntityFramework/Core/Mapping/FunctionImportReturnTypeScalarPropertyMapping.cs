// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    internal sealed class FunctionImportReturnTypeScalarPropertyMapping : FunctionImportReturnTypePropertyMapping
    {
        internal FunctionImportReturnTypeScalarPropertyMapping(string cMember, string sColumn, LineInfo lineInfo)
            : base(cMember, sColumn, lineInfo)
        {
        }
    }
}
