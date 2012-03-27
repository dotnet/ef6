namespace SimpleModel
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    public class LiveWriterContext : DbContext
    {
        static LiveWriterContext()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<LiveWriterContext>());
        }

        public LiveWriterContext()
        {
        }

        public LiveWriterContext(DbCompiledModel model)
            : base(model)
        {
        }

        public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>().HasKey(k => k.Id);
            base.OnModelCreating(modelBuilder);
        }
    }
}