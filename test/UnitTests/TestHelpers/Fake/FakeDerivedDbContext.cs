// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Internal.UnitTests
{
    using System.Data.Common;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;

    public class FakeDerivedDbContext : DbContext
    {
        public FakeDerivedDbContext()
        {
        }

        public FakeDerivedDbContext(DbCompiledModel model)
            : base(model)
        {
        }

        public FakeDerivedDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public FakeDerivedDbContext(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public FakeDerivedDbContext(DbConnection existingConnection, bool contextOwnsConnection = false)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public FakeDerivedDbContext(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection = false)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        public FakeDerivedDbContext(ObjectContext objectContext, bool dbContextOwnsObjectContext = false)
            : base(objectContext, dbContextOwnsObjectContext)
        {
        }

        public DbSet<FakeEntity> Base { get; set; }
    }
}
