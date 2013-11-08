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

        public class CodePlex1777 : TestBase
        {
#region Edmx and EF5 Views
            private readonly string Edmx =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx Version=""3.0"" xmlns:edmx=""http://schemas.microsoft.com/ado/2009/11/edmx"">
  <edmx:Runtime>
    <edmx:StorageModels>
      <Schema Namespace=""MindlinkTest2Model.Store"" Provider=""System.Data.SqlClient"" ProviderManifestToken=""2008"" Alias=""Self"" xmlns:store=""http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm/ssdl"">
        <EntityType Name=""vwRoles"">
          <Key>
            <PropertyRef Name=""Id"" />
          </Key>
          <Property Name=""Id"" Type=""uniqueidentifier"" Nullable=""false"" />
          <Property Name=""RoleName"" Type=""nvarchar"" MaxLength=""100"" Nullable=""false"" />
        </EntityType>
        <EntityType Name=""vwUserRoles"">
          <Key>
            <PropertyRef Name=""UserId"" />
            <PropertyRef Name=""RoleId"" />
          </Key>
          <Property Name=""UserId"" Type=""uniqueidentifier"" Nullable=""false"" />
          <Property Name=""RoleId"" Type=""uniqueidentifier"" Nullable=""false"" />
        </EntityType>
        <EntityType Name=""vwUsers"">
          <Key>
            <PropertyRef Name=""Id"" />
          </Key>
          <Property Name=""Id"" Type=""uniqueidentifier"" Nullable=""false"" />
          <Property Name=""Username"" Type=""nvarchar"" MaxLength=""100"" Nullable=""false"" />
          <Property Name=""FullName"" Type=""nvarchar"" MaxLength=""100"" />
          <Property Name=""EmailAddress"" Type=""nvarchar"" MaxLength=""100"" />
        </EntityType>
        <EntityContainer Name=""MindlinkTest2ModelStoreContainer"">
          <EntitySet Name=""vwRoles"" EntityType=""Self.vwRoles"" store:Type=""Views"" store:Schema=""dbo"">
            <DefiningQuery>SELECT 
    [vwRoles].[Id] AS [Id], 
    [vwRoles].[RoleName] AS [RoleName]
    FROM [dbo].[vwRoles] AS [vwRoles]</DefiningQuery>
          </EntitySet>
          <EntitySet Name=""vwUserRoles"" EntityType=""Self.vwUserRoles"" store:Type=""Views"" store:Schema=""dbo"">
            <DefiningQuery>SELECT 
    [vwUserRoles].[UserId] AS [UserId], 
    [vwUserRoles].[RoleId] AS [RoleId]
    FROM [dbo].[vwUserRoles] AS [vwUserRoles]</DefiningQuery>
          </EntitySet>
          <EntitySet Name=""vwUsers"" EntityType=""Self.vwUsers"" store:Type=""Views"" store:Schema=""dbo"">
            <DefiningQuery>SELECT 
    [vwUsers].[Id] AS [Id], 
    [vwUsers].[Username] AS [Username], 
    [vwUsers].[FullName] AS [FullName], 
    [vwUsers].[EmailAddress] AS [EmailAddress]
    FROM [dbo].[vwUsers] AS [vwUsers]</DefiningQuery>
          </EntitySet>
        </EntityContainer>
      </Schema>
    </edmx:StorageModels>
    <edmx:ConceptualModels>
      <Schema Namespace=""MindlinkTest2Model"" Alias=""Self"" annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
        <EntityType Name=""vwRole"">
          <Key>
            <PropertyRef Name=""Id"" />
          </Key>
          <Property Name=""Id"" Type=""Guid"" Nullable=""false"" />
          <Property Name=""RoleName"" Type=""String"" MaxLength=""100"" Unicode=""true"" Nullable=""false"" />
          <NavigationProperty Name=""vwUsers"" Relationship=""MindlinkTest2Model.vwUservwRole"" FromRole=""vwRole"" ToRole=""vwUser"" />
        </EntityType>
        <EntityType Name=""vwUser"">
          <Key>
            <PropertyRef Name=""Id"" />
          </Key>
          <Property Name=""Id"" Type=""Guid"" Nullable=""false"" />
          <Property Name=""Username"" Type=""String"" MaxLength=""100"" FixedLength=""false"" Unicode=""true"" Nullable=""false"" />
          <Property Name=""FullName"" Type=""String"" MaxLength=""100"" FixedLength=""false"" Unicode=""true"" />
          <Property Name=""EmailAddress"" Type=""String"" MaxLength=""100"" FixedLength=""false"" Unicode=""true"" />
          <NavigationProperty Name=""vwRoles"" Relationship=""MindlinkTest2Model.vwUservwRole"" FromRole=""vwUser"" ToRole=""vwRole"" />
        </EntityType>
        <EntityContainer Name=""Entities"" annotation:LazyLoadingEnabled=""true"">
          <EntitySet Name=""vwRoles"" EntityType=""Self.vwRole"" />
          <EntitySet Name=""vwUsers"" EntityType=""Self.vwUser"" />
          <AssociationSet Name=""vwUservwRole"" Association=""MindlinkTest2Model.vwUservwRole"">
            <End Role=""vwUser"" EntitySet=""vwUsers"" />
            <End Role=""vwRole"" EntitySet=""vwRoles"" />
          </AssociationSet>
        </EntityContainer>
        <Association Name=""vwUservwRole"">
          <End Type=""MindlinkTest2Model.vwUser"" Role=""vwUser"" Multiplicity=""*"" />
          <End Type=""MindlinkTest2Model.vwRole"" Role=""vwRole"" Multiplicity=""*"" />
        </Association>
      </Schema>
    </edmx:ConceptualModels>
    <edmx:Mappings>
      <Mapping Space=""C-S"" xmlns=""http://schemas.microsoft.com/ado/2009/11/mapping/cs"">
        <EntityContainerMapping StorageEntityContainer=""MindlinkTest2ModelStoreContainer"" CdmEntityContainer=""Entities"">
          <EntitySetMapping Name=""vwRoles"">
            <EntityTypeMapping TypeName=""MindlinkTest2Model.vwRole"">
              <MappingFragment StoreEntitySet=""vwRoles"">
                <ScalarProperty Name=""Id"" ColumnName=""Id"" />
                <ScalarProperty Name=""RoleName"" ColumnName=""RoleName"" />
              </MappingFragment>
            </EntityTypeMapping>
          </EntitySetMapping>
          <EntitySetMapping Name=""vwUsers"">
            <EntityTypeMapping TypeName=""MindlinkTest2Model.vwUser"">
              <MappingFragment StoreEntitySet=""vwUsers"">
                <ScalarProperty Name=""Id"" ColumnName=""Id"" />
                <ScalarProperty Name=""Username"" ColumnName=""Username"" />
                <ScalarProperty Name=""FullName"" ColumnName=""FullName"" />
                <ScalarProperty Name=""EmailAddress"" ColumnName=""EmailAddress"" />
              </MappingFragment>
            </EntityTypeMapping>
          </EntitySetMapping>
          <AssociationSetMapping Name=""vwUservwRole"" TypeName=""MindlinkTest2Model.vwUservwRole"" StoreEntitySet=""vwUserRoles"" >
            <EndProperty Name=""vwRole"">
              <ScalarProperty Name=""Id"" ColumnName=""RoleId"" />
            </EndProperty>
            <EndProperty Name=""vwUser"">
              <ScalarProperty Name=""Id"" ColumnName=""UserId"" />
            </EndProperty>
          </AssociationSetMapping>
        </EntityContainerMapping>
      </Mapping>
    </edmx:Mappings>
  </edmx:Runtime>
</edmx:Edmx>";

            private readonly Dictionary<string, string> EF5Views =
                new Dictionary<string, string> {
{ "MindlinkTest2ModelStoreContainer.vwRoles",
@"
SELECT VALUE -- Constructing vwRoles
    [MindlinkTest2Model.Store.vwRoles](T1.vwRoles_Id, T1.vwRoles_RoleName)
FROM (
    SELECT 
        T.Id AS vwRoles_Id, 
        T.RoleName AS vwRoles_RoleName, 
        True AS _from0
    FROM Entities.vwRoles AS T
) AS T1" 
},
{ "MindlinkTest2ModelStoreContainer.vwUsers",
@"
SELECT VALUE -- Constructing vwUsers
    [MindlinkTest2Model.Store.vwUsers](T1.vwUsers_Id, T1.vwUsers_Username, T1.vwUsers_FullName, T1.vwUsers_EmailAddress)
FROM (
    SELECT 
        T.Id AS vwUsers_Id, 
        T.Username AS vwUsers_Username, 
        T.FullName AS vwUsers_FullName, 
        T.EmailAddress AS vwUsers_EmailAddress, 
        True AS _from0
    FROM Entities.vwUsers AS T
) AS T1" 
},
{ "MindlinkTest2ModelStoreContainer.vwUserRoles",
@"
SELECT VALUE -- Constructing vwUserRoles
    [MindlinkTest2Model.Store.vwUserRoles](T1.vwUserRoles_UserId, T1.vwUserRoles_RoleId)
FROM (
    SELECT 
        Key(T.vwUser).Id AS vwUserRoles_UserId, 
        Key(T.vwRole).Id AS vwUserRoles_RoleId, 
        True AS _from0
    FROM Entities.vwUservwRole AS T
) AS T1" 
},
{ "Entities.vwRoles",
@"
SELECT VALUE -- Constructing vwRoles
    [MindlinkTest2Model.vwRole](T1.vwRole_Id, T1.vwRole_RoleName)
FROM (
    SELECT 
        T.Id AS vwRole_Id, 
        T.RoleName AS vwRole_RoleName, 
        True AS _from0
    FROM MindlinkTest2ModelStoreContainer.vwRoles AS T
) AS T1" 
},
{ "Entities.vwUsers",
@"
SELECT VALUE -- Constructing vwUsers
    [MindlinkTest2Model.vwUser](T1.vwUser_Id, T1.vwUser_Username, T1.vwUser_FullName, T1.vwUser_EmailAddress)
FROM (
    SELECT 
        T.Id AS vwUser_Id, 
        T.Username AS vwUser_Username, 
        T.FullName AS vwUser_FullName, 
        T.EmailAddress AS vwUser_EmailAddress, 
        True AS _from0
    FROM MindlinkTest2ModelStoreContainer.vwUsers AS T
) AS T1"
},
{ "Entities.vwUservwRole",
@"
SELECT VALUE -- Constructing vwUservwRole
    [MindlinkTest2Model.vwUservwRole](T3.vwUservwRole_vwUser, T3.vwUservwRole_vwRole)
FROM (
    SELECT -- Constructing vwUser
        CreateRef(Entities.vwUsers, row(T2.vwUservwRole_vwUser_Id), [MindlinkTest2Model.vwUser]) AS vwUservwRole_vwUser, 
        T2.vwUservwRole_vwRole
    FROM (
        SELECT -- Constructing vwRole
            T1.vwUservwRole_vwUser_Id, 
            CreateRef(Entities.vwRoles, row(T1.vwUservwRole_vwRole_Id), [MindlinkTest2Model.vwRole]) AS vwUservwRole_vwRole
        FROM (
            SELECT 
                T.UserId AS vwUservwRole_vwUser_Id, 
                T.RoleId AS vwUservwRole_vwRole_Id, 
                True AS _from0
            FROM MindlinkTest2ModelStoreContainer.vwUserRoles AS T
        ) AS T1
    ) AS T2
) AS T3"
}};
#endregion

            [Fact]
            public void EF6_generates_same_views_as_EF5()
            {
                var mappingItemCollection = GetMappingItemCollection(XDocument.Parse(Edmx));

                var errors = new List<EdmSchemaError>();
                var views = mappingItemCollection.GenerateViews(errors);

                Assert.Empty(errors);
                Assert.Equal(EF5Views.Count, views.Count);

                foreach (var view in views)
                {
                    var ef5ViewKey = view.Key.EntityContainer.Name + "." + view.Key.Name;
                    Assert.Equal(
                        RemoveWhiteSpace(EF5Views[ef5ViewKey]),
                        RemoveWhiteSpace(view.Value.EntitySql));
                }
            }

            private static string RemoveWhiteSpace(string str)
            {
                return new string(str.Where(c => !Char.IsWhiteSpace(c)).ToArray());
            }
        }
    }
}
