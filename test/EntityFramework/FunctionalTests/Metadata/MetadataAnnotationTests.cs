// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Metadata
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
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
        public void Column_annotations_are_serialized_to_and_from_XML()
        {
            var edmxBuilder = new StringBuilder();

            using (var context = new GearsOfWarContext())
            {
                //EdmxWriter.WriteEdmx(context, XmlWriter.Create(@"C:\Stuff\SSDLTest.xml"));
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
    }
}
