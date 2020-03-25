// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Infrastructure;

    internal static class CustomOperationExtensions
    {
        public static void Comment(this IDbMigration migration, string text, object anonymousArguments = null)
        {
            migration.AddOperation(new CommentOperation(text, anonymousArguments));
        }
    }
}
