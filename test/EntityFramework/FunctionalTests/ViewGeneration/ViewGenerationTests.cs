// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using Xunit;

    public class ViewGenerationTests : TestBase
    {
        public class Account
        {
            public int Id { get; set; }
            public Person Person { get; set; }
        }

        public class Address
        {
            public int Id { get; set; }
            public string Street { get; set; }
            public Person Person { get; set; }
            public ICollection<Itinerary> Itineraries { get; set; }
        }

        public class Itinerary
        {
            public int Id { get; set; }
            public ICollection<Address> Addresses { get; set; }
        }

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Address Address { get; set; }
        }

        public class TableSplittingContext : DbContext
        {
            static TableSplittingContext()
            {
                Database.SetInitializer<TableSplittingContext>(null);
            }

            public IDbSet<Account> Accounts { get; set; }
            public IDbSet<Person> Users { get; set; }
            public IDbSet<Address> Addresses { get; set; }
            public IDbSet<Itinerary> Itineraries { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Person>().ToTable("Users");
                modelBuilder.Entity<Address>().ToTable("Users");
                modelBuilder.Entity<Person>().HasRequired(u => u.Address).WithRequiredPrincipal(a => a.Person).WillCascadeOnDelete(true);
                base.OnModelCreating(modelBuilder);
            }
        }

        [Fact]
        public void Bug_1611_table_splitting_causing_view_gen_validation_errors()
        {
            using (var ctx = new TableSplittingContext())
            {
                var storageMappingItemCollection
                    = (StorageMappingItemCollection)((IObjectContextAdapter)ctx).ObjectContext
                        .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);

                var errors = new List<EdmSchemaError>();

                storageMappingItemCollection.GenerateViews(errors);

                Assert.Empty(errors);
            }
        }

        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<Role> Roles { get; set; }
        }

        public class Role
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<User> Users { get; set; }
        }

        public class MyContext : DbContext
        {
            public MyContext()
            {
                Database.SetInitializer<MyContext>(null);
            }

            public DbSet<User> Users { get; set; }
            public DbSet<Role> Roles { get; set; }
        }

        [Fact]
        public void MappingHash_does_not_change_if_model_saved_to_edmx_and_reloaded()
        {
            var hash1 = GetMappingItemCollectionFromDbContext().ComputeMappingHashValue();
            var hash2 = GetMappingItemCollectionFromEdmx().ComputeMappingHashValue();

            Assert.Equal(hash1, hash2);
        }

        private static StorageMappingItemCollection GetMappingItemCollectionFromDbContext()
        {
            using (var ctx = new MyContext())
            {
                return
                    (StorageMappingItemCollection)((IObjectContextAdapter)ctx).ObjectContext
                        .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
            }
        }

        private static StorageMappingItemCollection GetMappingItemCollectionFromEdmx()
        {
            var ms = new MemoryStream();

            using (var writer = XmlWriter.Create(ms))
            {
                using (var ctx = new MyContext())
                {
                    EdmxWriter.WriteEdmx(ctx, writer);
                }
            }

            ms.Position = 0;

            return GetMappingItemCollection(XDocument.Load(ms));
        }

        private static void SplitEdmx(XDocument edmx, out XmlReader csdlReader, out XmlReader ssdlReader, out XmlReader mslReader)
        {
            // xml namespace agnostic to make it work with any version of Entity Framework
            var edmxNs = edmx.Root.Name.Namespace;

            var storageModels = edmx.Descendants(edmxNs + "StorageModels").Single();
            var conceptualModels = edmx.Descendants(edmxNs + "ConceptualModels").Single();
            var mappings = edmx.Descendants(edmxNs + "Mappings").Single();

            ssdlReader = storageModels.Elements().Single(e => e.Name.LocalName == "Schema").CreateReader();
            csdlReader = conceptualModels.Elements().Single(e => e.Name.LocalName == "Schema").CreateReader();
            mslReader = mappings.Elements().Single(e => e.Name.LocalName == "Mapping").CreateReader();
        }

        private static StorageMappingItemCollection GetMappingItemCollection(XDocument edmx)
        {
            // extract csdl, ssdl and msl artifacts from the Edmx
            XmlReader csdlReader, ssdlReader, mslReader;
            SplitEdmx(edmx, out csdlReader, out ssdlReader, out mslReader);

            // Initialize item collections
            var edmItemCollection = new EdmItemCollection(new XmlReader[] { csdlReader });
            var storeItemCollection = new StoreItemCollection(new XmlReader[] { ssdlReader });
            return new StorageMappingItemCollection(edmItemCollection, storeItemCollection, new XmlReader[] { mslReader });
        }
    }
}
