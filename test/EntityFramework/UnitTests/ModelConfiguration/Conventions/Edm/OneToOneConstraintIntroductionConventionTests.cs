// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class OneToOneConstraintIntroductionConventionTests
    {
        [Fact]
        public void Apply_should_introduce_constraint_when_one_to_one()
        {
            var associationType = new EdmAssociationType().Initialize();
            var entityType1 = new EdmEntityType();
            entityType1.DeclaredKeyProperties.Add(new EdmProperty().AsPrimitive());
            var entityType2 = new EdmEntityType();
            entityType2.DeclaredKeyProperties.Add(new EdmProperty().AsPrimitive());

            associationType.SourceEnd.EntityType = entityType2;
            associationType.TargetEnd.EntityType = entityType1;
            associationType.MarkPrincipalConfigured();

            ((IEdmConvention<EdmAssociationType>)new OneToOneConstraintIntroductionConvention())
                .Apply(associationType, new EdmModel());

            Assert.NotNull(associationType.Constraint);
            Assert.Equal(1, associationType.Constraint.DependentProperties.Count);
        }
    }
}