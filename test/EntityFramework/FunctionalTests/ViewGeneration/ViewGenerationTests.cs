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
            var hash1 = ComputeHash(GetMappingItemCollectionFromDbContext());
            var hash2 = ComputeHash(GetMappingItemCollectionFromEdmx());

            Assert.Equal(hash1, hash2);
        }

        private static string ComputeHash(StorageMappingItemCollection mappingItemCollection)
        {
            IList<EdmSchemaError> errors = new List<EdmSchemaError>();
            var viewGroups = mappingItemCollection.GenerateViews(errors);
            return viewGroups.Single().MappingHash;
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
