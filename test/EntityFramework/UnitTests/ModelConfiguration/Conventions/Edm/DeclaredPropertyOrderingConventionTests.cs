// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class DeclaredPropertyOrderingConventionTests
    {
        public class SimpleEntityBase
        {
            public string InheritedPropertyA { get; set; }
            public string InheritedPropertyB { get; set; }
        }

        public class SimpleEntity : SimpleEntityBase
        {
            private string PrivateProperty { get; set; }
            public string PropertyA { get; set; }
            public string PropertyB { get; set; }
            public string Key { get; set; }
        }

        [Fact]
        public void Apply_should_move_declared_keys_head_of_declared_properties_list()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);
            var type = typeof(SimpleEntity);

            entityType.Annotations.SetClrType(type);

            var property1 = EdmProperty.CreatePrimitive("PrivateProperty", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property1);

            var property2 = EdmProperty.CreatePrimitive("InheritedPropertyB", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property2);

            var property3 = EdmProperty.CreatePrimitive("InheritedPropertyA", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property3);

            var property4 = EdmProperty.CreatePrimitive("PropertyB", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property4);

            var property5 = EdmProperty.CreatePrimitive("PropertyA", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property5);

            var property6 = EdmProperty.CreatePrimitive("Key", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddMember(property6);

            entityType.AddKeyMember(property6);

            new DeclaredPropertyOrderingConvention().Apply(entityType, new DbModel(new EdmModel(DataSpace.CSpace), null));

            Assert.True(
                entityType.DeclaredProperties.Select(e => e.Name)
                    .SequenceEqual(
                        new[]
                            {
                                "Key",
                                "PrivateProperty",
                                "PropertyA",
                                "PropertyB",
                                "InheritedPropertyA",
                                "InheritedPropertyB"
                            }));
        }
    }
}
