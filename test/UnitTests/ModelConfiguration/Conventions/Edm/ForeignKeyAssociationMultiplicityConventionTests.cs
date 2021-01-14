// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using Xunit;

    public sealed class ForeignKeyAssociationMultiplicityConventionTests
    {
        [Fact]
        public void Apply_should_set_principal_end_kind_to_required_when_all_properties_not_nullable()
        {
            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace));
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType("E", "N", DataSpace.CSpace));

            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;

            var property1 = EdmProperty.CreatePrimitive("P1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var property2 = EdmProperty.CreatePrimitive("P2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            var associationConstraint
                = new ReferentialConstraint(
                    associationType.SourceEnd,
                    associationType.TargetEnd,
                    new[]
                        {
                            property1,
                            property2
                        },
                    new[]
                        {
                            property1,
                            property2
                        });

            associationConstraint.ToProperties.Each(p => p.Nullable = false);

            associationType.Constraint = associationConstraint;

            (new ForeignKeyAssociationMultiplicityConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.True(associationType.SourceEnd.IsRequired());
        }
    }
}
