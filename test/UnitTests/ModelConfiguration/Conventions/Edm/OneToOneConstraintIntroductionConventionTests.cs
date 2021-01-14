// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class OneToOneConstraintIntroductionConventionTests
    {
        [Fact]
        public void Apply_should_introduce_constraint_when_one_to_one()
        {
            var entityType1 = new EntityType("E", "N", DataSpace.CSpace);
            entityType1.AddKeyMember(EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            var entityType2 = new EntityType("E", "N", DataSpace.CSpace);
            entityType2.AddKeyMember(EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String)));

            var associationType = new AssociationType("A", XmlConstants.ModelNamespace_3, false, DataSpace.CSpace);
            associationType.SourceEnd = new AssociationEndMember("S", entityType2);
            associationType.TargetEnd = new AssociationEndMember("T", entityType1);

            associationType.MarkPrincipalConfigured();

            (new OneToOneConstraintIntroductionConvention())
                .Apply(associationType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.NotNull(associationType.Constraint);
            Assert.Equal(1, associationType.Constraint.ToProperties.Count);
        }
    }
}
