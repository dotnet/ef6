// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.NotReady;
    using System.Data.Entity.Migrations.UpDownUp;
    using System.Linq;
    using Xunit;

    public class InitializerScenarios : FunctionalTestBase
    {
        [Fact] // CodePlex 1709
        public void DropCreateDatabaseAlways_with_no_database_uses_Migrations_configuration_if_available()
        {
            EnsureDeleted<AlwaysDropContext1>();

            VerifyInitialized<AlwaysDropContext1, AlwaysDropMigrationsConfig1>();
        }

        public class AlwaysDropContext1 : UpDownUpContext
        {
            static AlwaysDropContext1()
            {
                Database.SetInitializer<AlwaysDropContext1>(new AlwaysDropInitializer());
            }
        }

        [Fact] // CodePlex 1709
        public void DropCreateDatabaseAlways_with_out_of_date_database_uses_Migrations_configuration_if_available()
        {
            EnsureDeleted<AlwaysDropContext2>();

            new DbMigrator(new AlwaysDropMigrationsConfig2()).Update("201303090309299_Test");

            VerifyInitialized<AlwaysDropContext2, AlwaysDropMigrationsConfig2>();
        }

        public class AlwaysDropContext2 : UpDownUpContext
        {
            static AlwaysDropContext2()
            {
                Database.SetInitializer<AlwaysDropContext2>(new AlwaysDropInitializer());
            }
        }

        [Fact] // CodePlex 1709
        public void DropCreateDatabaseAlways_with_up_to_date_database_uses_Migrations_configuration_if_available()
        {
            EnsureDeleted<AlwaysDropContext3>();

            new DbMigrator(new AlwaysDropMigrationsConfig3()).Update();

            VerifyInitialized<AlwaysDropContext3, AlwaysDropMigrationsConfig3>();
        }

        public class AlwaysDropContext3 : UpDownUpContext
        {
            static AlwaysDropContext3()
            {
                Database.SetInitializer<AlwaysDropContext3>(new AlwaysDropInitializer());
            }
        }

        public class AlwaysDropInitializer : DropCreateDatabaseAlways<UpDownUpContext>
        {
            protected override void Seed(UpDownUpContext context)
            {
                Seeder.InitializerSeed(context);
            }
        }

        [Fact] // CodePlex 1709
        public void DropCreateDatabaseIfModelChanges_with_no_database_uses_Migrations_configuration_if_available()
        {
            EnsureDeleted<ModelChangesContext1>();

            VerifyInitialized<ModelChangesContext1, ModelChangesMigrationsConfig1>();
        }

        public class ModelChangesContext1 : UpDownUpContext
        {
            static ModelChangesContext1()
            {
                Database.SetInitializer<ModelChangesContext1>(new ModelChangesInitializer());
            }
        }

        [Fact] // CodePlex 1709
        public void DropCreateDatabaseIfModelChanges_with_out_of_date_database_uses_Migrations_configuration_if_available()
        {
            EnsureDeleted<ModelChangesContext2>();

            new DbMigrator(new ModelChangesMigrationsConfig2()).Update("201303090309299_Test");

            VerifyInitialized<ModelChangesContext2, ModelChangesMigrationsConfig2>();
        }

        public class ModelChangesContext2 : UpDownUpContext
        {
            static ModelChangesContext2()
            {
                Database.SetInitializer<ModelChangesContext2>(new ModelChangesInitializer());
            }
        }

        [Fact] // CodePlex 1709
        public void DropCreateDatabaseIfModelChanges_with_up_to_date_database_uses_Migrations_configuration_if_available()
        {
            EnsureDeleted<ModelChangesContext3>();

            new DbMigrator(new ModelChangesMigrationsConfig3()).Update();

            VerifyMigrated<ModelChangesContext3, ModelChangesMigrationsConfig3>();
        }

        public class ModelChangesContext3 : UpDownUpContext
        {
            static ModelChangesContext3()
            {
                Database.SetInitializer<ModelChangesContext3>(new ModelChangesInitializer());
            }
        }

        public class ModelChangesInitializer : DropCreateDatabaseIfModelChanges<UpDownUpContext>
        {
            protected override void Seed(UpDownUpContext context)
            {
                Seeder.InitializerSeed(context);
            }
        }

        [Fact] // CodePlex 1709
        public void CreateDatabaseIfNotExists_with_no_database_uses_Migrations_configuration_if_available()
        {
            EnsureDeleted<CreateFirstContext1>();

            VerifyInitialized<CreateFirstContext1, CreateFirstMigrationsConfig1>();
        }

        public class CreateFirstContext1 : UpDownUpContext
        {
            static CreateFirstContext1()
            {
                Database.SetInitializer<CreateFirstContext1>(new CreateFirstInitializer());
            }
        }

        [Fact] // CodePlex 1709
        public void CreateDatabaseIfNotExists_with_out_of_date_database_uses_Migrations_configuration_if_available()
        {
            EnsureDeleted<CreateFirstContext2>();

            new DbMigrator(new CreateFirstMigrationsConfig2()).Update("201303090309299_Test");

            using (var context = new CreateFirstContext2())
            {
                Assert.Throws<InvalidOperationException>(() => context.Database.Initialize(force: false))
                    .ValidateMessage("DatabaseInitializationStrategy_ModelMismatch", "CreateFirstContext2");
            }
        }

        public class CreateFirstContext2 : UpDownUpContext
        {
            static CreateFirstContext2()
            {
                Database.SetInitializer<CreateFirstContext2>(new CreateFirstInitializer());
            }
        }

        [Fact] // CodePlex 1709
        public void CreateDatabaseIfNotExists_with_up_to_date_database_uses_Migrations_configuration_if_available()
        {
            EnsureDeleted<CreateFirstContext3>();

            new DbMigrator(new CreateFirstMigrationsConfig3()).Update();

            VerifyMigrated<CreateFirstContext3, CreateFirstMigrationsConfig3>();
        }

        public class CreateFirstContext3 : UpDownUpContext
        {
            static CreateFirstContext3()
            {
                Database.SetInitializer<CreateFirstContext3>(new CreateFirstInitializer());
            }
        }

        public class CreateFirstInitializer : CreateDatabaseIfNotExists<UpDownUpContext>
        {
            protected override void Seed(UpDownUpContext context)
            {
                Seeder.InitializerSeed(context);
            }
        }

        public class MigrationRow
        {
            public string MigrationId { get; set; }
            public string ContextKey { get; set; }
        }

        public class UpDownUpContext : DbContext
        {
            public DbSet<UpDownUpEntity> Entities { get; set; }
        }

        public class UpDownUpEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Extra { get; set; }
        }

        [Fact] // CodePlex 529, 1709
        public void Creating_database_throws_if_Migrations_configuration_is_found_but_not_ready_to_update()
        {
            using (var context = new NotReadyContext())
            {
                context.Database.Delete();

                Assert.Throws<AutomaticMigrationsDisabledException>(() => context.Database.Initialize(force: true))
                    .ValidateMessage("AutomaticDisabledException");

                Assert.False(context.Database.Exists());
            }

            var migrator = new DbMigrator(new NotreadyMigrationsConfig());

            Assert.Throws<AutomaticMigrationsDisabledException>(
                () => migrator.Update()).ValidateMessage("AutomaticDisabledException");
        }

        public class NotReadyContext : DbContext
        {
            public DbSet<UpDownUpEntity> Entities { get; set; }
        }

        private static void EnsureDeleted<TContext>()
        where TContext : UpDownUpContext, new()
        {
            using (var context = new TContext())
            {
                context.Database.Delete();
            }
        }

        private static void VerifyInitialized<TContext, TMigrationsConfig>()
            where TContext : UpDownUpContext, new()
        {
            VerifyInitializedOrMigrated<TContext, TMigrationsConfig>("Initializer");
        }

        private static void VerifyMigrated<TContext, TMigrationsConfig>()
            where TContext : UpDownUpContext, new()
        {
            VerifyInitializedOrMigrated<TContext, TMigrationsConfig>("Migrations");
        }

        private static void VerifyInitializedOrMigrated<TContext, TMigrationsConfig>(string expectedSeed)
            where TContext : UpDownUpContext, new()
        {
            using (var context = new TContext())
            {
                context.Database.Initialize(force: false);

                Assert.True(context.Database.Exists());

                var upDownUpEntity = context.Entities.Single();
                Assert.Equal("Seeded", upDownUpEntity.Name);
                Assert.Equal(expectedSeed, upDownUpEntity.Extra);

                var migrations = context.Database.SqlQuery<MigrationRow>("select * from __MigrationHistory").ToList();

                Assert.Equal(2, migrations.Count);
                Assert.True(migrations.All(m => m.ContextKey == typeof(TMigrationsConfig).ToString()));
                Assert.Equal("201303090309299_Test", migrations[0].MigrationId);
                Assert.Equal("201303101635379_Second", migrations[1].MigrationId);
            }
        }
    }
}

