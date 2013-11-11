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

        public MockType()
            : this("T")
        {
        }

        public MockType(string typeName, bool hasDefaultCtor = true, string @namespace = null)
        {
            SetupGet(t => t.Name).Returns(typeName);
            SetupGet(t => t.FullName).Returns(typeName);
            Setup(t => t.Equals(It.IsAny<object>())).Returns<Type>(t => ReferenceEquals(Object, t));
            Setup(t => t.ToString()).Returns(typeName);
            Setup(t => t.Namespace).Returns(@namespace);
            Setup(t => t.GetCustomAttributes(typeof(Attribute), It.IsAny<bool>())).Returns(new Attribute[0]);
            Setup(t => t.GetCustomAttributes(typeof(FlagsAttribute), It.IsAny<bool>())).Returns(new FlagsAttribute[0]);
            Setup(t => t.IsAssignableFrom(this)).Returns(true);

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

        public MockType AsCollection()
        {
            var mockCollectionType = new MockType("ICollection[" + Object.Name + "]");

            mockCollectionType.Setup(t => t.GetInterfaces()).Returns(new[] { typeof(ICollection<>).MakeGenericType(this) });

            return mockCollectionType;
        }
    }
}
