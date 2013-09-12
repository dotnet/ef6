// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for the DbSet/ObjectSet discovery service.
    /// </summary>
    public class DbSetDiscoveryServiceTests : TestBase
    {
        [Fact]
        public void MethodInfo_fields_are_initialized()
        {
            Assert.NotNull(DbSetDiscoveryService.SetMethod);
        }

        #region Positive DbContext discovery and initialization tests

        public class FakeEntity1
        {
        }

        public class FakeEntity2
        {
        }

        public class FakeEntity3
        {
        }

        public class FakeEntity4
        {
        }

        public class FakeEntity5
        {
        }

        public class FakeEntity6
        {
        }

        public class FakeEntity7
        {
        }

        public class FakeEntity8
        {
        }

        public class FakeEntity9
        {
        }

        public class FakeEntity10
        {
        }

        public class FakeEntity11
        {
        }

        public class FakeEntity12
        {
        }

        public class FakeEntity13
        {
        }

        public class FakeEntity14
        {
        }

        public class FakeEntity15
        {
        }

        public class FakeEntity16
        {
        }

        public class FakeEntity17
        {
        }

        public class FakeEntity18
        {
        }

        private class FakeDbContextWithDbSets : DbContext
        {
            public FakeDbContextWithDbSets()
                : base("this=is=not=valid")
            {
            }

            // Should be detected: DbSets with no modifiers
            public DbSet<FakeEntity1> PublicGetSet { get; set; }
            protected IDbSet<FakeEntity2> ProtectedGetSet { get; set; }
            internal DbSet<FakeEntity3> InternalGetSet { get; set; }
            protected internal IDbSet<FakeEntity4> InternalProtectedGetSet { get; set; }
            private DbSet<FakeEntity5> PrivateGetSet { get; set; }

            // Should be detected: Public DbSets setter modifiers
            public IDbSet<FakeEntity6> PrivateSet { get; private set; }
            public DbSet<FakeEntity7> ProtectedSet { get; protected set; }
            public IDbSet<FakeEntity8> InternalSet { get; internal set; }
            public DbSet<FakeEntity9> InternalProtectedSet { get; protected internal set; }

            // Should be detected: Public DbSets getter modifiers
            public DbSet<FakeEntity10> PrivateGet { private get; set; }
            public IDbSet<FakeEntity11> ProtectedGet { protected get; set; }
            public DbSet<FakeEntity12> InternalGet { internal get; set; }
            public IDbSet<FakeEntity13> InternalProtectedGet { protected internal get; set; }

            // Should be detected: DbSets with no setters
            public DbSet<FakeEntity14> PublicGetNoSet
            {
                get { return null; }
            }

            protected IDbSet<FakeEntity15> ProtectedGetNoSet
            {
                get { return null; }
            }

            internal IDbSet<FakeEntity16> InternalGetNoSet
            {
                get { return null; }
            }

            protected internal DbSet<FakeEntity17> InternalProtectedGetNoSet
            {
                get { return null; }
            }

            private IDbSet<FakeEntity18> PrivateGetNoSet
            {
                get { return null; }
            }
        }

        [Fact]
        public void DbSet_and_IDbSet_properties_on_derived_DbContext_are_discovered()
        {
            var expected = new[]
                               {
                                   "PublicGetSet", "ProtectedGetSet", "InternalGetSet", "InternalProtectedGetSet", "PrivateGetSet",
                                   "PrivateSet", "ProtectedSet", "InternalSet", "InternalProtectedSet",
                                   "PrivateGet", "ProtectedGet", "InternalGet", "InternalProtectedGet",
                                   "PublicGetNoSet", "ProtectedGetNoSet", "InternalGetNoSet", "InternalProtectedGetNoSet", "PrivateGetNoSet"
                                   ,
                               };

            AssertExpectedSetsDiscovered(new FakeDbContextWithDbSets(), expected);
        }

        private class FakeDbContextWithNonSets : DbContext
        {
            public FakeDbContextWithNonSets()
                : base("this=is=not=valid")
            {
            }

            public DbSet<FakeEntity> Control { get; set; }

            public IQueryable<FakeEntity> QueryableT { get; set; }
            public IQueryable Queryable { get; set; }
            public IEnumerable<FakeEntity> EnumerableT { get; set; }
            public IEnumerable Enumerable { get; set; }
            public DbQuery<FakeEntity> DbQuery { get; set; }
        }

        [Fact]
        public void IQueryable_and_DbQuery_properties_on_derived_DbContext_are_ignored()
        {
            AssertExpectedSetsDiscovered(new FakeDbContextWithNonSets(), new[] { "Control" });
        }

        [Fact]
        public void DbSet_and_IDbSet_properties_with_public_setters_are_initialized()
        {
            using (var context = new FakeDbContextWithDbSets())
            {
                var contextType = typeof(FakeDbContextWithDbSets);

                Assert.NotNull(contextType.GetDeclaredProperty("PublicGetSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("ProtectedGetSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("InternalGetSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("InternalProtectedGetSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("PrivateGetSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("PrivateSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("ProtectedSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("InternalSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("InternalProtectedSet").GetValue(context, null));
                Assert.NotNull(contextType.GetDeclaredProperty("PrivateGet").GetValue(context, null));
                Assert.NotNull(contextType.GetDeclaredProperty("ProtectedGet").GetValue(context, null));
                Assert.NotNull(contextType.GetDeclaredProperty("InternalGet").GetValue(context, null));
                Assert.NotNull(contextType.GetDeclaredProperty("InternalProtectedGet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("PublicGetNoSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("ProtectedGetNoSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("InternalGetNoSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("InternalProtectedGetNoSet").GetValue(context, null));
                Assert.Null(contextType.GetDeclaredProperty("PrivateGetNoSet").GetValue(context, null));
            }
        }

        [Fact]
        public void IQueryable_and_DbQuery_properties_with_public_setters_are_not_initialized()
        {
            using (var context = new FakeDbContextWithNonSets())
            {
                Assert.NotNull(context.Control);
                Assert.Null(context.DbQuery);
                Assert.Null(context.Enumerable);
                Assert.Null(context.EnumerableT);
                Assert.Null(context.Queryable);
                Assert.Null(context.QueryableT);
            }
        }

        #endregion

        #region Negative entity types

        private class FakeDbContextWithInterfaceDbSet : DbContext
        {
            public DbSet<IList> SetProp { get; set; }
        }

        [Fact]
        public void Discovery_of_DbSet_of_interface_type_throws()
        {
            Assert.Equal(
                Strings.InvalidEntityType(typeof(IList)),
                Assert.Throws<InvalidOperationException>(
                    () => new DbSetDiscoveryService(new FakeDbContextWithInterfaceDbSet()).InitializeSets()).Message);
        }

        private class FakeDbContextWithGenericDbSet : DbContext
        {
            public DbSet<List<FakeEntity>> SetProp { get; set; }
        }

        [Fact]
        public void Discovery_of_DbSet_of_generic_type_throws()
        {
            Assert.Equal(
                Strings.InvalidEntityType(typeof(List<FakeEntity>)),
                Assert.Throws<InvalidOperationException>(
                    () => new DbSetDiscoveryService(new FakeDbContextWithGenericDbSet()).InitializeSets()).Message);
        }

        private class FakeDbContextWithObjectDbSet : DbContext
        {
            public DbSet<object> SetProp { get; set; }
        }

        [Fact]
        public void Discovery_of_DbSet_of_object_throws()
        {
            Assert.Equal(
                Strings.InvalidEntityType(typeof(Object)),
                Assert.Throws<InvalidOperationException>(
                    () => new DbSetDiscoveryService(new FakeDbContextWithObjectDbSet()).InitializeSets()).Message);
        }

        private class FakeDbContextWithEntityObjectDbSet : DbContext
        {
            public DbSet<EntityObject> SetProp { get; set; }
        }

        [Fact]
        public void Discovery_of_DbSet_of_EntityObject_throws()
        {
            Assert.Equal(
                Strings.InvalidEntityType(typeof(EntityObject)),
                Assert.Throws<InvalidOperationException>(
                    () => new DbSetDiscoveryService(new FakeDbContextWithEntityObjectDbSet()).InitializeSets()).Message);
        }

        private class FakeDbContextWithStructuralObjectDbSet : DbContext
        {
            public DbSet<StructuralObject> SetProp { get; set; }
        }

        [Fact]
        public void Discovery_of_DbSet_of_StructuralObject_throws()
        {
            Assert.Equal(
                Strings.InvalidEntityType(typeof(StructuralObject)),
                Assert.Throws<InvalidOperationException>(
                    () => new DbSetDiscoveryService(new FakeDbContextWithStructuralObjectDbSet()).InitializeSets()).Message);
        }

        private class FakeDbContextWithComplexObjectDbSet : DbContext
        {
            public DbSet<ComplexObject> SetProp { get; set; }
        }

        [Fact]
        public void Discovery_of_DbSet_of_ComplexObject_throws()
        {
            Assert.Equal(
                Strings.InvalidEntityType(typeof(ComplexObject)),
                Assert.Throws<InvalidOperationException>(
                    () => new DbSetDiscoveryService(new FakeDbContextWithComplexObjectDbSet()).InitializeSets()).Message);
        }

        private class FakeDbContextWithStringDbSet : DbContext
        {
            public DbSet<String> SetProp { get; set; }
        }

        [Fact]
        public void Discovery_of_DbSet_of_string_throws()
        {
            Assert.Equal(
                Strings.InvalidEntityType(typeof(String)),
                Assert.Throws<InvalidOperationException>(
                    () => new DbSetDiscoveryService(new FakeDbContextWithStringDbSet()).InitializeSets()).Message);
        }

        #endregion

        #region Positive tests for discovery attributes at the class level

        [SuppressDbSetInitialization]
        private class DerivedDbContextWithClassLevelSetDiscoverOnly : DbContext
        {
            public DbSet<DBNull> FakeSet1 { get; set; }
            public DbSet<Random> FakeSet2 { get; set; }
        }

        [Fact]
        public void SuppressDbSetInitializationAttribute_on_context_class_results_in_all_sets_being_found()
        {
            var context = new DerivedDbContextWithClassLevelSetDiscoverOnly();
            DiscoverAndInitializeSets(context, 2);

            Assert.Null(context.FakeSet1);
            Assert.Null(context.FakeSet2);
        }

        #endregion

        #region Positive tests for discovery attributes at the property level

        private class DerivedDbContextWithPropertyLevelSetDiscoverOnly : DbContext
        {
            public DbSet<DBNull> FakeSet1 { get; set; }

            [SuppressDbSetInitialization]
            public DbSet<Random> FakeSet2 { get; set; }
        }

        [Fact]
        public void SuppressDbSetInitializationAttribute_on_property_results_in_property_still_being_discovered()
        {
            var context = new DerivedDbContextWithPropertyLevelSetDiscoverOnly();
            DiscoverAndInitializeSets(context, 2);

            Assert.NotNull(context.FakeSet1);
            Assert.Null(context.FakeSet2);
        }

        #endregion

        #region Positive tests for discovery attributes on contexts with inheritance

        [SuppressDbSetInitialization]
        private class DerivedDbContextWithInheritanceLevel1 : DbContext
        {
            public DbSet<DBNull> FakeSet1 { get; set; }
        }

        private class DerivedDbContextWithInheritanceLevel2 : DerivedDbContextWithInheritanceLevel1
        {
            public DbSet<Random> FakeSet2 { get; set; }
        }

        [SuppressDbSetInitialization]
        private class DerivedDbContextWithInheritanceLevel3 : DerivedDbContextWithInheritanceLevel2
        {
            public DbSet<GopherStyleUriParser> FakeSet3 { get; set; }
        }

        [Fact]
        public void DbSetDiscoveryAttribute_on_class_applies_only_to_properties_in_that_class()
        {
            var context = new DerivedDbContextWithInheritanceLevel3();
            DiscoverAndInitializeSets(context, 3);

            Assert.Null(context.FakeSet1);
            Assert.NotNull(context.FakeSet2);
            Assert.Null(context.FakeSet3);
        }

        private class DerivedDbContextForPropertyOverrideLevel1 : DbContext
        {
            public virtual DbSet<Random> FakeSet1 { get; set; }
        }

        private class DerivedDbContextForPropertyOverrideLevel2 : DerivedDbContextForPropertyOverrideLevel1
        {
            [SuppressDbSetInitialization]
            public override DbSet<Random> FakeSet1 { get; set; }
        }

        [Fact]
        public void DbSetDiscoveryAttribute_on_overriden_property_is_used()
        {
            var context = new DerivedDbContextForPropertyOverrideLevel2();
            DiscoverAndInitializeSets(context, 1);

            Assert.Null(context.FakeSet1);
        }

        private class DerivedDbContextForPropertyHideLevel1 : DbContext
        {
            public DbSet<Random> FakeSet1 { get; set; }
        }

        private class DerivedDbContextForPropertyHideLevel2 : DerivedDbContextForPropertyHideLevel1
        {
            [SuppressDbSetInitialization]
            public new DbSet<Random> FakeSet1 { get; set; }
        }

        [Fact]
        public void DbSetDiscoveryAttribute_on_hiding_property_is_used()
        {
            var context = new DerivedDbContextForPropertyHideLevel2();
            DiscoverAndInitializeSets(context, 1);

            Assert.Null(context.FakeSet1);
        }

        private class DerivedDbContextWithInheritanceBLevel1 : DbContext
        {
            public virtual DbSet<Random> FakeSet1 { get; set; }
        }

        [SuppressDbSetInitialization]
        private class DerivedDbContextWithInheritanceBLevel2 : DerivedDbContextWithInheritanceBLevel1
        {
            public override DbSet<Random> FakeSet1 { get; set; }
        }

        [Fact]
        public void DbSetDiscoveryAttribute_on_class_applies_to_properties_overriden_in_that_class()
        {
            var context = new DerivedDbContextWithInheritanceBLevel2();
            DiscoverAndInitializeSets(context, 1);

            Assert.Null(context.FakeSet1);
        }

        #endregion

        #region Static tests for SuppressDbSetInitializationAttribute usage

        [Fact]
        public void DbSetDiscoveryAttribute_does_not_allow_multiple_uses()
        {
            Assert.False(GetDbSetDiscoveryAttributeUsage().AllowMultiple);
        }

        [Fact]
        public void DbSetDiscoveryAttribute_can_be_applied_to_classes_or_properties_only()
        {
            Assert.Equal(AttributeTargets.Class | AttributeTargets.Property, GetDbSetDiscoveryAttributeUsage().ValidOn);
        }

        private static AttributeUsageAttribute GetDbSetDiscoveryAttributeUsage()
        {
            return typeof(SuppressDbSetInitializationAttribute).GetCustomAttributes<AttributeUsageAttribute>(inherit: false).Single();
        }

        #endregion

        #region Helpers

        internal class EntityTypeConfigurationForMock : EntityTypeConfiguration
        {
            public EntityTypeConfigurationForMock()
                : base(typeof(FakeEntity))
            {
            }
        }

        private void AssertExpectedSetsDiscovered(DbContext context, IEnumerable<string> expected)
        {
            var mockBuilder = new Mock<DbModelBuilder>();
            var mockConfig = new Mock<EntityTypeConfigurationForMock>();
            mockBuilder.Setup(b => b.Entity(It.IsAny<Type>())).Returns(mockConfig.Object);

            var discoveryService = new DbSetDiscoveryService(context);
            discoveryService.RegisterSets(mockBuilder.Object);

            foreach (var setName in expected)
            {
                var name = setName;
                mockConfig.VerifySet(c => c.EntitySetName = name, Times.Once());
            }
        }

        private static void DiscoverAndInitializeSets(DbContext context, int setCount)
        {
            var mockBuilder = new Mock<DbModelBuilder>();
            var mockConfig = new Mock<EntityTypeConfigurationForMock>();
            mockBuilder.Setup(b => b.Entity(It.IsAny<Type>())).Returns(mockConfig.Object);

            var discoveryService = new DbSetDiscoveryService(context);
            discoveryService.RegisterSets(mockBuilder.Object);

            mockBuilder.Verify(b => b.Entity(It.IsAny<Type>()), Times.Exactly(setCount));
            mockConfig.VerifySet(c => c.EntitySetName = It.IsAny<string>(), Times.Exactly(setCount));
        }

        #endregion
    }
}