namespace System.Data.Entity.Migrations.UpDownUp
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations.Infrastructure;

    public class UpDownUpMigrationsConfig : DbMigrationsConfiguration<InitializerScenarios.UpDownUpContext>
    {
        protected override void Seed(InitializerScenarios.UpDownUpContext context)
        {
            Seeder.Seed(context);
        }
    }

    public class AlwaysDropMigrationsConfig1 : DbMigrationsConfiguration<InitializerScenarios.AlwaysDropContext1>
    {
        protected override void Seed(InitializerScenarios.AlwaysDropContext1 context)
        {
            Seeder.Seed(context);
        }
    }

    public class AlwaysDropMigrationsConfig2 : DbMigrationsConfiguration<InitializerScenarios.AlwaysDropContext2>
    {
        protected override void Seed(InitializerScenarios.AlwaysDropContext2 context)
        {
            Seeder.Seed(context);
        }
    }

    public class AlwaysDropMigrationsConfig3 : DbMigrationsConfiguration<InitializerScenarios.AlwaysDropContext3>
    {
        protected override void Seed(InitializerScenarios.AlwaysDropContext3 context)
        {
            Seeder.Seed(context);
        }
    }

    public class ModelChangesMigrationsConfig1 : DbMigrationsConfiguration<InitializerScenarios.ModelChangesContext1>
    {
        protected override void Seed(InitializerScenarios.ModelChangesContext1 context)
        {
            Seeder.Seed(context);
        }
    }

    public class ModelChangesMigrationsConfig2 : DbMigrationsConfiguration<InitializerScenarios.ModelChangesContext2>
    {
        protected override void Seed(InitializerScenarios.ModelChangesContext2 context)
        {
            Seeder.Seed(context);
        }
    }

    public class ModelChangesMigrationsConfig3 : DbMigrationsConfiguration<InitializerScenarios.ModelChangesContext3>
    {
        protected override void Seed(InitializerScenarios.ModelChangesContext3 context)
        {
            Seeder.Seed(context);
        }
    }

    public class CreateFirstMigrationsConfig1 : DbMigrationsConfiguration<InitializerScenarios.CreateFirstContext1>
    {
        protected override void Seed(InitializerScenarios.CreateFirstContext1 context)
        {
            Seeder.Seed(context);
        }
    }

    public class CreateFirstMigrationsConfig2 : DbMigrationsConfiguration<InitializerScenarios.CreateFirstContext2>
    {
        protected override void Seed(InitializerScenarios.CreateFirstContext2 context)
        {
            Seeder.Seed(context);
        }
    }

    public class CreateFirstMigrationsConfig3 : DbMigrationsConfiguration<InitializerScenarios.CreateFirstContext3>
    {
        protected override void Seed(InitializerScenarios.CreateFirstContext3 context)
        {
            Seeder.Seed(context);
        }
    }

    public static class Seeder
    {
        public static void Seed(InitializerScenarios.UpDownUpContext context)
        {
            context.Entities.AddOrUpdate(
                e => e.Name,
                new InitializerScenarios.UpDownUpEntity
                {
                    Name = "Seeded",
                    Extra = "Migrations"
                });
            context.SaveChanges();
        }

        public static void InitializerSeed(InitializerScenarios.UpDownUpContext context)
        {
            context.Entities.Add(new InitializerScenarios.UpDownUpEntity { Name = "Seeded", Extra = "Initializer" });
        }
    }

    public partial class Test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UpDownUpEntities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.UpDownUpEntities");
        }
    }

    [GeneratedCode("EntityFramework.Migrations", "6.0.0")]
    public sealed partial class Test : IMigrationMetadata
    {
        string IMigrationMetadata.Id
        {
            get { return "201303090309299_Test"; }
        }

        string IMigrationMetadata.Source
        {
            get { return null; }
        }

        string IMigrationMetadata.Target
        {
            get
            {
                return
                    @"H4sIAAAAAAAEAM1Wy3LTMBTdM8M/eLSCBVGSLoCO007btEwH0nTqpnvFvkk16GEsOSTfxoJP4he4ip9RHi2FBRuPLZ17dO/R0bV+/fgZni6lCBaQGa7VgPQ6XRKAinXC1XxAcjt794Gcnrx+FV4mchk8VLgjh8NIZQbk0dr0mFITP4JkpiN5nGmjZ7YTa0lZomm/2/1Iez0KSEGQKwjCu1xZLmH9gZ8XWsWQ2pyJkU5AmHIcZ6I1a3DDJJiUxTAgiDVawFmaCh4zi+n035PgTHCGuUQgZiRgSmm7njqeGIhsptU8SnGAiftVCoibMWGgrOC4gT+3mG7fFUObwBeJQeoysdBLFMSuXHrrYgdkkg71dzVJi4k2FtGfYbUxgEO3mU4hs6s7mJUM1wkJ6GYc9QPrsFaMSwLflD3qk+AmF4JNBdSatcSNrM7gEyjImIXkllkLmXIcUOTsr+6t5Z7VarhJ6DgSjNjyC6i5fRwQfCXBFV9CUo2UGUwUR4NikM1y2FwkpI2O2+qidSzjmG6ZwGg9AkvrqVugI7Al7lzouSFBQ10YreNt0c5M6jUbT9PC1JX56R73hyOWpihK6zSUI0FUHoV30Z8bTxYcNDY7/FdnW6+EW8zm4M26U5jAFc+MHTLLpsxty0Uit2BPKlyt0xbad3aje4V270XErm7g74vH1+h4haVJ9Oq6SqhzOnz2SoooZoJlO07PhRa5VPtO4KHo4jy044uRbYaQehX4ktEtzbw+4G/AIef6kHr12sGeU8PSNc9p5p6NCggJUJoFT5yFopWxIDsO0Im+iQvBsd4GMGKKz8DYe/0VsPf0u72+9zd4QWemxiTi/27P3InwZAPe6t7P78hqwbL4kWVvJFu+bTP9bdfdbh2Hu6/fZZ9sw4WHBiSZaiymSHoDw8G8uFlvWzuk7dtMOATD5w2Fu9soiF1nakgrzLWa6WoLsOB2RhXE26ERWJagbmeZ5TMWW5yOwZj1v/OBiRwhl3IKybUa5zbN7ZkxIKdi4+cU0sPrr/9ImzmH49R9mX9RAqbJsQQYq/Oci6TO+2rbofsonIVK22NWeHdAuvmqZrrR6plEpXxDSEG5Q3MPMhVIZsYqYgvYn9vTGm4qFg45m2dMmpKjiXeXa+pu1ye/AcPcvCaPCwAA";
            }
        }
    }

    public partial class Second : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UpDownUpEntities", "Extra", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.UpDownUpEntities", "Extra");
        }
    }

    [GeneratedCode("EntityFramework.Migrations", "6.0.0")]
    public sealed partial class Second : IMigrationMetadata
    {
        string IMigrationMetadata.Id
        {
            get { return "201303101635379_Second"; }
        }

        string IMigrationMetadata.Source
        {
            get { return null; }
        }

        string IMigrationMetadata.Target
        {
            get
            {
                return
                    @"H4sIAAAAAAAEAM1XzW7bSAy+F+g7CHPqHupx3EO7gZwgjZNF0DoJqqR3WqKdwc6PVjNK7Wfbwz7SvkI51q/Hie0EPfRiSBzyI/mRHMr///tffLpUMnrEwgqjx+xoMGQR6tRkQi/GrHTz95/Y6cnbN/FFppbR90bvg9cjS23H7MG5/Jhzmz6gAjtQIi2MNXM3SI3ikBk+Gg7/5EdHHAmCEVYUxd9K7YTC9Qu9nhudYu5KkFOTobS1nE6SNWp0DQptDimOGelaI/Esz6VIwVE4o48sOpMCKJYE5ZxFoLVx66Pje4uJK4xeJDkJQN6tciS9OUiLdQbHnfqhyQxHPhneGb6KDNamSYleECFu5cNbJztm9/nE/ND3eXXQ1yXtL7jaEJDotjA5Fm71Dec1wlXGIr5px0PD1qxn44OgJ+0+jFh0XUoJM4ktZz1yE2cK/As1FuAwuwXnsNAeA6uYQ++BL//beKMiUcexaArLr6gX7mHM6JFFl2KJWSOpI7jXghqUjFxR4j4nF0tXwC/2EvOuWts1pAZ1IIiUOoLpWoJLF9Sw0k7Q1XqfpVlYFnXQVTsPgkZ4MpLWZzc5vBqdZsT4MzMWTyHPiZTezNWSKKkH7n3y8vZWFQZP7RNd3kbbeqJGggUGp37WM7wUhXUTcDADX5bzTG2p7WW48dMnOpyfjvdG2z9XFk/dOWFdAryOx0tKTdFErLPENqbdE15DJClIKJ6Y0XMjS6Wfm/Nd1tXU9e0ryeEI9Uj1IWrRNkbMAxZC2vkW78GNFRZxV/eHKq33dgqCbo/rzjtk7QStWKmwiMh5FJlvw2RlHaqBVxgk/8hzKSjfTmEKWszRujvzN9ItORoejYK99Yodwq3N5O+9SIQnYe+q2Nozh+8O/QhF+gDFOwXLP/pIL94PL0B62Q7Yvsh274Lwzt+7FKpuHLNsZiiZKugNHYH21atje0hi3v+CiydoxaKD8N9zGlN/T3agjc6VnpumBpRwP6JGJSjRFB1kxNtZ4cQcUkfHKVq73uTfQZa+imqG2ZW+KV1eujNrUc3kxqqM+W7/6/24GXN8k/s3+ytSoDAFpYA3+nMpZNbGfbnd689B+BaqB4iioi8ZglusWqRrow8EqumbYI7aj98dqlwSmL3RCTzi87Ht53CTsXgiYFGAsjVGZ+//UHD/j+LkJ5BDT8qDDAAA";
            }
        }
    }
}

namespace System.Data.Entity.Migrations.NotReady
{
    public class NotreadyMigrationsConfig : DbMigrationsConfiguration<InitializerScenarios.NotReadyContext>
    {
    }
}
