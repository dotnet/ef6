// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using Xunit;

    public sealed class ForeignKeyAssociationMultiplicityConventionTests
    {
        [Fact]
        public void Apply_should_set_principal_end_kind_to_required_when_all_properties_not_nullable()
        {
            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", new EntityType());
            associationType.TargetEnd = new AssociationEndMember("T", new EntityType());

            associationType.SourceEnd.RelationshipMultiplicity = RelationshipMultiplicity.ZeroOrOne;

            var property1 = EdmProperty.Primitive("P1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            var property2 = EdmProperty.Primitive("P2", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

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

            ((IEdmConvention<AssociationType>)new ForeignKeyAssociationMultiplicityConvention())
                .Apply(associationType, new EdmModel(DataSpace.CSpace));

            Assert.True(associationType.SourceEnd.IsRequired());
        }
    }
}
