// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Data.Entity.Resources;
    using System.Linq;
    using Xunit;

    public class ReferentialConstraintTests
    {
        [Fact]
        public void Can_set_and_get_from_role()
        {
            var fromRole = new AssociationEndMember("P", new EntityType("E", "N", DataSpace.CSpace));

            var referentialConstraint
                = new ReferentialConstraint(
                    fromRole,
                    new AssociationEndMember("D", new EntityType("E", "N", DataSpace.CSpace)),
                    new[] { EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)) },
                    new[] { EdmProperty.CreatePrimitive("D", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)) });

            Assert.Same(fromRole, referentialConstraint.FromRole);

            var fromRole2 = new AssociationEndMember("P2", new EntityType("E", "N", DataSpace.CSpace));

            referentialConstraint.FromRole = fromRole2;

            Assert.Same(fromRole2, referentialConstraint.FromRole);
        }

        [Fact]
        public void Can_set_and_get_to_role()
        {
            var toRole = new AssociationEndMember("D", new EntityType("E", "N", DataSpace.CSpace));

            var referentialConstraint
                = new ReferentialConstraint(
                    new AssociationEndMember("P", new EntityType("E", "N", DataSpace.CSpace)),
                    toRole,
                    new[] { EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)) },
                    new[] { EdmProperty.CreatePrimitive("D", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32)) });

            Assert.Same(toRole, referentialConstraint.ToRole);

            var toRole2 = new AssociationEndMember("D2", new EntityType("E", "N", DataSpace.CSpace));

            referentialConstraint.ToRole = toRole2;

            Assert.Same(toRole2, referentialConstraint.ToRole);
        }

        [Fact]
        public void FromProperties_lazy_loaded_when_none_present()
        {
            var principalEntity = new EntityType("E", "N", DataSpace.CSpace);
            principalEntity.AddKeyMember(new EdmProperty("K"));

            var referentialConstraint
                = new ReferentialConstraint(
                    new AssociationEndMember("P", principalEntity),
                    new AssociationEndMember("D", new EntityType("E", "N", DataSpace.CSpace)),
                    Enumerable.Empty<EdmProperty>(),
                    Enumerable.Empty<EdmProperty>());

            Assert.NotEmpty(referentialConstraint.FromProperties);
        }

        [Fact]
        public void BuildConstraintExceptionMessage_returns_message_for_single_property_constraint()
        {
            var principalType = new EntityType("Principal", "N", DataSpace.CSpace);
            var principalProperty = EdmProperty.CreatePrimitive("P1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            principalType.AddMember(principalProperty);

            var dependentType = new EntityType("Dependent", "N", DataSpace.CSpace);
            var dependentProperty = EdmProperty.CreatePrimitive("D1", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32));
            dependentType.AddMember(dependentProperty);

            var referentialConstraint
                = new ReferentialConstraint(
                    new AssociationEndMember("P", principalType),
                    new AssociationEndMember("D", dependentType),
                    new[] { principalProperty },
                    new[] { dependentProperty });

            Assert.Equal(
                Strings.RelationshipManager_InconsistentReferentialConstraintProperties("Principal.P1", "Dependent.D1"),
                referentialConstraint.BuildConstraintExceptionMessage());
        }

        [Fact]
        public void BuildConstraintExceptionMessage_returns_message_for_composite_constraint()
        {
            var primitiveType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);

            var principalType = new EntityType("Principal", "N", DataSpace.CSpace);
            var principalProperties = new EdmProperty[3];

            var dependentType = new EntityType("Dependent", "N", DataSpace.CSpace);
            var dependentProperties = new EdmProperty[principalProperties.Length];

            for (var i = 0; i < principalProperties.Length; i++)
            {
                principalProperties[i] = EdmProperty.CreatePrimitive("P" + i, primitiveType);
                principalType.AddMember(principalProperties[i]);

                dependentProperties[i] = EdmProperty.CreatePrimitive("D" + i, primitiveType);
                dependentType.AddMember(dependentProperties[i]);
            }

            var referentialConstraint
                = new ReferentialConstraint(
                    new AssociationEndMember("P", principalType),
                    new AssociationEndMember("D", dependentType),
                    principalProperties,
                    dependentProperties);

            Assert.Equal(
                Strings.RelationshipManager_InconsistentReferentialConstraintProperties(
                    "Principal.P0, Principal.P1, Principal.P2", "Dependent.D0, Dependent.D1, Dependent.D2"),
                referentialConstraint.BuildConstraintExceptionMessage());
        }
    }
}
