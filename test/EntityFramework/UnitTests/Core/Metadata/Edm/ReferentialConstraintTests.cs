// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
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
    }
}
