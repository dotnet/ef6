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
        [Fact] // CodePlex 529
        public void Creating_database_using_initializer_does_nothing_if_Migrations_configuration_if_available()
        {
            // Make sure initializer does nothing...
            using (var context = new UpDownUpContext())
            {
                context.Database.Delete();

                context.Database.Initialize(force: true); // True not necessary here, but want to given initializer every chance to run!

                Assert.False(context.Database.Exists());
            }

            // ...but Migrations works fine
            new DbMigrator(new UpDownUpMigrationsConfig()).Update();

            using (var context = new UpDownUpContext())
            {
                var upDownUpEntity = context.Entities.Single();
                Assert.Equal("Cab", upDownUpEntity.Name);
                Assert.Equal("For Cutie", upDownUpEntity.Extra);

                var migrations = context.Database.SqlQuery<MigrationRow>("select * from __MigrationHistory").ToList();

                Assert.Equal(2, migrations.Count);
                Assert.True(migrations.All(m => m.ContextKey == typeof(UpDownUpMigrationsConfig).ToString()));
                Assert.Equal("201303090309299_Test", migrations[0].MigrationId);
                Assert.Equal("201303101635379_Second", migrations[1].MigrationId);
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

        [Fact] // CodePlex 529
        public void Creating_database_using_initializer_does_nothing_if_Migrations_configuration_is_found_but_not_ready_to_update()
        {
            using (var context = new NotReadyContext())
            {
                context.Database.Delete();

                context.Database.Initialize(force: true);

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
            context.Entities.AddOrUpdate(
                e => e.Name,
                new InitializerScenarios.UpDownUpEntity
                    {
                        Name = "Cab",
                        Extra = "For Cutie"
                    });
            context.SaveChanges();
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

    [GeneratedCode("EntityFramework.Migrations", "6.0.0-alpha3")]
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
                    @"H4sIAAAAAAAEAM1Y23LbNhB970z/gcOn9iGCJMdJ6qGScaS48bSyM6ad1wxErmRMSZAlQEf6tj70k/oLXfAmErxKsjt90Yi47J5dLPbw8J+//rY+bH3PeIJIsIDPzMlobBrAncBlfDMzY7l+9c788P7HH6xPrr81vubrztQ63MnFzHyUMrwgRDiP4FMx8pkTBSJYy5ET+IS6AZmOx7+QyYQAmjDRlmFYdzGXzIfkAR/nAXcglDH1loELnsjGccZOrBo31AcRUgdmpr0TEvzRgko6+oRW5G60ZJuISgQmTOPSYxRB2eCtTSN8ffEgwJZRwDd2iEuod78LAefX1BOQhXARvh4axXiqoiCU80AmHo/KglnEhxGmMShYSZQz8yFcBN/5Q5hOlNfi6t9gVxnAoS9REEIkd3ewzixcu6ZBqvuIvrHYVtqjQOA/Ls+mpnETex5deVDkCpNpyyCCX4EDphvcL1RKiLjaCylW3avmQ/3mXvBQsMRMY0m3vwPfyMeZiX9N44ptwc1HMs8PnGFF4iYZxVB1YpF9/upZxbqSlCFcLbVqHLZSy226xwaZrU6eGWBR7X2klTXSzkiPW7f0mQnM3K7BUDZzF3zHBL+9uBZpeeehZvX5dlB9TaZkMk7q0y9diKZsFXnpLMQKtjdt2N4cj61W2g1u+qu9uP2qhBtwkn4TWT0gggEW+u9SBVFruU/PzweVe/0i9iLU8JTDa4VzPpn+R3CSHp8j+cg4VTdjSB843TU+urEjMx7ryMbZyySjsV1ZJGW5nA1JCx1aSxqGCLREj9mIYafcOH9lH05IfmqDOKKBl4pOUXjClkA3oM2qGnPhikVCKmJeUZWLuevXlg3swLk3rRHrN3mfzHyD+p9u6nxP0Bu4Znif2SsM1kd2S+KGAl83S2cmbId6NGrg2XngxT5v4+qu3SmDlvenI3ULFtEi0HNHasnTupx+GIOOqmC6lyeMk84/wznq5rjDy+JAe23nXGGQ8nEfSnbtLsqkUPZwEBd2xpC2+Qr6dOgks3oLL9vX5wY5etGbUnvj0pcU3ovur3V5K+u4/cqo1oLTJaaBaXlirmq/5Uth/+nNPYbx7hcsKWdrEPI++APwvR4Z4p2mqI5QO0QI1/t/Sx6mktArbmpkP1zt8CcaOY80+smn25/Llk5VNHXa7VYiunDpVTZpDc1MdxVgMCnoyppEIA3TP0lX7JRATd6+fSua3suyy3NJpfM2bOfPKpVqbg6VSnWch0qlPgtHSaX8rtTFUv1tuxdArxJq9JZoodO9VYQO+lklWqfWA4413yxmGgNq/qDT7XCYVqmzk0XKX/esBQi22d8f9a2Pg6OOfG80X3PN10EeJHaPMph8iZ5ikNTF1ncZSbamjsRpB4RI5NxX6sVKPfgrcK/5bSzDWF4KAf7Kq3yysUi3/0SQVTFbt2FyL58jBITJMAS45R9j5rkF7qs6ybSZUCyQMReiQjmL5ja7wtJNwAcaytK3gBC44r178EMPjYlbbtMnaMfWn8NqxqwFo3jzfZHZ2O9XH5uJ+tr8/l+qx7AdnxYAAA==";
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

    [GeneratedCode("EntityFramework.Migrations", "6.0.0-alpha3")]
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
                    @"H4sIAAAAAAAEAM1Y23LbNhB970z/gcOn9CGCJMe5eKhkHMluPK3sjGnnNQORKxlTEmQJ0JG+rQ/9pP5CF7yJBCleZLvTF40IArtnF9g9OPznr7+tT1vfMx4hEizgM3MyGpsGcCdwGd/MzFiuX783P338+SfrwvW3xrd83omahyu5mJkPUoZnhAjnAXwqRj5zokAEazlyAp9QNyDT8fgDmUwIoAkTbRmGdRtzyXxIHvBxHnAHQhlTbxm44IlsHN/YiVXjmvogQurAzLR3QoI/WlBJRxdoRe5GS7aJqERgwjTOPUYRlA3e2jTCN2f3AmwZBXxjhziFene7EPD9mnoCshDOwjd9oxhPVRSEch7IxONRWTCL+DDCNAYFK4lyZt6Hi+AHvw/TF+W5OPs32FUGcOhrFIQQyd0trDMLV65pkOo6oi8slpXWKBD4j8uTqWlcx55HVx4UucJk2jKI4FfggOkG9yuVEiKu1kKKVfeq+VC/uRfcFDxiprGk29+Bb+TDzMS/pnHJtuDmI5nne87wROIiGcXQ5eRiKyP6zF4sst+l+t7h6ZWUYVK0DVTjsJXaDqZrbJA5XvXMAI/u3kd6fkfaSdAD1y19YQL3Z9dgKHtzG/zAbXx3diXSIspDzargXa9TPJmSyTipAr9Udk3ZKvLSetwr2N4ewvb2eGy1Ampw011TRY9RhdKAk3SbyM4DIuhhobtiK4gOHvfp6Wmv414v906EGp5yeAfhnE6m/xGchElyJJ8Zp6oy+vSBp7vGRzd2ZMaWLdk4eZlkNLYri6RcmnMuOUC61pKGIQItkXA2YtgpA89f28Npz09tEEc0sF/RKQpP2BLoBrS36oy5cMkiIRX9r6jKxdz1a9N6duDcm9aI9UreJzNfoP6ni1pvI3oD1wzvM3uJwfrIoUncUOBrvwtkJmyHejRqYPN54MU+P3QjaFud8nR5fTrS30JGwmUT2VDdhkW0LOj5J7UN0DqlvqG9trtgy5cnnSedoQznqJ0nhx+tgfYO7XSFhcr7PZQwD7soE0vZwyA+bY0hpYoK+nToSWZ1Gijb19/1cvSilVK7telTCu8Fg2hMYWVdu1vD1dp4OsU0MC2PzFUtvFwU9p/e3GMY737CknK2BiHvgj8AFQiyzHtN+x2hy4gQrvf/FmdMJaFThtUuDP11GX+kkfNAo1c+3f5StjRYew2wNExf1S8B7bpIl1GdOis9jTPTXQUYTAq6MieRa/3UWNJfWwVZk7fv34v2+bI89VzC7fQQttNnFW41N0OFWx3nUOHWZeEo4ZbXSl261e/+nQA6dVmjt0SZPd1bRXahn1WivGo94FjzzdKqMaDmj1jtDvsppzrPWaT8RdNagGCbff2o75scHLXle6P5nCu+DvIgsXuUweRT9BSDpC62vvNIsjV1JL52QIhEXH6jXqwasb8C94rfxDKM5bkQ4K+8ygcki7T7T+RhFbN1EyZ1+RwhIEyGIcAN/xwzzy1wX9bp6pAJxQIZByIqFNdobrMrLF0HvKehLH0LCIErBr0DP/TQmLjhNn2Ew9i6c1jNmLVgFCvfF5mN/Xr1gZ2oL+wf/wW49zDOkxcAAA==";
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
