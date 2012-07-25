// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Migrations.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;

    internal class DatabaseCreator
    {
        public virtual void Create(DbConnection connection)
        {
            using (var context = new EmptyContext(connection))
            {
                // Drop down to ObjectContext here to avoid recursive calls into the Migrations
                // pipeline and so that MigrationHistory table is not created by DbContext.
                ((IObjectContextAdapter)context).ObjectContext.CreateDatabase();
            }
        }
    }
}
