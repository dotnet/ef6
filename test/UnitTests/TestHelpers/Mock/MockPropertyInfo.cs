// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Reflection;
    using Moq;

    public sealed class MockPropertyInfo : Mock<PropertyInfo>
    {
        private readonly Mock<MethodInfo> _mockGetMethod = new Mock<MethodInfo>();

        public static implicit operator PropertyInfo(MockPropertyInfo mockPropertyInfo)
        {
            return mockPropertyInfo.Object;
        }

        public MockPropertyInfo()
            : this(typeof(object), "P")
        {
        }

        public MockPropertyInfo(Type propertyType, string propertyName)
        {
            SetupGet(p => p.DeclaringType).Returns(typeof(object));
            SetupGet(p => p.ReflectedType).Returns(typeof(object));
            SetupGet(p => p.Name).Returns(propertyName);
            SetupGet(p => p.PropertyType).Returns(propertyType);
            SetupGet(p => p.CanRead).Returns(true);
            SetupGet(p => p.CanWrite).Returns(true);
            Setup(p => p.Equals(It.IsAny<object>())).Returns<PropertyInfo>(p => ReferenceEquals(Object, p));
            Setup(p => p.GetCustomAttributes(typeof(Attribute), It.IsAny<bool>())).Returns(new Attribute[0]);

#if NET40
            Setup(p => p.GetGetMethod(true)).Returns(_mockGetMethod.Object);
#else
            Setup(p => p.GetMethod).Returns(_mockGetMethod.Object);
#endif

            _mockGetMethod.SetupGet(m => m.Attributes).Returns(MethodAttributes.Public);
        }

        public MockPropertyInfo Abstract()
        {
            _mockGetMethod.SetupGet(m => m.Attributes)
                .Returns(_mockGetMethod.Object.Attributes | MethodAttributes.Abstract);

            return this;
        }
    }
}
