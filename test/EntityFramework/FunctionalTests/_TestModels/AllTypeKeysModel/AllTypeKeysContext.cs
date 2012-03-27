namespace AllTypeKeysModel
{
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    public class AllTypeKeysContext : DbContext
    {
        static AllTypeKeysContext()
        {
            Database.SetInitializer(new AllTypeKeysModelInitializer());
        }

        public AllTypeKeysContext()
        {
        }

        public AllTypeKeysContext(DbCompiledModel model)
            : base(model)
        {
        }

        // CompositeKeyEntity has ordering of keys specified via configuration
        public DbSet<CompositeKeyEntity> CompositeKeyEntities { get; set; }

        // CompositeKeyEntityWithOrderingAnnotations has ordering of keys specified via annotations 
        public DbSet<CompositeKeyEntityWithOrderingAnnotations> CompositeKeyEntitiesWithOrderingAnnotations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            CreateModel(modelBuilder);
        }

        private static void CreateModel(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompositeKeyEntity>().HasKey(k => new { k.binaryKey, k.intKey, k.stringKey });
            modelBuilder.Entity<CompositeKeyEntityWithOrderingAnnotations>();
            
            modelBuilder.Entity<BoolKeyEntity>();
            modelBuilder.Entity<ByteKeyEntity>();
            modelBuilder.Entity<DateTimeKeyEntity>();
            modelBuilder.Entity<DateTimeOffSetKeyEntity>();
            modelBuilder.Entity<DecimalKeyEntity>();
            modelBuilder.Entity<DoubleKeyEntity>();
            modelBuilder.Entity<FloatKeyEntity>();
            modelBuilder.Entity<GuidKeyEntity>();
            modelBuilder.Entity<LongKeyEntity>().Property(s => s.key).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<ShortKeyEntity>().Property(s => s.key).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<TimeSpanKeyEntity>();
        }

        public static DbModelBuilder CreateBuilder()
        {
            var builder = new DbModelBuilder();

            CreateModel(builder);

            return builder;
        }
    }
}