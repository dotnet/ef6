// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.DependencyResolution
{
    using Xunit;

    public class ClrTypeAnnotationSerializerTests : TestBase
    {
        [Fact]
        public void SerializeValue_returns_the_assembly_qualified_name()
        {
            Assert.Equal(
                "System.Random, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                new ClrTypeAnnotationSerializer().Serialize("Foo", typeof(Random)));
        }

        [Fact]
        public void DeserializeValue_loads_and_returns_the_specified_type()
        {
            Assert.Same(
                typeof(Random),
                new ClrTypeAnnotationSerializer().Deserialize(
                    "Foo", "System.Random, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
        }

        [Fact]
        public void DeserializeValue_returns_null_if_the_specified_type_cannot_be_loaded()
        {
            Assert.Null(
                new ClrTypeAnnotationSerializer().Deserialize(
                    "Foo", "System.NotRandom, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"));
        }
    }
}
