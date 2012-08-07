// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Data.Common;

    internal class EmptyContext : DbContext
    {
        static EmptyContext()
        {
            Database.SetInitializer<EmptyContext>(null);
        }

        public EmptyContext(DbConnection existingConnection)
            : base(existingConnection, false)
        {
        }
    }
}
