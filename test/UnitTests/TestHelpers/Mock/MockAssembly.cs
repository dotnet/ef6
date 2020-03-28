// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using Moq;

    public sealed class MockAssembly : Mock<MockAssembly.ShimAssembly>
    {
        public static implicit operator Assembly(MockAssembly mockAssembly)
        {
            return mockAssembly.Object;
        }

        public MockAssembly(params Type[] types)
        {
#if NET40
            Setup(a => a.GetTypes()).Returns(types.ToArray());
#else
            Setup(a => a.DefinedTypes).Returns(types.Select(m => m.GetTypeInfo()).ToArray());
#endif
        }

        public class ShimAssembly : Assembly
        {
            // So Moq can mock Assembly
        }
    }
}
