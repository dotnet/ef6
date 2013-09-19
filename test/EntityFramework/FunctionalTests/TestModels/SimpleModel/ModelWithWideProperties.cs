// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;

    public class ModelWithWideProperties : DbContext
    {
        public ModelWithWideProperties()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ModelWithWideProperties>());
        }

        public DbSet<EntityWithExplicitWideProperties> ExplicitlyWide { get; set; }
        public DbSet<EntityWithImplicitWideProperties> ImplicitlyWide { get; set; }
    }

    public class ModelWithWidePropertiesForSqlCe : DbContext
    {
        public ModelWithWidePropertiesForSqlCe()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ModelWithWidePropertiesForSqlCe>());
        }

        public DbSet<EntityWithExplicitWideProperties> ExplicitlyWide { get; set; }
        public DbSet<EntityWithImplicitWideProperties> ImplicitlyWide { get; set; }
    }

    public class EntityWithExplicitWideProperties
    {
        public int Id { get; set; }

        [MaxLength(4000)]
        public string Property1 { get; set; }

        [MaxLength(4000)]
        public string Property2 { get; set; }

        [MaxLength(4000)]
        public string Property3 { get; set; }

        [MaxLength(4000)]
        public string Property4 { get; set; }
    }

    public class EntityWithImplicitWideProperties
    {
        public int Id { get; set; }

        public string Property1 { get; set; }
        public string Property2 { get; set; }
        public string Property3 { get; set; }
        public string Property4 { get; set; }
    }
}
