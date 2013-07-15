// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Linq;
    using Xunit;

    public class NavigationPropertyTests
    {
        [Fact]
        public void Nullability_updated_when_property_goes_readonly()
        {
            var navigationProperty 
                = new NavigationProperty("N", TypeUsage.Create(new EntityType("E", "N", DataSpace.CSpace)))
                                         {
                                             ToEndMember =
                                                 new AssociationEndMember(
                                                 "T", new RefType(new EntityType("E", "N", DataSpace.CSpace)), RelationshipMultiplicity.ZeroOrOne)
                                         };

            Assert.Equal(true, navigationProperty.TypeUsage.Facets[EdmConstants.Nullable].Value);

            navigationProperty.ToEndMember.RelationshipMultiplicity = RelationshipMultiplicity.One;

            Assert.Equal(true, navigationProperty.TypeUsage.Facets[EdmConstants.Nullable].Value);

            navigationProperty.SetReadOnly();

            Assert.Equal(false, navigationProperty.TypeUsage.Facets[EdmConstants.Nullable].Value);
        }

        [Fact]
        public static void Create_sets_properties_and_seals_the_instance()
        {
            var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var associationType = new AssociationType("AssociationType", "Namespace", true, DataSpace.CSpace);
            var source = new EntityType("Source", "Namespace", DataSpace.CSpace);
            var target = new EntityType("Target", "Namespace", DataSpace.CSpace);
            var sourceEnd = new AssociationEndMember("SourceEnd", source);
            var targetEnd = new AssociationEndMember("TargetEnd", target);            

            var navigationProperty =
                NavigationProperty.Create(
                    "NavigationProperty",
                    typeUsage,
                    associationType,
                    sourceEnd,
                    targetEnd,
                    new[]
                        {
                            new MetadataProperty(
                                "TestProperty",
                                TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)),
                                "value"),
                        });

            Assert.Equal("NavigationProperty", navigationProperty.Name);
            Assert.Same(typeUsage, navigationProperty.TypeUsage);
            Assert.Same(associationType, navigationProperty.RelationshipType);
            Assert.Same(sourceEnd, navigationProperty.FromEndMember);
            Assert.Same(targetEnd, navigationProperty.ToEndMember);
            Assert.True(navigationProperty.IsReadOnly);

            var metadataProperty = navigationProperty.MetadataProperties.SingleOrDefault(p => p.Name == "TestProperty");
            Assert.NotNull(metadataProperty);
            Assert.Equal("value", metadataProperty.Value);
        }

        [Fact]
        public static void Adding_a_NavigationProperty_to_an_EntityType_can_be_forced_when_read_only()
        {
            var typeUsage = TypeUsage.CreateDefaultTypeUsage(PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var associationType = new AssociationType("AssociationType", "Namespace", true, DataSpace.CSpace);
            var source = new EntityType("Source", "Namespace", DataSpace.CSpace);
            var target = new EntityType("Target", "Namespace", DataSpace.CSpace);
            var sourceEnd = new AssociationEndMember("SourceEnd", source);
            var targetEnd = new AssociationEndMember("TargetEnd", target);

            var navigationProperty =
                NavigationProperty.Create(
                    "NavigationProperty",
                    typeUsage,
                    associationType,
                    sourceEnd,
                    targetEnd,
                    null);

            source.SetReadOnly();
            Assert.True(source.IsReadOnly);

            Assert.Equal(
                Resources.Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => source.AddMember(navigationProperty)).Message);

            Assert.Equal(0, source.Members.Count);

            source.AddNavigationProperty(navigationProperty);

            Assert.True(source.IsReadOnly);
            Assert.Equal(1, source.Members.Count);
            Assert.Same(navigationProperty, source.Members[0]);

            Assert.Equal(
                Resources.Strings.OperationOnReadOnlyItem,
                Assert.Throws<InvalidOperationException>(
                    () => source.AddMember(navigationProperty)).Message);
        }
    }
}
