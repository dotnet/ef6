namespace System.Data.Entity.Core.Objects
{
    using System;
    using Xunit;

    public class EntityEntryTests
    {
        [Fact]
        public void ApplyCurrentValues_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => new EntityEntry().ApplyCurrentValues(null));
        }

        [Fact]
        public void OriginalValues_throws_for_null_argument()
        {
            Assert.Throws<ArgumentNullException>(
                () => new EntityEntry().ApplyOriginalValues(null));
        }
    }
}
