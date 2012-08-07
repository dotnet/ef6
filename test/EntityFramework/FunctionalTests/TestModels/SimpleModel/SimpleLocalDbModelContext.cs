// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;

    [DbModelBuilderVersion(DbModelBuilderVersion.Latest)]
    public class SimpleLocalDbModelContext : DbContext
    {
        public SimpleLocalDbModelContext()
        {
        }

        public SimpleLocalDbModelContext(DbCompiledModel model)
            : base(model)
        {
        }

        public SimpleLocalDbModelContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public SimpleLocalDbModelContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public SimpleLocalDbModelContext(DbConnection existingConnection, bool contextOwnsConnection = false)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public SimpleLocalDbModelContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection = false)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        public SimpleLocalDbModelContext(ObjectContext objectContext, bool dbContextOwnsObjectContext = false)
            : base(objectContext, dbContextOwnsObjectContext)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public static DbModelBuilder CreateBuilder()
        {
            var builder = new DbModelBuilder();

            builder.Entity<Product>();
            builder.Entity<Category>();

            return builder;
        }
    }
}
