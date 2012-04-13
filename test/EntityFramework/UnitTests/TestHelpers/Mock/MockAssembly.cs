namespace System.Data.Entity
{
    using System.Linq;
    using System.Reflection;
    using Moq;

    public sealed class MockAssembly : Mock<MockAssembly.ShimAssembly>
    {
        public static implicit operator Assembly(MockAssembly mockAssembly)
        {
            return mockAssembly.Object;
        }

        public MockAssembly(params MockType[] types)
        {
            foreach (var type in types)
            {
                type.SetupGet(t => t.Assembly).Returns(Object);
            }

            Setup(a => a.GetTypes()).Returns(types.Select(m => m.Object).ToArray());
        }

        public class ShimAssembly : Assembly
        {
            // So Moq can mock Assembly
        }
    }
}