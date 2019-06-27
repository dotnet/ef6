// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Metadata
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.TestHelpers;
    using System.Data.Entity.TestModels.GearsOfWarModel;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Xunit;

    public class MetadataAnnotationTests : FunctionalTestBase
    {
        private const string CustomAnnotationNamespace = "http://schemas.microsoft.com/ado/2013/11/edm/customannotation";

        [Fact]
        public void ClrType_annotations_are_serialized_to_and_from_XML()
        {
            var edmxBuilder = new StringBuilder();

            using (var context = new GearsOfWarContext())
            {
                EdmxWriter.WriteEdmx(context, XmlWriter.Create(edmxBuilder));
            }

            var edmItemCollection = LoadEdmItemCollection(edmxBuilder.ToString());

            Assert.Equal(
                "true",
                edmItemCollection
                    .GetEntityContainer("GearsOfWarContext")
                    .MetadataProperties
                    .Single(p => p.Name == CustomAnnotationNamespace + ":UseClrTypes")
                    .Value);

            var entityTypes = edmItemCollection.GetItems<EntityType>().ToList();

            Assert.Same(typeof(City), GetClrType(entityTypes.Single(e => e.Name == "City")));
            Assert.Same(typeof(Gear), GetClrType(entityTypes.Single(e => e.Name == "Gear")));
            Assert.Same(typeof(Squad), GetClrType(entityTypes.Single(e => e.Name == "Squad")));
            Assert.Same(typeof(CogTag), GetClrType(entityTypes.Single(e => e.Name == "CogTag")));
            Assert.Same(typeof(Weapon), GetClrType(entityTypes.Single(e => e.Name == "Weapon")));
            Assert.Same(typeof(HeavyWeapon), GetClrType(entityTypes.Single(e => e.Name == "HeavyWeapon")));
            Assert.Same(typeof(StandardWeapon), GetClrType(entityTypes.Single(e => e.Name == "StandardWeapon")));

            var complexTypes = edmItemCollection.GetItems<ComplexType>().ToList();
            Assert.Same(typeof(WeaponSpecification), GetClrType(complexTypes.Single(e => e.Name == "WeaponSpecification")));

            var enumTypes = edmItemCollection.GetItems<EnumType>().ToList();
            Assert.Same(typeof(MilitaryRank), GetClrType(enumTypes.Single(e => e.Name == "MilitaryRank")));
        }

        private static Type GetClrType(EdmType type)
        {
            return (Type)type.MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":ClrType").Value;
        }

        private static EdmItemCollection LoadEdmItemCollection(string edmx)
        {
            return
                new EdmItemCollection(
                    new[]
                    {
                        XDocument.Parse(edmx)
                            .Descendants(XName.Get("Schema", "http://schemas.microsoft.com/ado/2009/11/edm"))
                            .Single()
                            .CreateReader()
                    });
        }

        [Fact]
        public void Column_and_table_annotations_are_serialized_to_and_from_XML()
        {
            var edmxBuilder = new StringBuilder();

            using (var context = new GearsOfWarContext())
            {
                EdmxWriter.WriteEdmx(context, XmlWriter.Create(edmxBuilder));
            }

            var storeItemCollection = LoadStoreItemCollection(edmxBuilder.ToString());

            var entityTypes = storeItemCollection.GetItems<EntityType>().ToList();

            Assert.Equal(
                "Love not war!",
                entityTypes.Single(e => e.Name == "Gear")
                    .Properties.Single(p => p.Name == "Rank")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_Rank")
                    .Value);

            Assert.Equal(
                "All you need is love...",
                entityTypes.Single(e => e.Name == "Squad")
                    .Properties.Single(p => p.Name == "Id")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_Id")
                    .Value);

            Assert.Equal(
                "She loves me, yeah, yeah, yeah.",
                entityTypes.Single(e => e.Name == "Squad")
                    .Properties.Single(p => p.Name == "InternalNumber")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_InternalNumber")
                    .Value);

            Assert.Equal(
                "...living life in peace.",
                entityTypes.Single(e => e.Name == "CogTag")
                    .Properties.Single(p => p.Name == "Note")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_Note")
                    .Value);

            Assert.Equal(
                "Let It Be",
                entityTypes.Single(e => e.Name == "Weapon")
                    .Properties.Single(p => p.Name == "Specs_AmmoPerClip")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_AmmoPerClip")
                    .Value);

            Assert.Equal(
                "Step to West 17",
                entityTypes.Single(e => e.Name == "Gear")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_Gear")
                    .Value);

            Assert.Equal(
                "The Long Earth",
                entityTypes.Single(e => e.Name == "City")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_City1")
                    .Value);

            Assert.Equal(
                "Natural Stepper",
                entityTypes.Single(e => e.Name == "City")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_City3")
                    .Value);

            Assert.False(
                entityTypes.Single(e => e.Name == "City")
                    .MetadataProperties.Any(p => p.Name == CustomAnnotationNamespace + ":Annotation_City2"));

            Assert.Equal(
                "Happy Place",
                entityTypes.Single(e => e.Name == "Squad")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_Squad1")
                    .Value);

            Assert.Equal(
                "Happy Planet",
                entityTypes.Single(e => e.Name == "Squad")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_Squad2")
                    .Value);

            Assert.Equal(
                "It's an elf!",
                entityTypes.Single(e => e.Name == "CogTag")
                    .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Annotation_CogTag")
                    .Value);
        }

        [Fact]
        public void Index_annotations_are_serialized_to_and_from_XML()
        {
            var edmxBuilder = new StringBuilder();

            using (var context = new TeaSplitPeaHContext())
            {
                EdmxWriter.WriteEdmx(context, XmlWriter.Create(edmxBuilder));
            }

            var entityTypes = LoadStoreItemCollection(edmxBuilder.ToString()).GetItems<EntityType>().ToList();

            CheckIndex(entityTypes, "TeaPeaHBase", "Id", new IndexAnnotation(new IndexAttribute()));
            CheckIndex(entityTypes, "TeaPeaHBase", "Carbs1_Mmmm", new IndexAnnotation(new IndexAttribute("C1")));
            CheckIndex(entityTypes, "TeaPeaHBase", "Carbs1_MoreCarbs_Ahhh", new IndexAnnotation(new IndexAttribute("C2")));
            CheckIndex(entityTypes, "TeaPeaHBase", "Carbs2_Mmmm", new IndexAnnotation(new IndexAttribute("C1")));
            CheckIndex(entityTypes, "TeaPeaHBase", "Carbs2_MoreCarbs_Ahhh", new IndexAnnotation(new IndexAttribute("C2")));
            CheckIndex(
                entityTypes, "TeaPeaHBase", "Shared", new IndexAnnotation(
                    new[]
                    {
                        new IndexAttribute("I0", 6) { IsClustered = true, IsUnique = true },
                        new IndexAttribute("I1", 6) { IsClustered = false, IsUnique = false },
                        new IndexAttribute("I2", 7) { IsClustered = true, IsUnique = true },
                        new IndexAttribute("I3", 8) { IsClustered = false, IsUnique = true },
                        new IndexAttribute("I4", 9) { IsClustered = true, IsUnique = false }
                    }));
            CheckIndex(
                entityTypes, "SplitPea", "Id", new IndexAnnotation(
                    new[]
                    {
                        new IndexAttribute("I0", 6) { IsClustered = true, IsUnique = true },
                        new IndexAttribute("I3", 8) { IsClustered = false, IsUnique = true },
                        new IndexAttribute("I4", 9) { IsClustered = true, IsUnique = false }
                    }));
            CheckIndex(entityTypes, "SplitPea", "Prop1", new IndexAnnotation(new IndexAttribute("I1") { IsClustered = false }));
            CheckIndex(entityTypes, "SplitPea", "Carbs1_Mmmm", new IndexAnnotation(new[] { new IndexAttribute("C1") }));
            CheckIndex(entityTypes, "SplitPea", "Carbs1_MoreCarbs_Ahhh", new IndexAnnotation(new IndexAttribute("C2")));
            CheckIndex(
                entityTypes, "SplitPea1", "Id", new IndexAnnotation(
                    new[]
                    {
                        new IndexAttribute("I0", 6) { IsClustered = true, IsUnique = true },
                        new IndexAttribute("I3", 8) { IsClustered = false, IsUnique = true },
                        new IndexAttribute("I4", 9) { IsClustered = true, IsUnique = false }
                    }));
            CheckIndex(entityTypes, "SplitPea1", "Prop2", new IndexAnnotation(new IndexAttribute("I2") { IsUnique = true }));
            CheckIndex(entityTypes, "SplitPea1", "Carbs2_Mmmm", new IndexAnnotation(new IndexAttribute("C1")));
            CheckIndex(entityTypes, "SplitPea1", "Carbs2_MoreCarbs_Ahhh", new IndexAnnotation(new IndexAttribute("C2")));
        }

        private static void CheckIndex(
            IEnumerable<EntityType> entityTypes, string storeTypeName, string columnName, IndexAnnotation expected)
        {
            var annotation = entityTypes.Single(e => e.Name == storeTypeName)
                .Properties.Single(p => p.Name == columnName)
                .MetadataProperties.Single(p => p.Name == CustomAnnotationNamespace + ":Index")
                .Value;

            Assert.IsType<IndexAnnotation>(annotation);
            Assert.Equal(expected, annotation, new IndexAnnotationEqualityComparer());
        }

        public class TeaSplitPeaHContext : DbContext
        {
            static TeaSplitPeaHContext()
            {
                Database.SetInitializer<TeaSplitPeaHContext>(null);
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TeaPeaHBase>();
                modelBuilder.Entity<TeaPeaH1>().Property(e => e.Prop1).HasColumnName("Shared");
                modelBuilder.Entity<TeaPeaH2>().Property(e => e.Prop2).HasColumnName("Shared");

                modelBuilder.Entity<SplitPea>()
                    .Map(m => m.ToTable("One").Properties(p => new { p.Id, p.Prop1, p.Carbs1 }))
                    .Map(m => m.ToTable("Two").Properties(p => new { p.Id, p.Prop2, p.Carbs2 }));
            }
        }

        public class TeaPeaHBase
        {
            [Index]
            public int Id { get; set; }

            public AllCarbs Carbs1 { get; set; }
            public AllCarbs Carbs2 { get; set; }
        }

        public class TeaPeaH1 : TeaPeaHBase
        {
            [Index("I1", 6, IsClustered = false, IsUnique = false)]
            [Index("I0")]
            [Index("I0", Order = 6)]
            [Index("I0", IsUnique = true)]
            [Index("I0", Order = 6, IsClustered = true)]
            [Index("I0", Order = 6, IsUnique = true)]
            [Index("I0", Order = 6, IsClustered = true, IsUnique = true)]
            [Index("I2", 7, IsClustered = true, IsUnique = true)]
            public int Prop1 { get; set; }
        }

        public class TeaPeaH2 : TeaPeaHBase
        {
            [Index("I3", 8, IsClustered = false, IsUnique = true)]
            [Index("I0", 6)]
            [Index("I0", IsClustered = true)]
            [Index("I0", 6, IsClustered = true)]
            [Index("I0", 6, IsUnique = true)]
            [Index("I0", 6, IsClustered = true, IsUnique = true)]
            [Index("I4", 9, IsClustered = true, IsUnique = false)]
            public int Prop2 { get; set; }
        }

        public class SplitPea
        {
            [Index("I3", 8, IsClustered = false, IsUnique = true)]
            [Index("I0", 6)]
            [Index("I0", IsClustered = true)]
            [Index("I0", 6, IsClustered = true)]
            [Index("I0", 6, IsUnique = true)]
            [Index("I0", 6, IsClustered = true, IsUnique = true)]
            [Index("I4", 9, IsClustered = true, IsUnique = false)]
            public int Id { get; set; }

            [Index("I1", IsClustered = false)]
            public int Prop1 { get; set; }

            [Index("I2", IsUnique = true)]
            public int Prop2 { get; set; }

            public AllCarbs Carbs1 { get; set; }
            public AllCarbs Carbs2 { get; set; }
        }

        [ComplexType]
        public class AllCarbs
        {
            [Index("C1")]
            public int Mmmm { get; set; } 
            public ComplexCarbs MoreCarbs { get; set; } 
        }

        [ComplexType]
        public class ComplexCarbs
        {
            [Index("C2")]
            public int Ahhh { get; set; } 
        }

        private static StoreItemCollection LoadStoreItemCollection(string edmx)
        {
            return
                new StoreItemCollection(
                    new[]
                    {
                        XDocument.Parse(edmx)
                            .Descendants(XName.Get("Schema", "http://schemas.microsoft.com/ado/2009/11/edm/ssdl"))
                            .Single()
                            .CreateReader()
                    });
        }

        [Fact] // CodePlex 1832
        public void Structural_annotations_are_read_into_metadata_properties()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(CsdlWithStructs).CreateReader() });

            var entityType = edmItemCollection.OfType<EntityType>().Single(e => e.Name == "NorwegianAnimal");
            var entityAnnotation = entityType.MetadataProperties.Single(p => p.Name == "FoxAnnotations:TheSecretOfTheFox");

            var element = ((XElement)entityAnnotation.Value);
            Assert.Equal("FoxAnnotations", element.Name.Namespace);
            Assert.Equal("TheSecretOfTheFox", element.Name.LocalName);

            var innerElement = element.Elements().Single();
            Assert.Equal("", innerElement.Name.Namespace);
            Assert.Equal("Secret", innerElement.Name.LocalName);

            Assert.Equal("Ancient Mystery", innerElement.Attributes().Single(e => e.Name.LocalName == "Name").Value);

            var property = entityType.Properties.Single(p => p.Name == "WhatDoesItSay");
            var propertyAnnotation = property.MetadataProperties.Single(p => p.Name == "FoxAnnotations:TheSecretOfTheFox");

            element = ((XElement)propertyAnnotation.Value);
            Assert.Equal("FoxAnnotations", element.Name.Namespace);
            Assert.Equal("TheSecretOfTheFox", element.Name.LocalName);

            Assert.Equal(2, element.Elements().Count());

            Assert.True(element.Elements().All(e => e.Name.Namespace == "FoxAnnotations"));
            Assert.True(element.Elements().All(e => e.Name.LocalName == "Option"));

            Assert.Equal(
                new[] { "Hattie Hattie Hattie Ho", "Wa-pa-pa-pa-pa-pow!" },
                element.Elements().Attributes().Where(e => e.Name.LocalName == "Name").Select(a => a.Value));
        }

        private const string CsdlWithStructs = @"
            <Schema Namespace=""Investigate1833"" Alias=""Self"" annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns:customannotation=""http://schemas.microsoft.com/ado/2013/11/edm/customannotation"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
              <EntityType Name=""NorwegianAnimal"" customannotation:ClrType=""Investigate1833.NorwegianAnimal, Investigate1833, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"">
                <Key>
                  <PropertyRef Name=""Id"" />
                </Key>
                <Property Name=""Id"" Type=""Int32"" Nullable=""false"" annotation:StoreGeneratedPattern=""Identity"" />
                <Property Name=""WhatDoesItSay"" Type=""String"" MaxLength=""Max"" FixedLength=""false"" Unicode=""true"">
                  <TheSecretOfTheFox xmlns=""FoxAnnotations"">
                    <Option Name=""Hattie Hattie Hattie Ho"" xmlns=""FoxAnnotations"" />
                    <Option Name=""Wa-pa-pa-pa-pa-pow!"" xmlns=""FoxAnnotations"" />
                  </TheSecretOfTheFox>
                </Property>
                <TheSecretOfTheFox xmlns=""FoxAnnotations"">
                  <Secret Name=""Ancient Mystery"" xmlns="""" />
                </TheSecretOfTheFox>
              </EntityType>
              <EntityContainer Name=""FoxContext"" customannotation:UseClrTypes=""true"">
                <EntitySet Name=""Animals"" EntityType=""Self.NorwegianAnimal"" />
              </EntityContainer>
            </Schema>";


#if NET452
        [Fact] // CodePlex 2051
        public void Can_load_model_after_assembly_version_of_types_changes()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(CsdlWithVersion).CreateReader() });

            Assert.Null(GetClrType(edmItemCollection.GetItems<EntityType>().Single(e => e.Name == "Man")));
        }
#endif

        public class Man
        {
            public int Id { get; set; }
        }

        private const string CsdlWithVersion = @"
            <Schema Namespace=""Half"" Alias=""Self"" annotation:UseStrongSpatialTypes=""false"" xmlns:annotation=""http://schemas.microsoft.com/ado/2009/02/edm/annotation"" xmlns:customannotation=""http://schemas.microsoft.com/ado/2013/11/edm/customannotation"" xmlns=""http://schemas.microsoft.com/ado/2009/11/edm"">
              <EntityType Name=""Man"" customannotation:ClrType=""System.Data.Entity.Metadata.MetadataAnnotationTests+Man, EntityFramework.FunctionalTests, Version=0.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"">
                <Key>
                  <PropertyRef Name=""Id"" />
                </Key>
                <Property Name=""Id"" Type=""Int32"" Nullable=""false"" annotation:StoreGeneratedPattern=""Identity"" />
              </EntityType>
              <EntityContainer Name=""HalfManContext"" customannotation:UseClrTypes=""true"">
                <EntitySet Name=""HalfMen"" EntityType=""Self.Man"" />
              </EntityContainer>
            </Schema>";
    }
}
