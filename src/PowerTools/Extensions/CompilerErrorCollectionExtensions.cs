// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using System.CodeDom.Compiler;
    using System.Linq;
    using Microsoft.DbContextPackage.Utilities;

    internal static class CompilerErrorCollectionExtensions
    {
        public static void HandleErrors(this CompilerErrorCollection errors, string message)
        {
            DebugCheck.NotNull(errors);
            DebugCheck.NotEmpty(message);

            if (errors.HasErrors)
            {
                throw new CompilerErrorException(
                    message,
                    errors.Cast<CompilerError>().ToList());
            }
        }
    }
}
