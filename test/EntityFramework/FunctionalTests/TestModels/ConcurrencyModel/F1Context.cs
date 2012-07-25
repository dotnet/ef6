// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace ConcurrencyModel
{
    using System.Data.Entity.Core.Common;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;

    public class F1Context : DbContext
    {
        static F1Context()
        {
            Database.SetInitializer(new ConcurrencyModelInitializer());
        }

        public F1Context(bool lazyLoadingEnabled = true, bool proxyCreationEnabled = true)
        {
            SetContextOptions(lazyLoadingEnabled, proxyCreationEnabled);
        }

        public F1Context(DbCompiledModel model, bool lazyLoadingEnabled = true, bool proxyCreationEnabled = true)
            : base(model)
        {
            SetContextOptions(lazyLoadingEnabled, proxyCreationEnabled);
        }

        public F1Context(string nameOrConnectionString, bool lazyLoadingEnabled = true, bool proxyCreationEnabled = true)
            : base(nameOrConnectionString)
        {
            SetContextOptions(lazyLoadingEnabled, proxyCreationEnabled);
        }

        public F1Context(string nameOrConnectionString, DbCompiledModel model, bool lazyLoadingEnabled = true, bool proxyCreationEnabled = true)
            : base(nameOrConnectionString, model)
        {
            SetContextOptions(lazyLoadingEnabled, proxyCreationEnabled);
        }

        public F1Context(DbConnection existingConnection, bool contextOwnsConnection, bool lazyLoadingEnabled = true, bool proxyCreationEnabled = true)
            : base(existingConnection, contextOwnsConnection)
        {
            SetContextOptions(lazyLoadingEnabled, proxyCreationEnabled);
        }

        public F1Context(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection, bool lazyLoadingEnabled = true, bool proxyCreationEnabled = true)
            : base(existingConnection, model, contextOwnsConnection)
        {
            SetContextOptions(lazyLoadingEnabled, proxyCreationEnabled);
        }

        public F1Context(ObjectContext objectContext, bool dbContextOwnsObjectContext, bool lazyLoadingEnabled = true, bool proxyCreationEnabled = true)
            : base(objectContext, dbContextOwnsObjectContext)
        {
            SetContextOptions(lazyLoadingEnabled, proxyCreationEnabled);
        }

        private void SetContextOptions(bool lazyLoadingEnabled, bool proxyCreationEnabled)
        {
            // Only change the flags if set to false so that the tests can test that the defaults are true.
            if (!lazyLoadingEnabled)
            {
                Configuration.LazyLoadingEnabled = false;
            }
            if (!proxyCreationEnabled)
            {
                Configuration.ProxyCreationEnabled = false;
            }
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<Engine> Engines { get; set; }
        public DbSet<EngineSupplier> EngineSuppliers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            AdditionalModelConfiguration(modelBuilder);
        }

        public static void AdditionalModelConfiguration(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chassis>().HasRequired(c => c.Team).WithRequiredDependent(t => t.Chassis).WillCascadeOnDelete();
            modelBuilder.ComplexType<Location>();
            modelBuilder.Entity<Sponsor>().ToTable("Sponsors");
            modelBuilder.Entity<TitleSponsor>().ToTable("TitleSponsors");
        }
    }
}