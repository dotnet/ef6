// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Infrastructure;

    internal interface IContextGenerator
    {
        string Generate(DbModel model, string codeNamespace, string contextClassName, string connectionStringName);
    }
}