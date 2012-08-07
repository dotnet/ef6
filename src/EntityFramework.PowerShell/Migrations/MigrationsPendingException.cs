// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Infrastructure;

    [Serializable]
    public sealed class MigrationsPendingException : MigrationsException
    {
        public MigrationsPendingException(string message)
            : base(message)
        {
        }
    }
}
