namespace SimpleModel
{
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;

    public class SimpleLocalDbModelContextWithNoData : DbContext
    {
        static SimpleLocalDbModelContextWithNoData()
        {
            Database.SetInitializer(new DropCreateDatabaseAlways<SimpleModelContextWithNoData>());
        }

        public SimpleLocalDbModelContextWithNoData()
        {
        }

        public SimpleLocalDbModelContextWithNoData(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public SimpleLocalDbModelContextWithNoData(DbCompiledModel model)
            : base(model)
        {
        }

        public SimpleLocalDbModelContextWithNoData(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        public SimpleLocalDbModelContextWithNoData(DbConnection existingConnection, bool contextOwnsConnection = false)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public SimpleLocalDbModelContextWithNoData(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection = false)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        public SimpleLocalDbModelContextWithNoData(ObjectContext objectContext, bool dbContextOwnsObjectContext = false)
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
    }
}