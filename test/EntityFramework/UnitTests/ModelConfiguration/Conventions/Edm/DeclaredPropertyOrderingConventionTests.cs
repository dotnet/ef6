// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public sealed class DeclaredPropertyOrderingConventionTests
    {
        class SimpleEntityBase
        {
            public string InheritedPropertyA { get; set; }
            public string InheritedPropertyB { get; set; }
        }

        class SimpleEntity : SimpleEntityBase
        {
            private string PrivateProperty { get; set; }

            public string PropertyA { get; set; }

            public string PropertyB { get; set; }

            public string Key { get; set; }

            static public string StaticProperty { get; set; }
        }

        [Fact]
        public void Apply_should_move_declared_keys_head_of_declared_properties_list()
        {
            var entityType = new EdmEntityType();
            entityType.SetClrType(typeof(SimpleEntity));
            entityType.AddPrimitiveProperty("StaticProperty");
            entityType.AddPrimitiveProperty("PrivateProperty");
            entityType.AddPrimitiveProperty("InheritedPropertyB");
            entityType.AddPrimitiveProperty("InheritedPropertyA");
            entityType.AddPrimitiveProperty("PropertyB");
            entityType.AddPrimitiveProperty("PropertyA");
            entityType.DeclaredKeyProperties.Add(
                entityType.AddPrimitiveProperty("Key"));

            ((IEdmConvention<EdmEntityType>)new DeclaredPropertyOrderingConvention())
                .Apply(entityType, new EdmModel());

            Assert.True(entityType.DeclaredProperties.Select(e => e.Name)
                .SequenceEqual(new[] { "Key", "PrivateProperty", "PropertyA", "PropertyB", "InheritedPropertyA", "InheritedPropertyB", "StaticProperty" }));
        }
    }
}