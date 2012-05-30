namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="DbSet"/> and <see cref="DbSet{T}"/>.
    /// Note that some tests that would normally be unit tests are in the functional tests project because they
    /// were created before the functional/unit division.
    /// </summary> 
    public class DbSetTests : TestBase
    {
        #region Tests for nulls and incorrect types passed to non-generic DbSet API

        [Fact]
        public void Passing_null_type_to_Non_generic_Set_method_throws()
        {
            var context = new Mock<InternalContextForMock> {CallBase = true}.Object.Owner;
            Assert.Equal("entityType", Assert.Throws<ArgumentNullException>(() => context.Set(null)).ParamName);
        }

        [Fact]
        public void Passing_wrong_type_to_Non_generic_Set_Add_throws()
        {
            var set = new Mock<InternalContextForMock> { CallBase = true }.Object.Owner.Set(typeof(FakeEntity));
            Assert.Equal(Strings.DbSet_BadTypeForAddAttachRemove("Add", "String", "FakeEntity"), Assert.Throws<ArgumentException>(() => set.Add("Bang!")).Message);
        }

        [Fact]
        public void Passing_wrong_type_to_Non_generic_Set_Attach_throws()
        {
            var set = new Mock<InternalContextForMock> { CallBase = true }.Object.Owner.Set(typeof(FakeEntity));
            Assert.Equal(Strings.DbSet_BadTypeForAddAttachRemove("Attach", "String", "FakeEntity"), Assert.Throws<ArgumentException>(() => set.Attach("Bang!")).Message);
        }

        [Fact]
        public void Passing_wrong_type_to_Non_generic_Set_Remove_throws()
        {
            var set = new Mock<InternalContextForMock> { CallBase = true }.Object.Owner.Set(typeof(FakeEntity));
            Assert.Equal(Strings.DbSet_BadTypeForAddAttachRemove("Remove", "String", "FakeEntity"), Assert.Throws<ArgumentException>(() => set.Remove("Bang!")).Message);
        }

        [Fact]
        public void Passing_null_type_to_Non_generic_Set_Create_throws()
        {
            var set = new Mock<InternalContextForMock> { CallBase = true }.Object.Owner.Set(typeof(FakeEntity));
            Assert.Equal("derivedEntityType", Assert.Throws<ArgumentNullException>(() => set.Create(null)).ParamName);
        }

        [Fact]
        public void Passing_wrong_type_to_Non_generic_Set_Create_throws()
        {
            var set = new Mock<InternalContextForMock> { CallBase = true }.Object.Owner.Set(typeof(FakeEntity));
            Assert.Equal(Strings.DbSet_BadTypeForCreate("String", "FakeEntity"), Assert.Throws<ArgumentException>(() => set.Create(typeof(string))).Message);
        }

        [Fact]
        public void Passing_wrong_type_to_Non_generic_Set_Cast_throws()
        {
            var set = new Mock<InternalContextForMock> { CallBase = true }.Object.Owner.Set(typeof(FakeEntity));
            Assert.Equal(Strings.DbEntity_BadTypeForCast("DbSet", "String", "FakeEntity"), Assert.Throws<InvalidCastException>(() => set.Cast<string>()).Message);
        }

        [Fact]
        public void Passing_derived_type_to_Non_generic_Set_Cast_throws()
        {
            var set = new Mock<InternalContextForMock> { CallBase = true }.Object.Owner.Set(typeof(FakeEntity));
            Assert.Equal(Strings.DbEntity_BadTypeForCast("DbSet", "FakeDerivedEntity", "FakeEntity"), Assert.Throws<InvalidCastException>(() => set.Cast<FakeDerivedEntity>()).Message);
        }

        [Fact]
        public void Passing_base_type_to_Non_generic_Set_Cast_throws()
        {
            var set = new Mock<InternalContextForMock> { CallBase = true }.Object.Owner.Set(typeof(FakeDerivedEntity));
            Assert.Equal(Strings.DbEntity_BadTypeForCast("DbSet", "FakeEntity", "FakeDerivedEntity"), Assert.Throws<InvalidCastException>(() => set.Cast<FakeEntity>()).Message);
        }

        #endregion

        #region Tests for returning entities from Add/Attach/Remove

        [Fact]
        public void Generic_DbSet_Add_returns_the_added_entity()
        {
            var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
            var entity = new FakeEntity();

            var retVal = set.Add(entity);

            Assert.Same(entity, retVal);
        }

        [Fact]
        public void Generic_DbSet_Attach_returns_the_added_entity()
        {
            var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
            var entity = new FakeEntity();

            var retVal = set.Attach(entity);

            Assert.Same(entity, retVal);
        }

        [Fact]
        public void Generic_DbSet_Remove_returns_the_added_entity()
        {
            var set = new DbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
            var entity = new FakeEntity();

            var retVal = set.Remove(entity);

            Assert.Same(entity, retVal);
        }

        [Fact]
        public void Non_Generic_DbSet_Add_returns_the_added_entity()
        {
            var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
            var entity = new FakeEntity();

            var retVal = set.Add(entity);

            Assert.Same(entity, retVal);
        }

        [Fact]
        public void Non_Generic_DbSet_Attach_returns_the_added_entity()
        {
            var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
            var entity = new FakeEntity();

            var retVal = set.Attach(entity);

            Assert.Same(entity, retVal);
        }

        [Fact]
        public void Non_Generic_DbSet_Remove_returns_the_added_entity()
        {
            var set = new InternalDbSet<FakeEntity>(new Mock<InternalSetForMock<FakeEntity>>().Object);
            var entity = new FakeEntity();

            var retVal = set.Remove(entity);

            Assert.Same(entity, retVal);
        }

        #endregion
    }
}
