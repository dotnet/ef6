// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Metadata
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class MetadataAnnotationsTests : FunctionalTestBase
    {
        [Fact] // CodePlex 1832
        public void Structural_annotations_are_read_into_metadata_properties()
        {
            var edmItemCollection = new EdmItemCollection(new[] { XDocument.Parse(Csdl).CreateReader() });

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

        private const string Csdl = @"
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
