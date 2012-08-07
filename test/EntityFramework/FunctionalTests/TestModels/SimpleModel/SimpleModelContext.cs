// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;

    [DbModelBuilderVersion(DbModelBuilderVersion.Latest)]
    public class SimpleModelContext : DbContext
    {
        public SimpleModelContext()
        {
        }

        public SimpleModelContext(DbCompiledModel model)
            : base(model)
        {
        }

        public SimpleModelContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public SimpleModelContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public SimpleModelContext(DbConnection existingConnection, bool contextOwnsConnection = false)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public SimpleModelContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection = false)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        public SimpleModelContext(ObjectContext objectContext, bool dbContextOwnsObjectContext = false)
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
