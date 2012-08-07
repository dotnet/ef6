// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System;
    using System.Data.Entity.Resources;
    using System.Runtime.Serialization;
    using System.Xml;
    using Moq;
    using Xunit;

    public class ProxyDataContractResolverTests
    {
        [Fact]
        public void ResolveName_throws_for_null_typeName_argument()
        {
            var mockDataContractResolver = new Mock<DataContractResolver>().Object;

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("typeName"),
                Assert.Throws<ArgumentException>(
                    () => new ProxyDataContractResolver().ResolveName(null, "foo", typeof(object), mockDataContractResolver)).Message);
        }

        [Fact]
        public void ResolveName_throws_for_null_typeNamespace_argument()
        {
            var mockDataContractResolver = new Mock<DataContractResolver>().Object;

            Assert.Equal(
                Strings.ArgumentIsNullOrWhitespace("typeNamespace"),
                Assert.Throws<ArgumentException>(
                    () => new ProxyDataContractResolver().ResolveName("foo", null, typeof(object), mockDataContractResolver)).Message);
        }

        [Fact]
        public void ResolveName_throws_for_null_declaredType_argument()
        {
            var mockDataContractResolver = new Mock<DataContractResolver>().Object;

            Assert.Equal(
                "declaredType",
                Assert.Throws<ArgumentNullException>(
                    () => new ProxyDataContractResolver().ResolveName("foo", "foo", null, mockDataContractResolver)).ParamName);
        }

        [Fact]
        public void ResolveName_throws_for_null_knownTypeResolver_argument()
        {
            Assert.Equal(
                "knownTypeResolver",
                Assert.Throws<ArgumentNullException>(
                    () => new ProxyDataContractResolver().ResolveName("foo", "foo", typeof(object), null)).ParamName);
        }

        [Fact]
        public void TryResolveType_throws_for_null_type_argument()
        {
            XmlDictionaryString _;
            var mockDataContractResolver = new Mock<DataContractResolver>().Object;

            Assert.Equal(
                "type",
                Assert.Throws<ArgumentNullException>(
                    () => new ProxyDataContractResolver().TryResolveType(null, typeof(object), mockDataContractResolver, out _, out _)).
                    ParamName);
        }

        [Fact]
        public void TryResolveType_throws_for_null_declaredType_argument()
        {
            XmlDictionaryString _;
            var mockDataContractResolver = new Mock<DataContractResolver>().Object;

            Assert.Equal(
                "declaredType",
                Assert.Throws<ArgumentNullException>(
                    () => new ProxyDataContractResolver().TryResolveType(typeof(object), null, mockDataContractResolver, out _, out _)).
                    ParamName);
        }

        [Fact]
        public void TryResolveType_throws_for_null_knownTypeResolver_argument()
        {
            XmlDictionaryString _;

            Assert.Equal(
                "knownTypeResolver",
                Assert.Throws<ArgumentNullException>(
                    () => new ProxyDataContractResolver().TryResolveType(typeof(object), typeof(object), null, out _, out _)).ParamName);
        }
    }
}
