// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class OneToOneConstraintIntroductionConventionTests
    {
        [Fact]
        public void Apply_should_introduce_constraint_when_one_to_one()
        {
            var entityType1 = new EntityType();
            entityType1.AddKeyMember(EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            var entityType2 = new EntityType();
            entityType2.AddKeyMember(EdmProperty.Primitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            var associationType = new AssociationType();
            associationType.SourceEnd = new AssociationEndMember("S", entityType2);
            associationType.TargetEnd = new AssociationEndMember("T", entityType1);

            associationType.MarkPrincipalConfigured();

            ((IEdmConvention<AssociationType>)new OneToOneConstraintIntroductionConvention())
                .Apply(associationType, new EdmModel());

            Assert.NotNull(associationType.Constraint);
            Assert.Equal(1, associationType.Constraint.ToProperties.Count);
        }
    }
}
