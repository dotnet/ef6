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

    public class MetadataAnnotationTests
    {
        private const string CustomAnnotationNamespace = "http://schemas.microsoft.com/ado/2013/11/edm/customannotation";

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
    }
}
