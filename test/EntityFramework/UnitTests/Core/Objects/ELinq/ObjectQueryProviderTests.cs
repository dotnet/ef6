namespace System.Data.Entity.Core.Objects.ELinq
{
    using System;
    using System.Linq;
    using Xunit;

    public class ObjectQueryProviderTests
    {
        [Fact]
        public void CreateQuery_nongeneric_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => CreateObjectQueryProvider().CreateQuery(null));
        }

        [Fact]
        public void CreateQuery_generic_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => CreateObjectQueryProvider().CreateQuery<object>(null));
        }

        [Fact]
        public void Execute_nongeneric_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => CreateObjectQueryProvider().Execute(null));
        }

        [Fact]
        public void Execute_generic_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => CreateObjectQueryProvider().Execute<object>(null));
        }

        private IQueryProvider CreateObjectQueryProvider()
        {
            return new ObjectQueryProvider(new ObjectContext());
        }
    }
}
