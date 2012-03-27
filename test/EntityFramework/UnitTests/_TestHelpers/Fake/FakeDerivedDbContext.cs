namespace System.Data.Entity.ModelConfiguration.Internal.UnitTests
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;

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