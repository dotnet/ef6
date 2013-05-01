// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Moq.Protected;

    public sealed class MockType : Mock<Type>
    {
        public static implicit operator Type(MockType mockType)
        {
            return mockType.Object;
        }

        private readonly List<PropertyInfo> _propertyInfos = new List<PropertyInfo>();

        public MockType()
            : this("T")
        {
        }

        public MockType(string typeName, bool hasDefaultCtor = true, string @namespace = null)
        {
            SetupGet(t => t.Name).Returns(typeName);
            SetupGet(t => t.FullName).Returns(typeName);
            SetupGet(t => t.BaseType).Returns(typeof(Object));
            SetupGet(t => t.Assembly).Returns(typeof(object).Assembly);
            Setup(t => t.GetProperties(It.IsAny<BindingFlags>())).Returns(() => _propertyInfos.ToArray());
            Setup(t => t.Equals(It.IsAny<object>())).Returns<Type>(t => ReferenceEquals(Object, t));
            Setup(t => t.ToString()).Returns(typeName);
            Setup(t => t.Namespace).Returns(@namespace);

            this.Protected()
                .Setup<PropertyInfo>(
                    "GetPropertyImpl",
                    ItExpr.IsAny<string>(),
                    ItExpr.IsAny<BindingFlags>(),
                    ItExpr.IsNull<Binder>(),
                    ItExpr.IsNull<Type>(),
                    ItExpr.IsNull<Type[]>(),
                    ItExpr.IsNull<ParameterModifier[]>())
                .Returns<string, BindingFlags, Binder, Type, Type[], ParameterModifier[]>(
                    (name, bindingAttr, binder, returnType, types, modifiers) => GetProperty(name));

            if (hasDefaultCtor)
            {
                this.Protected()
                    .Setup<ConstructorInfo>(
                        "GetConstructorImpl",
                        BindingFlags.Instance | BindingFlags.Public,
                        ItExpr.IsNull<Binder>(),
                        CallingConventions.Standard | CallingConventions.VarArgs,
                        Type.EmptyTypes,
                        ItExpr.IsNull<ParameterModifier[]>())
                    .Returns(new Mock<ConstructorInfo>().Object);
            }
        }

        public MockType TypeAttributes(TypeAttributes typeAttributes)
        {
            this.Protected()
                .Setup<TypeAttributes>("GetAttributeFlagsImpl")
                .Returns(typeAttributes);

            return this;
        }

        public MockType BaseType(MockType mockBaseType)
        {
            SetupGet(t => t.BaseType).Returns(mockBaseType);
            Setup(t => t.IsSubclassOf(mockBaseType)).Returns(true);

            return this;
        }

        public MockType Property<T>(string propertyName)
        {
            Property(typeof(T), propertyName);

            return this;
        }

        public MockType Property(Type propertyType, string propertyName)
        {
            var mockPropertyInfo = new MockPropertyInfo(propertyType, propertyName);
            mockPropertyInfo.SetupGet(p => p.DeclaringType).Returns(this);
            mockPropertyInfo.SetupGet(p => p.ReflectedType).Returns(this);

            _propertyInfos.Add(mockPropertyInfo);

            return this;
        }

        public PropertyInfo GetProperty(string name)
        {
            return _propertyInfos.SingleOrDefault(p => p.Name == name);
        }

        public MockType AsCollection()
        {
            var mockCollectionType = new MockType();

            mockCollectionType.Setup(t => t.GetInterfaces()).Returns(new[] { typeof(ICollection<>).MakeGenericType(this) });

            return mockCollectionType;
        }
    }
}
