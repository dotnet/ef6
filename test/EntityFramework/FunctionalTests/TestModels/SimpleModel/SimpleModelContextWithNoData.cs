namespace SimpleModel
{
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Threading;
    using System.Threading.Tasks;

    public class SimpleModelContextWithNoData : DbContext
    {
        static SimpleModelContextWithNoData()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<SimpleModelContextWithNoData>());
        }

        public SimpleModelContextWithNoData()
        {
        }

        public SimpleModelContextWithNoData(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public SimpleModelContextWithNoData(DbCompiledModel model)
            : base(model)
        {
        }

        public SimpleModelContextWithNoData(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public SimpleModelContextWithNoData(DbConnection existingConnection, bool contextOwnsConnection = false)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public SimpleModelContextWithNoData(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection = false)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        public SimpleModelContextWithNoData(ObjectContext objectContext, bool dbContextOwnsObjectContext = false)
            : base(objectContext, dbContextOwnsObjectContext)
        {
        }

        public IDbSet<Product> Products { get; set; }
        public IDbSet<Category> Categories { get; set; }

        public bool SaveChangesCalled { get; set; }

        public override int SaveChanges()
        {
            SaveChangesCalled = true;
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalled = true;
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
