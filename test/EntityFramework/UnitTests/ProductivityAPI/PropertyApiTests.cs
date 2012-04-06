namespace ProductivityApiUnitTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Linq.Expressions;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for the Property, Reference, and Collection methods on DbEntityEntry.
    /// </summary>
    public class PropertyApiTests : UnitTestBase
    {
        #region Helpers

        private static readonly PropertyEntryMetadata ValueTypePropertyMetadata = new PropertyEntryMetadata(typeof(FakeWithProps), typeof(int), "ValueTypeProp", isMapped: true, isComplex: false);
        private static readonly PropertyEntryMetadata RefTypePropertyMetadata = new PropertyEntryMetadata(typeof(FakeWithProps), typeof(string), "RefTypeProp", isMapped: true, isComplex: false);
        private static readonly PropertyEntryMetadata ComplexPropertyMetadata = new PropertyEntryMetadata(typeof(FakeWithProps), typeof(FakeWithProps), "ComplexProp", isMapped: true, isComplex: true);
        private static readonly PropertyEntryMetadata FakeNamedFooPropertyMetadata = new PropertyEntryMetadata(typeof(FakeEntity), typeof(string), "Foo", isMapped: false, isComplex: false);

        private static readonly NavigationEntryMetadata ReferenceMetadata = new NavigationEntryMetadata(typeof(FakeWithProps), typeof(FakeEntity), "Reference", isCollection: false);
        private static readonly NavigationEntryMetadata CollectionMetadata = new NavigationEntryMetadata(typeof(FakeWithProps), typeof(FakeEntity), "Collection", isCollection: true);
        private static readonly NavigationEntryMetadata HiddenReferenceMetadata = new NavigationEntryMetadata(typeof(FakeWithProps), typeof(FakeEntity), "HiddenReference", isCollection: false);
        private static readonly NavigationEntryMetadata HiddenCollectionMetadata = new NavigationEntryMetadata(typeof(FakeWithProps), typeof(FakeEntity), "HiddenCollection", isCollection: true);

        private static Mock<IEntityStateEntry> CreateMockStateEntry<TEntity>() where TEntity : class, new()
        {
            var mockStateEntry = new Mock<IEntityStateEntry>();
            var fakeEntity = new TEntity();
            mockStateEntry.Setup(e => e.Entity).Returns(fakeEntity);

            var modifiedProps = new List<string> { "Foo", "ValueTypeProp", "Bar" };
            mockStateEntry.Setup(e => e.GetModifiedProperties()).Returns(modifiedProps);
            mockStateEntry.Setup(e => e.SetModifiedProperty(It.IsAny<string>())).Callback<string>(modifiedProps.Add);
            
            return mockStateEntry;
        }

        private static Mock<InternalEntityEntryForMock<FakeWithProps>> CreateMockInternalEntry(InternalPropertyValues currentValues = null,
                                                                                               InternalPropertyValues originalValues = null)
        {
            currentValues = currentValues ?? CreateSimpleValues(10);
            var entity = currentValues.ToObject();
            var mockInternalEntry = new Mock<InternalEntityEntryForMock<FakeWithProps>>();
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", It.IsAny<Type>(), It.IsAny<Type>())).Returns(ValueTypePropertyMetadata);
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", It.IsAny<Type>(), It.IsAny<Type>())).Returns(RefTypePropertyMetadata);
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("ComplexProp", It.IsAny<Type>(), It.IsAny<Type>())).Returns(ComplexPropertyMetadata);
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("Reference", It.IsAny<Type>(), It.IsAny<Type>()));
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("Collection", It.IsAny<Type>(), It.IsAny<Type>()));
            mockInternalEntry.Setup(e => e.ValidateAndGetPropertyMetadata("Missing", It.IsAny<Type>(), It.IsAny<Type>()));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("ValueTypeProp"));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("RefTypeProp"));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("ComplexProp"));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("Reference")).Returns(ReferenceMetadata);
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("Collection")).Returns(CollectionMetadata);
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("ValueTypeProp"));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("RefTypeProp"));
            mockInternalEntry.Setup(e => e.GetNavigationMetadata("ComplexProp"));
            mockInternalEntry.Setup(e => e.GetRelatedEnd("Reference")).Returns(new EntityReference<FakeEntity>());
            mockInternalEntry.Setup(e => e.GetRelatedEnd("Collection")).Returns(new EntityCollection<FakeEntity>());
            mockInternalEntry.SetupGet(e => e.CurrentValues).Returns(currentValues);
            mockInternalEntry.SetupGet(e => e.OriginalValues).Returns(originalValues ?? CreateSimpleValues(20));
            mockInternalEntry.SetupGet(e => e.Entity).Returns(entity);
            mockInternalEntry.SetupGet(e => e.EntityType).Returns(typeof(FakeWithProps));
            mockInternalEntry.CallBase = true;
            return mockInternalEntry;
        }

        private static Mock<InternalEntityEntryForMock<TEntity>> CreateMockInternalEntryForNavs<TEntity>(TEntity entity, IRelatedEnd relatedEnd, bool isDetached) where TEntity : class, new() 
        {
            var mockInternalEntry = new Mock<InternalEntityEntryForMock<TEntity>>();
            mockInternalEntry.SetupGet(e => e.Entity).Returns(entity);
            mockInternalEntry.SetupGet(e => e.EntityType).Returns(typeof(TEntity));
            mockInternalEntry.Setup(e => e.GetRelatedEnd(It.IsAny<string>())).Returns(relatedEnd);
            mockInternalEntry.SetupGet(e => e.IsDetached).Returns(isDetached);
            return mockInternalEntry;
        }

        internal class InternalEntityEntryForMock<TEntity> : InternalEntityEntry where TEntity : class, new()
        {
            public InternalEntityEntryForMock()
                : base(new Mock<InternalContextForMock>().Object, CreateMockStateEntry<TEntity>().Object)
            {
            }
        }

        internal class FakeWithProps
        {
            public int ValueTypeProp { get; set; }
            public string RefTypeProp { get; set; }
            public FakeWithProps ComplexProp { get; set; }
            public FakeEntity Reference { get; set; }
            public ICollection<FakeEntity> Collection { get; set; }
        }

        internal class DerivedFakeWithProps : FakeWithProps
        {
        }

        private static TestInternalPropertyValues<FakeWithProps> CreateSimpleValues(int tag)
        {
            var level3Properties = new Dictionary<string, object>
            {
                {"ValueTypeProp", 3 + tag},
                {"RefTypeProp", "3" + tag},
            };
            var level3Values = new TestInternalPropertyValues<FakeWithProps>(level3Properties);

            var level2Properties = new Dictionary<string, object>
            {
                {"ValueTypeProp", 2 + tag},
                {"RefTypeProp", "2" + tag},
                {"ComplexProp", level3Values},
            };
            var level2Values = new TestInternalPropertyValues<FakeWithProps>(level2Properties, new[] { "ComplexProp" });

            var level1Properties = new Dictionary<string, object>
            {
                {"ValueTypeProp", 1 + tag},
                {"RefTypeProp", "1" + tag},
                {"ComplexProp", level2Values},
            };
            return new TestInternalPropertyValues<FakeWithProps>(level1Properties, new[] { "ComplexProp" });
        }

        #endregion

        #region Obtaining reference/collection/property entries

        [Fact]
        public void Can_get_reference_entry_using_expression_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbReferenceEntry<FakeWithProps, FakeEntity> propEntry = entityEntry.Reference(e => e.Reference);
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
        }

        [Fact]
        public void Can_get_reference_entry_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbReferenceEntry propEntry = entityEntry.Reference("Reference");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
        }

        [Fact]
        public void Can_get_reference_entry_using_generic_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbReferenceEntry<FakeWithProps, FakeEntity> propEntry = entityEntry.Reference<FakeEntity>("Reference");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
        }

        [Fact]
        public void Can_get_reference_entry_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            DbReferenceEntry propEntry = entityEntry.Reference("Reference");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
        }

        [Fact]
        public void Can_get_collection_entry_using_expression_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbCollectionEntry<FakeWithProps, FakeEntity> propEntry = entityEntry.Collection(e => e.Collection);
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
        }

        [Fact]
        public void Can_get_collection_entry_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbCollectionEntry propEntry = entityEntry.Collection("Collection");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
        }

        [Fact]
        public void Can_get_collection_entry_using_generic_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbCollectionEntry<FakeWithProps, FakeEntity> propEntry = entityEntry.Collection<FakeEntity>("Collection");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
        }

        [Fact]
        public void Can_get_collection_entry_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            DbCollectionEntry propEntry = entityEntry.Collection("Collection");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
        }

        [Fact]
        public void Can_get_value_type_property_entry_using_expression_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbPropertyEntry<FakeWithProps, int> propEntry = entityEntry.Property(e => e.ValueTypeProp);
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
        }

        [Fact]
        public void Can_get_value_type_property_entry_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbPropertyEntry propEntry = entityEntry.Property("ValueTypeProp");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_value_type_property_entry_using_generic_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbPropertyEntry<FakeWithProps, int> propEntry = entityEntry.Property<int>("ValueTypeProp");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
        }

        [Fact]
        public void Can_get_value_type_property_entry_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            DbPropertyEntry propEntry = entityEntry.Property("ValueTypeProp");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_reference_type_property_entry_using_expression_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbPropertyEntry<FakeWithProps, string> propEntry = entityEntry.Property(e => e.RefTypeProp);
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(string)));
        }

        [Fact]
        public void Can_get_reference_type_property_entry_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbPropertyEntry propEntry = entityEntry.Property("RefTypeProp");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_reference_type_property_entry_using_generic_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbPropertyEntry<FakeWithProps, string> propEntry = entityEntry.Property<string>("RefTypeProp");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(string)));
        }

        [Fact]
        public void Can_get_reference_type_property_entry_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            DbPropertyEntry propEntry = entityEntry.Property("RefTypeProp");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_using_expression_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbPropertyEntry<FakeWithProps, FakeWithProps> propEntry = entityEntry.Property(e => e.ComplexProp);
            Assert.NotNull(propEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbPropertyEntry propEntry = entityEntry.Property("ComplexProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbComplexPropertyEntry>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_using_generic_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbPropertyEntry<FakeWithProps, FakeWithProps> propEntry = entityEntry.Property<FakeWithProps>("ComplexProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            DbPropertyEntry propEntry = entityEntry.Property("ComplexProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbComplexPropertyEntry>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_using_ComplexProperty_with_expression_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbComplexPropertyEntry<FakeWithProps, FakeWithProps> propEntry = entityEntry.ComplexProperty(e => e.ComplexProp);
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_using_ComplexProperty_with_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbComplexPropertyEntry propEntry = entityEntry.ComplexProperty("ComplexProp");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_using_generic_ComplexProperty_with_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            DbComplexPropertyEntry<FakeWithProps, FakeWithProps> propEntry = entityEntry.ComplexProperty<FakeWithProps>("ComplexProp");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_using_ComplexProperty_with_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            DbComplexPropertyEntry propEntry = entityEntry.ComplexProperty("ComplexProp");
            Assert.NotNull(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_reference_entry_with_Member_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var propEntry = entityEntry.Member("Reference");
            Assert.NotNull(propEntry);
            Assert.IsType<DbReferenceEntry>(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
        }

        [Fact]
        public void Can_get_reference_entry_with_Member_using_generic_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var propEntry = entityEntry.Member<FakeEntity>("Reference");
            Assert.NotNull(propEntry);
            Assert.IsType<DbReferenceEntry<FakeWithProps, FakeEntity>>(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
        }

        [Fact]
        public void Can_get_reference_entry_with_Member_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var propEntry = entityEntry.Member("Reference");
            Assert.NotNull(propEntry);
            Assert.IsType<DbReferenceEntry>(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Reference"));
        }

        [Fact]
        public void Can_get_collection_entry_with_Member_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var propEntry = entityEntry.Member("Collection");
            Assert.NotNull(propEntry);
            Assert.IsType<DbCollectionEntry>(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
        }

        [Fact]
        public void Can_get_collection_entry_with_Member_using_generic_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var propEntry = entityEntry.Member<ICollection<FakeEntity>>("Collection");
            Assert.NotNull(propEntry);
            Assert.IsType<DbCollectionEntry<FakeWithProps, FakeEntity>>(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
        }

        [Fact]
        public void Can_get_collection_entry_with_Member_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var propEntry = entityEntry.Member("Collection");
            Assert.NotNull(propEntry);
            Assert.IsType<DbCollectionEntry>(propEntry);

            mockInternalEntry.Verify(e => e.GetNavigationMetadata("Collection"));
        }

        [Fact]
        public void Can_get_value_type_property_entry_with_Member_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var propEntry = entityEntry.Member("ValueTypeProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbPropertyEntry>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_value_type_property_entry_with_Member_using_generic_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var propEntry = entityEntry.Member<int>("ValueTypeProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbPropertyEntry<FakeWithProps, int>>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
        }

        [Fact]
        public void Can_get_value_type_property_entry_with_Member_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var propEntry = entityEntry.Member("ValueTypeProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbPropertyEntry>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_reference_type_property_entry_with_Member_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var propEntry = entityEntry.Member("RefTypeProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbPropertyEntry>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_reference_type_property_entry_with_Member_using_generic_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var propEntry = entityEntry.Member<string>("RefTypeProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbPropertyEntry<FakeWithProps, string>>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(string)));
        }

        [Fact]
        public void Can_get_reference_type_property_entry_with_Member_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var propEntry = entityEntry.Member("RefTypeProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbPropertyEntry>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("RefTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_with_Member_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var propEntry = entityEntry.Member("ComplexProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbComplexPropertyEntry>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_with_Member_using_generic_string_method_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var propEntry = entityEntry.Member<FakeWithProps>("ComplexProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_complex_type_property_entry_with_Member_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var propEntry = entityEntry.Member("ComplexProp");
            Assert.NotNull(propEntry);
            Assert.IsType<DbComplexPropertyEntry>(propEntry);

            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Passing_null_expression_to_generic_DbEntityEntry_Reference_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal("navigationProperty", Assert.Throws<ArgumentNullException>(() => entityEntry.Reference((Expression<Func<FakeWithProps, string>>)null)).ParamName);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbEntityEntry_Reference_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Reference((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbEntityEntry_Reference_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Reference("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbEntityEntry_Reference_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Reference(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbEntityEntry_Reference_generic_string_method_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Reference<object>((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbEntityEntry_Reference_generic_string_method_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Reference<string>("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbEntityEntry_Reference_generic_string_method_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Reference<Random>(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_non_generic_DbEntityEntry_Reference_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Reference((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_non_generic_DbEntityEntry_Reference_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Reference("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_non_generic_DbEntityEntry_Reference_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Reference(" ")).Message);
        }

        [Fact]
        public void Passing_null_expression_to_generic_DbEntityEntry_Collection_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal("navigationProperty", Assert.Throws<ArgumentNullException>(() => entityEntry.Collection((Expression<Func<FakeWithProps, ICollection<string>>>)null)).ParamName);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbEntityEntry_Collection_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Collection((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbEntityEntry_Collection_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Collection("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbEntityEntry_Collection_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Collection(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbEntityEntry_Collection_generic_string_method_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Collection<string>((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbEntityEntry_Collection_generic_string_method_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Collection<object>("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbEntityEntry_Collection_generic_string_method_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Collection<Random>(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_non_generic_DbEntityEntry_Collection_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Collection((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_non_generic_DbEntityEntry_Collection_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Collection("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_non_generic_DbEntityEntry_Collection_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("navigationProperty"), Assert.Throws<ArgumentException>(() => entityEntry.Collection(" ")).Message);
        }

        [Fact]
        public void Passing_null_expression_to_generic_DbEntityEntry_Property_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal("property", Assert.Throws<ArgumentNullException>(() => entityEntry.Property((Expression<Func<FakeWithProps, string>>)null)).ParamName);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbEntityEntry_Property_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Property((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbEntityEntry_Property_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Property("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbEntityEntry_Property_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Property(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbEntityEntry_generic_string_Property_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Property<int>((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbEntityEntry_generic_string_Property_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbEntityEntry_generic_string_Property_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Property<Random>(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_non_generic_DbEntityEntry_Property_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Property((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_non_generic_DbEntityEntry_Property_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Property("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_non_generic_DbEntityEntry_Property_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Property(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbEntityEntry_ComplexProperty_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbEntityEntry_ComplexProperty_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbEntityEntry_ComplexProperty_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbEntityEntry_generic_string_ComplexProperty_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<int>((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbEntityEntry_generic_string_ComplexProperty_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbEntityEntry_generic_string_ComplexProperty_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<Random>(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_non_generic_DbEntityEntry_ComplexProperty_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_non_generic_DbEntityEntry_ComplexProperty_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_non_generic_DbEntityEntry_ComplexProperty_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbEntityEntry_Member_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Member((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbEntityEntry_Member_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Member("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbEntityEntry_Member_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Member(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbEntityEntry_generic_string_Member_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Member<int>((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbEntityEntry_generic_string_Member_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbEntityEntry_generic_string_Member_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Member<Random>(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_non_generic_DbEntityEntry_Member_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Member((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_non_generic_DbEntityEntry_Member_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Member("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_non_generic_DbEntityEntry_Member_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => entityEntry.Member(" ")).Message);
        }

        [Fact]
        public void Passing_bad_expression_to_generic_DbEntityEntry_Reference_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Reference", "FakeWithProps"), "navigationProperty").Message, Assert.Throws<ArgumentException>(() => entityEntry.Reference(e => new FakeEntity())).Message);
        }

        [Fact]
        public void Passing_bad_expression_to_generic_DbEntityEntry_Collection_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Collection", "FakeWithProps"), "navigationProperty").Message, Assert.Throws<ArgumentException>(() => entityEntry.Collection(e => new List<FakeEntity>())).Message);
        }

        [Fact]
        public void Passing_bad_expression_to_generic_DbEntityEntry_Property_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Property", "FakeWithProps"), "property").Message, Assert.Throws<ArgumentException>(() => entityEntry.Property(e => new FakeEntity())).Message);
        }

        [Fact]
        public void Passing_bad_expression_to_generic_DbEntityEntry_ComplexProperty_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Property", "FakeWithProps"), "property").Message, Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(e => new FakeEntity())).Message);
        }

        #endregion

        #region DbPropertyEntry classes use the underlying InternalPropertyEntry

        [Fact]
        public void Non_Generic_DbPropertyEntry_uses_original_values_on_InternalPropertyEntry()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new DbPropertyEntry(new InternalEntityPropertyEntry(entityEntry, ValueTypePropertyMetadata));

            var valueBeforeSet = propEntry.OriginalValue;
            propEntry.OriginalValue = -1;

            Assert.Equal(21, valueBeforeSet);
            Assert.Equal(-1, entityEntry.OriginalValues["ValueTypeProp"]);
        }

        [Fact]
        public void Generic_DbPropertyEntry_uses_original_values_on_InternalPropertyEntry()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new DbPropertyEntry<FakeWithProps, int>(new InternalEntityPropertyEntry(entityEntry, ValueTypePropertyMetadata));

            var valueBeforeSet = propEntry.OriginalValue;
            propEntry.OriginalValue = -1;

            Assert.Equal(21, valueBeforeSet);
            Assert.Equal(-1, entityEntry.OriginalValues["ValueTypeProp"]);
        }

        [Fact]
        public void Non_Generic_DbPropertyEntry_uses_current_values_on_InternalPropertyEntry()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new DbPropertyEntry(new InternalEntityPropertyEntry(entityEntry, ValueTypePropertyMetadata));

            var valueBeforeSet = propEntry.CurrentValue;
            propEntry.CurrentValue = -1;

            Assert.Equal(11, valueBeforeSet);
            Assert.Equal(-1, entityEntry.CurrentValues["ValueTypeProp"]);
        }

        [Fact]
        public void Generic_DbPropertyEntry_uses_current_values_on_InternalPropertyEntry()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new DbPropertyEntry<FakeWithProps, int>(new InternalEntityPropertyEntry(entityEntry, ValueTypePropertyMetadata));

            var valueBeforeSet = propEntry.CurrentValue;
            propEntry.CurrentValue = -1;

            Assert.Equal(11, valueBeforeSet);
            Assert.Equal(-1, entityEntry.CurrentValues["ValueTypeProp"]);
        }

        [Fact]
        public void Non_Generic_DbPropertyEntry_uses_IsModified_on_InternalPropertyEntry()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var mockStateEntry = Mock.Get(entityEntry.ObjectStateEntry);
            var propEntry = new DbPropertyEntry(new InternalEntityPropertyEntry(entityEntry, RefTypePropertyMetadata));

            Assert.False(propEntry.IsModified);

            propEntry.IsModified = true;

            Assert.True(propEntry.IsModified);
            mockStateEntry.Verify(e => e.SetModifiedProperty("RefTypeProp"));
        }

        [Fact]
        public void Generic_DbPropertyEntry_uses_IsModified_on_InternalPropertyEntry()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var mockStateEntry = Mock.Get(entityEntry.ObjectStateEntry);
            var propEntry = new DbPropertyEntry<FakeWithProps, string>(new InternalEntityPropertyEntry(entityEntry, RefTypePropertyMetadata));

            Assert.False(propEntry.IsModified);

            propEntry.IsModified = true;

            Assert.True(propEntry.IsModified);
            mockStateEntry.Verify(e => e.SetModifiedProperty("RefTypeProp"));
        }

        [Fact]
        public void Non_Generic_DbReferenceEntry_gets_CurrentValue_from_InternalNavigationEntry()
        {
            var mockEntry = new Mock<InternalReferenceEntryForMock>();
            var refEntry = new DbReferenceEntry(mockEntry.Object);

            var _ = refEntry.CurrentValue;

            mockEntry.VerifyGet(e => e.CurrentValue, Times.Once());
        }

        [Fact]
        public void Generic_DbReferenceEntry_gets_CurrentValue_from_InternalNavigationEntry()
        {
            var mockEntry = new Mock<InternalReferenceEntryForMock>();
            var refEntry = new DbReferenceEntry<FakeWithProps, FakeEntity>(mockEntry.Object);

            var _ = refEntry.CurrentValue;

            mockEntry.VerifyGet(e => e.CurrentValue, Times.Once());
        }

        [Fact]
        public void Non_Generic_DbReferenceEntry_sets_CurrentValue_to_InternalNavigationEntry()
        {
            var mockEntry = new Mock<InternalReferenceEntryForMock>();
            var refEntry = new DbReferenceEntry(mockEntry.Object);

            refEntry.CurrentValue = new FakeEntity();

            mockEntry.VerifySet(e => e.CurrentValue = It.IsAny<object>(), Times.Once());
        }

        [Fact]
        public void Generic_DbReferenceEntry_sets_CurrentValue_to_InternalNavigationEntry()
        {
            var mockEntry = new Mock<InternalReferenceEntryForMock>();
            var refEntry = new DbReferenceEntry<FakeWithProps, FakeEntity>(mockEntry.Object);

            refEntry.CurrentValue = new FakeEntity();

            mockEntry.VerifySet(e => e.CurrentValue = It.IsAny<object>(), Times.Once());
        }

        [Fact]
        public void Non_Generic_DbCollectionEntry_gets_CurrentValue_from_InternalNavigationEntry()
        {
            var mockEntry = new Mock<InternalCollectionEntryForMock>();
            var refEntry = new DbCollectionEntry(mockEntry.Object);

            var _ = refEntry.CurrentValue;

            mockEntry.VerifyGet(e => e.CurrentValue, Times.Once());
        }

        [Fact]
        public void Generic_DbCollectionEntry_gets_CurrentValue_from_InternalNavigationEntry()
        {
            var mockEntry = new Mock<InternalCollectionEntryForMock>();
            var refEntry = new DbCollectionEntry<FakeWithProps, FakeEntity>(mockEntry.Object);

            var _ = refEntry.CurrentValue;

            mockEntry.VerifyGet(e => e.CurrentValue, Times.Once());
        }

        [Fact]
        public void Non_Generic_DbCollectionEntry_sets_CurrentValue_to_InternalNavigationEntry()
        {
            var mockEntry = new Mock<InternalCollectionEntryForMock>();
            var refEntry = new DbCollectionEntry(mockEntry.Object);

            refEntry.CurrentValue = new List<FakeEntity>();

            mockEntry.VerifySet(e => e.CurrentValue = It.IsAny<object>(), Times.Once());
        }

        [Fact]
        public void Generic_DbCollectionEntry_sets_CurrentValue_to_InternalNavigationEntry()
        {
            var mockEntry = new Mock<InternalCollectionEntryForMock>();
            var refEntry = new DbCollectionEntry<FakeWithProps, FakeEntity>(mockEntry.Object);

            refEntry.CurrentValue = new List<FakeEntity>();

            mockEntry.VerifySet(e => e.CurrentValue = It.IsAny<object>(), Times.Once());
        }

        #endregion

        #region Setting and reading current and original values

        [Fact]
        public void Scalar_original_value_can_be_set()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ValueTypePropertyMetadata);

            propEntry.OriginalValue = -1;

            Assert.Equal(-1, entityEntry.OriginalValues["ValueTypeProp"]);
        }

        [Fact]
        public void Scalar_current_value_can_be_set()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ValueTypePropertyMetadata);

            propEntry.CurrentValue = -1;

            Assert.Equal(-1, entityEntry.CurrentValues["ValueTypeProp"]);
        }

        [Fact]
        public void Scalar_original_value_can_be_read()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ValueTypePropertyMetadata);

            var value = propEntry.OriginalValue;

            Assert.Equal(21, value);
        }

        [Fact]
        public void Scalar_current_value_can_be_read()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ValueTypePropertyMetadata);

            var value = propEntry.CurrentValue;

            Assert.Equal(11, value);
        }

        [Fact]
        public void Scalar_original_value_can_be_set_to_null()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, RefTypePropertyMetadata);

            propEntry.OriginalValue = null;

            Assert.Null(entityEntry.OriginalValues["RefTypeProp"]);
        }

        [Fact]
        public void Scalar_current_value_can_be_set_to_null()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, RefTypePropertyMetadata);

            propEntry.CurrentValue = null;

            Assert.Null(entityEntry.CurrentValues["RefTypeProp"]);
        }

        [Fact]
        public void Scalar_original_value_can_be_read_when_when_it_is_null()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, RefTypePropertyMetadata);

            propEntry.OriginalValue = null;
            var value = propEntry.OriginalValue;

            Assert.Null(value);
        }

        [Fact]
        public void Original_value_for_scalar_property_cannot_be_set_to_instance_of_wrong_type()
        {
            SetWrongTypeTest(e => e.OriginalValue = new Random());
        }

        [Fact]
        public void Current_value_for_scalar_property_cannot_be_set_to_instance_of_wrong_type()
        {
            SetWrongTypeTest(e => e.CurrentValue = new Random());
        }

        private void SetWrongTypeTest(Action<InternalEntityPropertyEntry> setValue)
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, RefTypePropertyMetadata);

            Assert.Equal(Strings.DbPropertyValues_WrongTypeForAssignment(typeof(Random).Name, "RefTypeProp", typeof(string).Name, typeof(FakeWithProps).Name), Assert.Throws<InvalidOperationException>(() => setValue(propEntry)).Message);
        }

        [Fact]
        public void Original_value_returned_for_complex_property_is_object_instance()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            var value = (FakeWithProps)propEntry.OriginalValue;

            Assert.Equal(22, value.ValueTypeProp);
            Assert.Equal(23, value.ComplexProp.ValueTypeProp);
        }

        [Fact]
        public void Current_value_returned_for_complex_property_is_actual_complex_object_instance()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);
            var entity = (FakeWithProps)entityEntry.Entity;

            var value = (FakeWithProps)propEntry.CurrentValue;

            Assert.Same(entity.ComplexProp, value);
            Assert.Equal(12, value.ValueTypeProp);
            Assert.Equal(13, value.ComplexProp.ValueTypeProp);
        }

        [Fact]
        public void Original_value_returned_for_complex_property_can_be_null()
        {
            var properties = new Dictionary<string, object> { {"ComplexProp", null} };
            var originalValues = new TestInternalPropertyValues<FakeWithProps>(properties, new[] { "ComplexProp" });
            var entityEntry = CreateMockInternalEntry(null, originalValues).Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            var value = propEntry.OriginalValue;

            Assert.Null(value);
        }

        [Fact]
        public void Current_value_returned_for_complex_property_can_be_null()
        {
            var properties = new Dictionary<string, object> { { "ComplexProp", null } };
            var currentValues = new TestInternalPropertyValues<FakeWithProps>(properties, new[] { "ComplexProp" });
            var entityEntry = CreateMockInternalEntry(currentValues).Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            var value = propEntry.CurrentValue;

            Assert.Null(value);
        }

        [Fact]
        public void Complex_original_value_can_be_set()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            propEntry.OriginalValue = new FakeWithProps { ValueTypeProp = -2, ComplexProp = new FakeWithProps { ValueTypeProp = -3 } };

            Assert.Equal(21, entityEntry.OriginalValues["ValueTypeProp"]);
            Assert.Equal(-2, ((InternalPropertyValues)entityEntry.OriginalValues["ComplexProp"])["ValueTypeProp"]);
            Assert.Equal(-3, ((InternalPropertyValues)((InternalPropertyValues)entityEntry.OriginalValues["ComplexProp"])["ComplexProp"])["ValueTypeProp"]);
        }

        [Fact]
        public void Complex_current_value_can_be_set_and_the_actual_complex_object_is_set()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);
            var entity = (FakeWithProps)entityEntry.Entity;

            var complexObject = new FakeWithProps { ValueTypeProp = -2, ComplexProp = new FakeWithProps { ValueTypeProp = -3 } };
            propEntry.CurrentValue = complexObject;

            Assert.Same(entity.ComplexProp, complexObject);
            Assert.Equal(-2, entity.ComplexProp.ValueTypeProp);
            Assert.Equal(-3, entity.ComplexProp.ComplexProp.ValueTypeProp);

            Assert.Equal(11, entityEntry.CurrentValues["ValueTypeProp"]);
            Assert.Equal(-2, ((InternalPropertyValues)entityEntry.CurrentValues["ComplexProp"])["ValueTypeProp"]);
            Assert.Equal(-3, ((InternalPropertyValues)((InternalPropertyValues)entityEntry.CurrentValues["ComplexProp"])["ComplexProp"])["ValueTypeProp"]);
        }

        [Fact]
        public void Original_value_for_complex_property_cannot_be_set_to_null()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            Assert.Equal(Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", "FakeWithProps"), Assert.Throws<InvalidOperationException>(() => propEntry.OriginalValue = null).Message);
        }

        [Fact]
        public void Original_value_for_complex_property_cannot_be_set_to_null_even_if_it_is_already_null()
        {
            var properties = new Dictionary<string, object> { { "ComplexProp", null } };
            var originalValues = new TestInternalPropertyValues<FakeWithProps>(properties, new[] { "ComplexProp" });
            var entityEntry = CreateMockInternalEntry(null, originalValues).Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            Assert.Equal(Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", "FakeWithProps"), Assert.Throws<InvalidOperationException>(() => propEntry.OriginalValue = null).Message);
        }

        [Fact]
        public void Current_value_for_complex_property_cannot_be_set_to_null()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            Assert.Equal(Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", "FakeWithProps"), Assert.Throws<InvalidOperationException>(() => propEntry.CurrentValue = null).Message);
        }

        [Fact]
        public void Current_value_for_complex_property_cannot_be_set_to_null_even_if_it_is_already_null()
        {
            var properties = new Dictionary<string, object> { { "ComplexProp", null } };
            var currentValues = new TestInternalPropertyValues<FakeWithProps>(properties, new[] { "ComplexProp" });
            var entityEntry = CreateMockInternalEntry(currentValues).Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            Assert.Equal(Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", "FakeWithProps"), Assert.Throws<InvalidOperationException>(() => propEntry.CurrentValue = null).Message);
        }

        [Fact]
        public void Original_value_for_complex_property_cannot_be_set_to_instance_with_nested_null_complex_property()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            var complexObject = new FakeWithProps { ValueTypeProp = -2, ComplexProp = null };

            Assert.Equal(Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", typeof(FakeWithProps).Name), Assert.Throws<InvalidOperationException>(() => propEntry.OriginalValue = complexObject).Message);
        }

        [Fact]
        public void Current_value_for_complex_property_cannot_be_set_to_instance_with_nested_null_complex_property()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            var complexObject = new FakeWithProps { ValueTypeProp = -2, ComplexProp = null };

            Assert.Equal(Strings.DbPropertyValues_ComplexObjectCannotBeNull("ComplexProp", typeof(FakeWithProps).Name), Assert.Throws<InvalidOperationException>(() => propEntry.CurrentValue = complexObject).Message);
        }

        [Fact]
        public void Original_value_for_complex_property_cannot_be_set_to_instance_of_wrong_type()
        {
            SetWrongComplexTypeTest(e => e.OriginalValue = new Random());
        }

        [Fact]
        public void Current_value_for_complex_property_cannot_be_set_to_instance_of_wrong_type()
        {
            SetWrongComplexTypeTest(e => e.CurrentValue = new Random());
        }

        private void SetWrongComplexTypeTest(Action<InternalEntityPropertyEntry> setValue)
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ComplexPropertyMetadata);

            Assert.Equal(Strings.DbPropertyValues_AttemptToSetValuesFromWrongObject(typeof(Random).Name, typeof(FakeWithProps).Name), Assert.Throws<ArgumentException>(() => setValue(propEntry)).Message);
        }

        #endregion

        #region IsModified

        [Fact]
        public void IsModified_returns_true_if_property_is_in_modified_list()
        {
            // Note that CreateMockInternalEntry sets ValueTypeProp as modified
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ValueTypePropertyMetadata);
 
            Assert.True(propEntry.IsModified);
        }

        [Fact]
        public void IsModified_returns_false_if_property_is_not_in_modified_list()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, RefTypePropertyMetadata);

            Assert.False(propEntry.IsModified);
        }

        [Fact]
        public void IsModified_can_be_set_to_true_when_it_is_currently_false()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, RefTypePropertyMetadata);
            var mockStateEntry = Mock.Get(entityEntry.ObjectStateEntry);

            Assert.False(propEntry.IsModified);

            propEntry.IsModified = true;

            Assert.True(propEntry.IsModified);
            mockStateEntry.Verify(e => e.SetModifiedProperty("RefTypeProp"));
        }

        [Fact]
        public void IsModified_can_be_set_to_true_when_it_is_currently_true()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, ValueTypePropertyMetadata);

            Assert.True(propEntry.IsModified);

            propEntry.IsModified = true;

            Assert.True(propEntry.IsModified);
        }

        [Fact]
        public void IsModified_can_be_set_to_false_when_it_is_currently_false()
        {
            var entityEntry = CreateMockInternalEntry().Object;
            var propEntry = new InternalEntityPropertyEntry(entityEntry, RefTypePropertyMetadata);

            Assert.False(propEntry.IsModified);

            propEntry.IsModified = false;

            Assert.False(propEntry.IsModified);
        }

        #endregion

        #region Property name tests

        [Fact]
        public void Non_generic_DbPropertyEntry_Name_returns_name_of_property_from_internal_entry()
        {
            var internalEntry = new InternalEntityPropertyEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object,
                                                                FakeNamedFooPropertyMetadata);
            var propEntry = new DbPropertyEntry(internalEntry);

            Assert.Equal("Foo", internalEntry.Name);
            Assert.Equal("Foo", propEntry.Name);
        }

        [Fact]
        public void Generic_DbPropertyEntry_Name_returns_name_of_property_from_internal_entry()
        {
            var internalEntry = new InternalEntityPropertyEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object,
                                                                FakeNamedFooPropertyMetadata);
            var propEntry = new DbPropertyEntry<FakeWithProps, FakeWithProps>(internalEntry);

            Assert.Equal("Foo", internalEntry.Name);
            Assert.Equal("Foo", propEntry.Name);
        }

        [Fact]
        public void Non_generic_DbReferenceEntry_Name_returns_name_of_property_from_internal_entry()
        {
            var internalEntry = new InternalReferenceEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, ReferenceMetadata);
            var propEntry = new DbReferenceEntry(internalEntry);

            Assert.Equal("Reference", internalEntry.Name);
            Assert.Equal("Reference", propEntry.Name);
        }

        [Fact]
        public void Generic_DbReferenceEntry_Name_returns_name_of_property_from_internal_entry()
        {
            var internalEntry = new InternalReferenceEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, ReferenceMetadata);
            var propEntry = new DbReferenceEntry<FakeWithProps, FakeEntity>(internalEntry);

            Assert.Equal("Reference", internalEntry.Name);
            Assert.Equal("Reference", propEntry.Name);
        }

        [Fact]
        public void Non_generic_DbCollectionEntry_Name_returns_name_of_property_from_internal_entry()
        {
            var internalEntry = new InternalCollectionEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, CollectionMetadata);
            var propEntry = new DbCollectionEntry(internalEntry);

            Assert.Equal("Collection", internalEntry.Name);
            Assert.Equal("Collection", propEntry.Name);
        }

        [Fact]
        public void Generic_DbCollectionEntry_Name_returns_name_of_property_from_internal_entry()
        {
            var internalEntry = new InternalCollectionEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, CollectionMetadata);
            var propEntry = new DbCollectionEntry<FakeWithProps, FakeEntity>(internalEntry);

            Assert.Equal("Collection", internalEntry.Name);
            Assert.Equal("Collection", propEntry.Name);
        }

        #endregion

        #region Tests for implicit conversion of generic to non-generic

        [Fact]
        public void Generic_DbPropertyEntry_can_be_implicitly_converted_to_non_generic_version()
        {
            var propEntry = new DbPropertyEntry<FakeWithProps, FakeEntity>(new InternalEntityPropertyEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object,
                                                                                            FakeNamedFooPropertyMetadata));

            NonGenericTestMethod(propEntry, "Foo");
        }

        private void NonGenericTestMethod(DbPropertyEntry nonGenericEntry, string name)
        {
            Assert.Same(name, nonGenericEntry.Name);
        }

        [Fact]
        public void Generic_DbReferenceEntry_can_be_implicitly_converted_to_non_generic_version()
        {
            var propEntry = new DbReferenceEntry<FakeWithProps, FakeEntity>(new InternalReferenceEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, ReferenceMetadata));

            NonGenericTestMethod(propEntry, "Reference");
        }

        private void NonGenericTestMethod(DbReferenceEntry nonGenericEntry, string name)
        {
            Assert.Same(name, nonGenericEntry.Name);
        }

        [Fact]
        public void Generic_DbCollectionEntry_can_be_implicitly_converted_to_non_generic_version()
        {
            var propEntry = new DbCollectionEntry<FakeWithProps, FakeEntity>(new InternalCollectionEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, CollectionMetadata));

            NonGenericTestMethod(propEntry, "Collection");
        }

        private void NonGenericTestMethod(DbCollectionEntry nonGenericEntry, string name)
        {
            Assert.Same(name, nonGenericEntry.Name);
        }

        [Fact]
        public void Generic_DbPropertyEntry_typed_as_DbMemberEntry_can_be_implicitly_converted_to_non_generic_version()
        {
            DbMemberEntry<FakeWithProps, FakeEntity> propEntry = new DbPropertyEntry<FakeWithProps, FakeEntity>(new InternalEntityPropertyEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object,
                                                                                                                  FakeNamedFooPropertyMetadata));

            NonGenericTestMethodPropAsMember(propEntry, "Foo");
        }

        private void NonGenericTestMethodPropAsMember(DbMemberEntry nonGenericEntry, string name)
        {
            Assert.Same(name, nonGenericEntry.Name);
            Assert.IsType<DbPropertyEntry>(nonGenericEntry);
        }

        [Fact]
        public void Generic_DbReferenceEntry_typed_as_DbMemberEntry_can_be_implicitly_converted_to_non_generic_version()
        {
            DbMemberEntry<FakeWithProps, FakeEntity> propEntry = new DbReferenceEntry<FakeWithProps, FakeEntity>(new InternalReferenceEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, ReferenceMetadata));

            NonGenericTestMethodRefAsMember(propEntry, "Reference");
        }

        private void NonGenericTestMethodRefAsMember(DbMemberEntry nonGenericEntry, string name)
        {
            Assert.Same(name, nonGenericEntry.Name);
            Assert.IsType<DbReferenceEntry>(nonGenericEntry);
        }

        [Fact]
        public void Generic_DbCollectionEntry_typed_as_DbMemberEntry_can_be_implicitly_converted_to_non_generic_version()
        {
            DbMemberEntry<FakeWithProps, ICollection<FakeEntity>> propEntry = new DbCollectionEntry<FakeWithProps, FakeEntity>(new InternalCollectionEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, CollectionMetadata));

            NonGenericTestMethodCollectionAsMember(propEntry, "Collection");
        }

        private void NonGenericTestMethodCollectionAsMember(DbMemberEntry nonGenericEntry, string name)
        {
            Assert.Same(name, nonGenericEntry.Name);
            Assert.IsType<DbCollectionEntry>(nonGenericEntry);
        }

        [Fact]
        public void Generic_DbPropertyEntry_typed_as_DbMemberEntry_can_be_explicitly_converted_to_non_generic_DbPropertyEntry()
        {
            DbMemberEntry<FakeWithProps, FakeEntity> propEntry = new DbPropertyEntry<FakeWithProps, FakeEntity>(new InternalEntityPropertyEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object,
                                                                                                                  FakeNamedFooPropertyMetadata));

            NonGenericTestMethod((DbPropertyEntry)propEntry, "Foo");
        }

        [Fact]
        public void Generic_DbReferenceEntry_typed_as_DbMemberEntry_can_be_implicitly_converted_to_non_generic_DbReferenceEntry()
        {
            DbMemberEntry<FakeWithProps, FakeEntity> propEntry = new DbReferenceEntry<FakeWithProps, FakeEntity>(new InternalReferenceEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, ReferenceMetadata));

            NonGenericTestMethod((DbReferenceEntry)propEntry, "Reference");
        }

        [Fact]
        public void Generic_DbCollectionEntry_typed_as_DbMemberEntry_can_be_implicitly_converted_to_non_generic_DbCollectionEntry()
        {
            DbMemberEntry<FakeWithProps, ICollection<FakeEntity>> propEntry = new DbCollectionEntry<FakeWithProps, FakeEntity>(new InternalCollectionEntry(new Mock<InternalEntityEntryForMock<FakeEntity>>().Object, CollectionMetadata));

            NonGenericTestMethod((DbCollectionEntry)propEntry, "Collection");
        }

        #endregion

        #region Getting current values from navigation properties

        [Fact]
        public void InternalReferenceEntry_gets_current_value_from_entity_if_property_exists()
        {
            InternalReferenceEntry_gets_current_value_from_entity_if_property_exists_implementation(isDetached: false);
        }

        [Fact]
        public void InternalReferenceEntry_gets_current_value_from_entity_even_when_detached_if_property_exists()
        {
            InternalReferenceEntry_gets_current_value_from_entity_if_property_exists_implementation(isDetached: true);
        }

        private void InternalReferenceEntry_gets_current_value_from_entity_if_property_exists_implementation(bool isDetached)
        {
            var relatedEntity = new FakeEntity();
            var entity = new FakeWithProps { Reference = relatedEntity };
            var internalEntry = new InternalReferenceEntry(CreateMockInternalEntryForNavs(entity, null, isDetached).Object, ReferenceMetadata);

            var propValue = internalEntry.CurrentValue;

            Assert.Same(relatedEntity, propValue);
        }

        [Fact]
        public void InternalReferenceEntry_sets_current_value_onto_entity_if_property_exists()
        {
            InternalReferenceEntry_sets_current_value_onto_entity_if_property_exists_implementation(isDetached: false);
        }

        [Fact]
        public void InternalReferenceEntry_sets_current_value_onto_entity_even_when_detached_if_property_exists()
        {
            InternalReferenceEntry_sets_current_value_onto_entity_if_property_exists_implementation(isDetached: true);
        }

        private void InternalReferenceEntry_sets_current_value_onto_entity_if_property_exists_implementation(bool isDetached)
        {
            var entity = new FakeWithProps { Reference = new FakeEntity() };
            var internalEntry = new InternalReferenceEntry(CreateMockInternalEntryForNavs(entity, null, isDetached).Object, ReferenceMetadata);

            var relatedEntity = new FakeEntity();
            internalEntry.CurrentValue = relatedEntity;

            Assert.Same(relatedEntity, entity.Reference);
        }

        [Fact]
        public void InternalCollectionEntry_gets_current_value_from_entity_if_property_exists()
        {
            InternalCollectionEntry_gets_current_value_from_entity_if_property_exists_implementation(isDetached: false);
        }

        [Fact]
        public void InternalCollectionEntry_gets_current_value_from_entity_even_when_detached_if_property_exists()
        {
            InternalCollectionEntry_gets_current_value_from_entity_if_property_exists_implementation(isDetached: true);
        }

        private void InternalCollectionEntry_gets_current_value_from_entity_if_property_exists_implementation(bool isDetached)
        {
            var relatedCollection = new List<FakeEntity>();
            var entity = new FakeWithProps { Collection = relatedCollection };
            var internalEntry = new InternalCollectionEntry(CreateMockInternalEntryForNavs(entity, null, isDetached).Object, CollectionMetadata);

            var propValue = internalEntry.CurrentValue;

            Assert.Same(relatedCollection, propValue);
        }

        [Fact]
        public void InternalCollectionEntry_sets_current_value_onto_entity_if_property_exists()
        {
            InternalCollectionEntry_sets_current_value_onto_entity_if_property_exists_implementation(isDetached: false);
        }

        [Fact]
        public void InternalCollectionEntry_sets_current_value_onto_entity_even_when_detached_if_property_exists()
        {
            InternalCollectionEntry_sets_current_value_onto_entity_if_property_exists_implementation(isDetached: true);
        }

        private void InternalCollectionEntry_sets_current_value_onto_entity_if_property_exists_implementation(bool isDetached)
        {
            var entity = new FakeWithProps { Collection = new List<FakeEntity>() };
            var internalEntry = new InternalCollectionEntry(CreateMockInternalEntryForNavs(entity, null, isDetached).Object, CollectionMetadata);

            var relatedCollection = new List<FakeEntity>();
            internalEntry.CurrentValue = relatedCollection;

            Assert.Same(relatedCollection, entity.Collection);
        }

        private Mock<IRelatedEnd> CreateMockRelatedReference(FakeEntity relatedEntity)
        {
            var mockEnumerator = new Mock<IEnumerator>();
            mockEnumerator.Setup(e => e.MoveNext()).Returns(relatedEntity != null);
            mockEnumerator.Setup(e => e.Current).Returns(relatedEntity);

            var mockRelatedEnd = new Mock<IRelatedEnd>();
            mockRelatedEnd.Setup(r => r.GetEnumerator()).Returns(mockEnumerator.Object);
            return mockRelatedEnd;
        }

        [Fact]
        public void InternalReferenceEntry_gets_current_value_from_RelatedEnd_if_navigation_property_has_been_removed_from_entity()
        {
            InternalReferenceEntry_gets_current_value_from_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation(new FakeEntity());
        }

        [Fact]
        public void InternalReferenceEntry_gets_null_from_RelatedEnd_if_navigation_property_has_been_removed_from_entity_and_no_value_is_set()
        {
            InternalReferenceEntry_gets_current_value_from_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation(null);
        }

        private void InternalReferenceEntry_gets_current_value_from_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation(FakeEntity relatedEntity)
        {
            var mockRelatedEnd = CreateMockRelatedReference(relatedEntity);

            var internalEntry = new InternalReferenceEntry(CreateMockInternalEntryForNavs(new FakeEntity(), mockRelatedEnd.Object, isDetached: false).Object, HiddenReferenceMetadata);

            var propValue = internalEntry.CurrentValue;

            Assert.Same(relatedEntity, propValue);
        }

        [Fact]
        public void InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity()
        {
            InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation(new FakeEntity(), new FakeEntity());
        }

        [Fact]
        public void InternalReferenceEntry_sets_null_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity()
        {
            InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation(new FakeEntity(), null);
        }

        [Fact]
        public void InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_when_current_value_is_null()
        {
            InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation(null, new FakeEntity());
        }

        [Fact]
        public void InternalReferenceEntry_sets_null_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_when_current_value_and_new_value_are_both_null()
        {
            InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation(null, null);
        }

        [Fact]
        public void InternalReferenceEntry_does_not_set_anything_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_when_current_value_and_new_value_are_same()
        {
            var relatedEntity = new FakeEntity();
            InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation(relatedEntity, relatedEntity);
        }

        /// <summary>
        /// Validates that the call to set a value on an <see cref="EntityReference{T}"/> is made as
        /// expected without mocking EntityReference (since it is sealed). This could be dones with Moq
        /// but it turned out easier and clearer this way.
        /// </summary>
        internal class FakeInternalReferenceEntry : InternalReferenceEntry
        {
            public FakeInternalReferenceEntry(InternalEntityEntry internalEntityEntry, NavigationEntryMetadata navigationMetadata)
                : base(internalEntityEntry, navigationMetadata)
            {
            }

            protected override void SetNavigationPropertyOnRelatedEnd(object value)
            {
                SetCount++;
                ValueSet = value;
            }

            public object ValueSet { get; set; }
            public int SetCount { get; set; }
        }

        private void InternalReferenceEntry_sets_current_value_onto_RelatedEnd_if_navigation_property_has_been_removed_from_entity_implementation(FakeEntity currentRelatedEntity, FakeEntity newRelatedEntity)
        {
            var mockRelatedEnd = CreateMockRelatedReference(currentRelatedEntity);

            var internalEntry = new FakeInternalReferenceEntry(CreateMockInternalEntryForNavs(new FakeEntity(), mockRelatedEnd.Object, isDetached: false).Object, HiddenReferenceMetadata);

            internalEntry.CurrentValue = newRelatedEntity;

            Assert.Equal(1, internalEntry.SetCount);
            Assert.Same(newRelatedEntity, internalEntry.ValueSet);
        }

        [Fact]
        public void InternalCollectionEntry_gets_current_value_that_is_the_RelatedEnd_if_navigation_property_has_been_removed_from_entity()
        {
            var relatedCollection = new EntityCollection<FakeEntity>();
            var internalEntry = new InternalCollectionEntry(CreateMockInternalEntryForNavs(new FakeEntity(), relatedCollection, isDetached: false).Object, HiddenCollectionMetadata);

            var propValue = internalEntry.CurrentValue;

            Assert.Same(relatedCollection, propValue);
        }

        [Fact]
        public void InternalCollectionEntry_does_nothing_if_attempting_to_set_the_actual_EntityCollection_as_a_current_value_when_navigation_property_has_been_removed_from_entity()
        {
            var relatedCollection = new EntityCollection<FakeEntity>();
            var internalEntry = new InternalCollectionEntry(CreateMockInternalEntryForNavs(new FakeEntity(), relatedCollection, isDetached: false).Object, HiddenCollectionMetadata);

            internalEntry.CurrentValue = relatedCollection; // Test that it doesn't throw
        }

        [Fact]
        public void InternalCollectionEntry_throws_when_attempting_to_set_a_new_collection_when_navigation_property_has_been_removed_from_entity()
        {
            var internalEntry = new InternalCollectionEntry(CreateMockInternalEntryForNavs(new FakeEntity(), new EntityCollection<FakeEntity>(), isDetached: false).Object, HiddenCollectionMetadata);

            Assert.Equal(Strings.DbCollectionEntry_CannotSetCollectionProp("HiddenCollection", typeof(FakeEntity).ToString()), Assert.Throws<NotSupportedException>(() => internalEntry.CurrentValue = new List<FakeEntity>()).Message);
        }

        [Fact]
        public void InternalReferenceEntry_throws_getting_current_value_from_detached_entity_if_navigation_property_has_been_removed()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, ReferenceMetadata);

            Assert.Equal(Strings.DbPropertyEntry_NotSupportedForDetached("CurrentValue", "Reference", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => { var _ = internalEntry.CurrentValue; }).Message);
        }

        [Fact]
        public void InternalReferenceEntry_throws_setting_current_value_from_detached_entity_if_navigation_property_has_been_removed()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, ReferenceMetadata);

            Assert.Equal(Strings.DbPropertyEntry_SettingEntityRefNotSupported("Reference", "FakeEntity", "Detached"), Assert.Throws<NotSupportedException>(() => { internalEntry.CurrentValue = null; }).Message);
        }

        [Fact]
        public void InternalCollectionEntry_throws_getting_current_value_from_detached_entity_if_navigation_property_has_been_removed()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, CollectionMetadata);

            Assert.Equal(Strings.DbPropertyEntry_NotSupportedForDetached("CurrentValue", "Collection", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => { var _ = internalEntry.CurrentValue; }).Message);
        }

        [Fact]
        public void InternalCollectionEntry_throws_setting_current_value_from_detached_entity_if_navigation_property_has_been_removed()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, CollectionMetadata);

            Assert.Equal(Strings.DbCollectionEntry_CannotSetCollectionProp("Collection", typeof(FakeEntity).ToString()), Assert.Throws<NotSupportedException>(() => { internalEntry.CurrentValue = null; }).Message);
        }

        #endregion

        #region Tests for Query, Load, IsLoaded, and Name on detached entities

        [Fact]
        public void InternalReferenceEntry_Load_throws_if_used_with_Detached_entity()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, ReferenceMetadata);

            Assert.Equal(Strings.DbPropertyEntry_NotSupportedForDetached("Load", "Reference", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => internalEntry.Load()).Message);
        }

        [Fact]
        public void InternalReferenceEntry_IsLoaded_throws_if_used_with_Detached_entity()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, ReferenceMetadata);

            Assert.Equal(Strings.DbPropertyEntry_NotSupportedForDetached("IsLoaded", "Reference", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => { var _ = internalEntry.IsLoaded; }).Message);
        }

        [Fact]
        public void InternalReferenceEntry_Query_throws_if_used_with_Detached_entity()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, ReferenceMetadata);

            Assert.Equal(Strings.DbPropertyEntry_NotSupportedForDetached("Query", "Reference", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => internalEntry.Query()).Message);
        }


        [Fact]
        public void InternalCollectionEntry_Load_throws_if_used_with_Detached_entity()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, CollectionMetadata);

            Assert.Equal(Strings.DbPropertyEntry_NotSupportedForDetached("Load", "Collection", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => internalEntry.Load()).Message);
        }

        [Fact]
        public void InternalCollectionEntry_IsLoaded_throws_if_used_with_Detached_entity()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, CollectionMetadata);

            Assert.Equal(Strings.DbPropertyEntry_NotSupportedForDetached("IsLoaded", "Collection", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => { var _ = internalEntry.IsLoaded; }).Message);
        }

        [Fact]
        public void InternalCollectionEntry_Query_throws_if_used_with_Detached_entity()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, CollectionMetadata);

            Assert.Equal(Strings.DbPropertyEntry_NotSupportedForDetached("Query", "Collection", "FakeEntity"), Assert.Throws<InvalidOperationException>(() => internalEntry.Query()).Message);
        }

        [Fact]
        public void InternalReferenceEntry_Name_works_even_when_used_with_Detached_entity()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalReferenceEntry(mockInternalEntry.Object, ReferenceMetadata);

            Assert.Equal("Reference", internalEntry.Name);
        }

        [Fact]
        public void InternalCollectionEntry_Name_works_even_when_used_with_Detached_entity()
        {
            var mockInternalEntry = CreateMockInternalEntryForNavs(new FakeEntity(), null, isDetached: true);
            var internalEntry = new InternalCollectionEntry(mockInternalEntry.Object, CollectionMetadata);

            Assert.Equal("Collection", internalEntry.Name);
        }

        #endregion

        #region Tests for access to nested property entries

        internal class InternalEntityPropertyEntryForMock : InternalEntityPropertyEntry
        {
            public InternalEntityPropertyEntryForMock()
                : base(CreateMockInternalEntry().Object, ComplexPropertyMetadata)
            {
            }
        }

        [Fact]
        public void Can_get_nested_property_entry_using_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property(e => e.ValueTypeProp);

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
        }

        [Fact]
        public void Can_get_nested_property_entry_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property("ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_generic_nested_property_entry_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property<int>("ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
        }

        [Fact]
        public void Can_get_nested_property_entry_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property("ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property(e => e.ComplexProp);

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property("ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_generic_nested_complex_property_entry_using_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property<FakeWithProps>("ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property("ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.ComplexProperty(e => e.ComplexProp);

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.ComplexProperty("ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_generic_nested_complex_property_entry_using_ComplexProperty_with_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.ComplexProperty<FakeWithProps>("ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.ComplexProperty("ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_nested_property_entry_using_dotted_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property(e => e.ComplexProp.ValueTypeProp);

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
        }

        [Fact]
        public void Can_get_nested_property_entry_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property("ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
        }

        [Fact]
        public void Can_get_generic_nested_property_entry_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property<int>("ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
        }

        [Fact]
        public void Can_get_nested_property_entry_using_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property("ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_dotted_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property(e => e.ComplexProp.ComplexProp);

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
        }

        [Fact]
        public void Can_get_generic_nested_complex_property_entry_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property<FakeWithProps>("ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
        }

        [Fact]
        public void Can_get_nested_property_entry_using_Member_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member("ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
        }

        [Fact]
        public void Can_get_generic_nested_property_entry_using_Member_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member<int>("ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbPropertyEntry<FakeWithProps, int>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
        }

        [Fact]
        public void Can_get_nested_property_entry_using_Member_with_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member("ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_Member_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
        }

        [Fact]
        public void Can_get_generic_nested_complex_property_entry_using_Member_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member<FakeWithProps>("ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_Member_with_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_dotted_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.ComplexProperty(e => e.ComplexProp.ComplexProp);

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.ComplexProperty("ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
        }

        [Fact]
        public void Can_get_generic_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.ComplexProperty<FakeWithProps>("ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Once());
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
        }

        [Fact]
        public void Can_get_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var nestedEntry = entityEntry.ComplexProperty("ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
        }

        [Fact]
        public void Can_get_double_nested_property_entry_using_dotted_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property(e => e.ComplexProp.ComplexProp.ValueTypeProp);

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
        }

        [Fact]
        public void Can_get_double_nested_property_entry_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
        }

        [Fact]
        public void Can_get_generic_double_nested_property_entry_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property<int>("ComplexProp.ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
        }

        [Fact]
        public void Can_get_double_nested_property_entry_using_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_using_dotted_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property(e => e.ComplexProp.ComplexProp.ComplexProp);

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
        }

        [Fact]
        public void Can_get_generic_double_nested_complex_property_entry_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property<FakeWithProps>("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_using_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Property("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
        }

        [Fact]
        public void Can_get_double_nested_property_entry_using_Member_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
        }

        [Fact]
        public void Can_get_generic_double_nested_property_entry_using_Member_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member<int>("ComplexProp.ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbPropertyEntry<FakeWithProps, int>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)), Times.Once());
        }

        [Fact]
        public void Can_get_double_nested_property_entry_using_Member_with_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)), Times.Once());
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_using_Member_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
        }

        [Fact]
        public void Can_get_generic_double_nested_complex_property_entry_using_Member_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member<FakeWithProps>("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_using_Member_with_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.Member("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_using_ComplexProperty_with_dotted_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.ComplexProperty(e => e.ComplexProp.ComplexProp.ComplexProp);

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.ComplexProperty("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
        }

        [Fact]
        public void Can_get_generic_double_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry<FakeWithProps>(mockInternalEntry.Object);

            var nestedEntry = entityEntry.ComplexProperty<FakeWithProps>("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)), Times.Once());
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_using_ComplexProperty_with_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var entityEntry = new DbEntityEntry(mockInternalEntry.Object);

            var nestedEntry = entityEntry.ComplexProperty("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
        }

        [Fact]
        public void Can_get_double_nested_property_entry_from_DbComplexProperty_using_dotted_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property(e => e.ComplexProp.ComplexProp.ValueTypeProp);

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
        }

        [Fact]
        public void Can_get_double_nested_property_entry_from_DbComplexProperty_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property("ComplexProp.ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_generic_double_nested_property_entry_from_DbComplexProperty_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property<int>("ComplexProp.ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(int)));
        }

        [Fact]
        public void Can_get_double_nested_property_entry_from_DbComplexProperty_using_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property("ComplexProp.ComplexProp.ValueTypeProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ValueTypeProp", typeof(FakeWithProps), typeof(object)));
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_dotted_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property(e => e.ComplexProp.ComplexProp.ComplexProp);

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
        }

        [Fact]
        public void Can_get_generic_double_nested_complex_property_entry_from_DbComplexProperty_using_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property<FakeWithProps>("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.Property("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            Assert.IsType<DbComplexPropertyEntry>(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_ComplexProperty_with_dotted_lambda_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.ComplexProperty(e => e.ComplexProp.ComplexProp.ComplexProp);

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_ComplexProperty_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.ComplexProperty("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
        }

        [Fact]
        public void Can_get_generic_double_nested_complex_property_entry_from_DbComplexProperty_using_ComplexProperty_with_dotted_string_on_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.ComplexProperty<FakeWithProps>("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(2));
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(FakeWithProps)));
        }

        [Fact]
        public void Can_get_double_nested_complex_property_entry_from_DbComplexProperty_using_ComplexProperty_with_dotted_string_on_non_generic_API()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var nestedEntry = propEntry.ComplexProperty("ComplexProp.ComplexProp.ComplexProp");

            Assert.NotNull(nestedEntry);
            mockInternalEntry.Verify(e => e.ValidateAndGetPropertyMetadata("ComplexProp", typeof(FakeWithProps), typeof(object)), Times.Exactly(3));
        }

        [Fact]
        public void Using_Reference_with_dotted_lamda_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.RefTypeProp"), Assert.Throws<ArgumentException>(() => entityEntry.Reference(e => e.ComplexProp.RefTypeProp)).Message);
        }

        [Fact]
        public void Using_Reference_with_dotted_string_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.RefTypeProp"), Assert.Throws<ArgumentException>(() => entityEntry.Reference("ComplexProp.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_Reference_with_dotted_generic_string_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.RefTypeProp"), Assert.Throws<ArgumentException>(() => entityEntry.Reference<string>("ComplexProp.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_non_generic_Reference_with_dotted_string_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.RefTypeProp"), Assert.Throws<ArgumentException>(() => entityEntry.Reference("ComplexProp.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_Collection_with_dotted_lamda_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.Collection"), Assert.Throws<ArgumentException>(() => entityEntry.Collection(e => e.ComplexProp.Collection)).Message);
        }

        [Fact]
        public void Using_Collection_with_dotted_string_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.Collection"), Assert.Throws<ArgumentException>(() => entityEntry.Collection("ComplexProp.Collection")).Message);
        }

        [Fact]
        public void Using_Collection_with_dotted_generic_string_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.Collection"), Assert.Throws<ArgumentException>(() => entityEntry.Collection<FakeWithProps>("ComplexProp.Collection")).Message);
        }

        [Fact]
        public void Using_non_generic_Collection_with_dotted_string_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPathMustBeProperty("ComplexProp.Collection"), Assert.Throws<ArgumentException>(() => entityEntry.Collection("ComplexProp.Collection")).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_lamda_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property(e => e.RefTypeProp.Length)).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_string_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_generic_string_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_non_generic_Property_with_dotted_string_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_lamda_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property(e => e.Reference.Id)).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_string_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property("Reference.Id")).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_generic_string_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("Reference.Id")).Message);
        }

        [Fact]
        public void Using_non_generic_Property_with_dotted_string_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property("Reference.Id")).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_lamda_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property(e => e.Collection.Count)).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_string_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property("Collection.Count")).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_generic_string_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("Collection.Count")).Message);
        }

        [Fact]
        public void Using_non_generic_Property_with_dotted_string_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property("Collection.Count")).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_string_containing_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_generic_string_containing_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_non_generic_Property_with_dotted_string_containing_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_string_ending_in_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_Property_with_dotted_generic_string_ending_in_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property<string>("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_non_generic_Property_with_dotted_string_ending_in_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Property("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_Member_with_dotted_string_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_Member_with_dotted_generic_string_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_non_generic_Member_with_dotted_string_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_Member_with_dotted_string_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member("Reference.Id")).Message);
        }

        [Fact]
        public void Using_Member_with_dotted_generic_string_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("Reference.Id")).Message);
        }

        [Fact]
        public void Using_non_generic_Member_with_dotted_string_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member("Reference.Id")).Message);
        }

        [Fact]
        public void Using_Member_with_dotted_string_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member("Collection.Count")).Message);
        }

        [Fact]
        public void Using_Member_with_dotted_generic_string_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("Collection.Count")).Message);
        }

        [Fact]
        public void Using_non_generic_Member_with_dotted_string_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member("Collection.Count")).Message);
        }

        [Fact]
        public void Using_Member_with_dotted_string_containing_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_Member_with_dotted_generic_string_containing_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_non_generic_Member_with_dotted_string_containing_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_Member_with_dotted_string_ending_in_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_Member_with_dotted_generic_string_ending_in_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member<string>("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_non_generic_Member_with_dotted_string_ending_in_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.Member("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_lamda_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(e => e.RefTypeProp.Length)).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_string_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_generic_string_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_with_dotted_string_containing_scalar_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_lamda_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(e => e.Reference.Id)).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Reference.Id")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_generic_string_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("Reference.Id")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Reference.Id")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_lamda_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(e => e.Collection.Count)).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Collection.Count")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_generic_string_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("Collection.Count")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Collection.Count")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_generic_string_containing_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_generic_string_ending_in_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_lambda_ending_in_scalar_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty(e => e.ComplexProp.RefTypeProp)).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_string_ending_in_scalar_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("ComplexProp.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_with_dotted_generic_string_ending_in_scalar_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty<string>("ComplexProp.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_with_dotted_string_ending_in_scalar_prop_name_throws()
        {
            var entityEntry = new DbEntityEntry(CreateMockInternalEntry().Object);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => entityEntry.ComplexProperty("ComplexProp.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_lamda_containing_scalar_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property(e => e.RefTypeProp.Length)).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_string_containing_scalar_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_generic_string_containing_scalar_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property<string>("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_non_generic_Property_on_DbComplexProperty_with_dotted_string_containing_scalar_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_lamda_containing_reference_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property(e => e.Reference.Id)).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property("Reference.Id")).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_generic_string_containing_reference_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property<string>("Reference.Id")).Message);
        }

        [Fact]
        public void Using_non_generic_Property_on_DbComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property("Reference.Id")).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_lamda_containing_collection_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property(e => e.Collection.Count)).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property("Collection.Count")).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_generic_string_containing_collection_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property<string>("Collection.Count")).Message);
        }

        [Fact]
        public void Using_non_generic_Property_on_DbComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property("Collection.Count")).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_generic_string_containing_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property<string>("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_non_generic_Property_on_DbComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_Property_on_DbComplexProperty_with_dotted_generic_string_ending_in_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property<string>("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_non_generic_Property_on_DbComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_NotAScalarProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.Property("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_lamda_containing_scalar_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(e => e.RefTypeProp.Length)).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_scalar_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_containing_scalar_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_scalar_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("RefTypeProp", "RefTypeProp.Length", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("RefTypeProp.Length")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_lamda_containing_reference_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(e => e.Reference.Id)).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Reference.Id")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_containing_reference_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("Reference.Id")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_reference_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Reference", "Reference.Id", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Reference.Id")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_lamda_containing_collection_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(e => e.Collection.Count)).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Collection.Count")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_containing_collection_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("Collection.Count")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_collection_nav_prop_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Collection", "Collection.Count", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Collection.Count")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_containing_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_containing_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_DottedPartNotComplex("Missing", "Missing.RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("Missing.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_ending_in_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_ending_in_missing_prop_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("Missing", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("ComplexProp.Missing")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_lambda_ending_in_scalar_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(e => e.ComplexProp.RefTypeProp)).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_string_ending_in_scalar_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("ComplexProp.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_ComplexProperty_on_DbComplexProperty_with_dotted_generic_string_ending_in_scalar_prop_name_throws()
        {
            var propEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<string>("ComplexProp.RefTypeProp")).Message);
        }

        [Fact]
        public void Using_non_generic_ComplexProperty_on_DbComplexProperty_with_dotted_string_ending_in_scalar_prop_name_throws()
        {
            var propEntry = new DbEntityEntry(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbEntityEntry_NotAComplexProperty("RefTypeProp", "FakeWithProps"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("ComplexProp.RefTypeProp")).Message);
        }

        [Fact]
        public void Passing_null_expression_to_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>().Object);

            Assert.Equal("property", Assert.Throws<ArgumentNullException>(() => propEntry.Property((Expression<Func<FakeWithProps, string>>)null)).ParamName);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.Property((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.Property("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.Property(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_generic_method_on_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock> { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.Property<int>((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_method_on_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock> { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.Property<int>("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_method_on_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock> { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.Property<int>(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_non_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.Property((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_non_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.Property("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_non_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.Property(" ")).Message);
        }

        [Fact]
        public void Passing_bad_expression_to_generic_DbPropertyEntry_Property_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Property", "FakeWithProps"), "property").Message, Assert.Throws<ArgumentException>(() => propEntry.Property(e => new FakeEntity())).Message);
        }

        [Fact]
        public void Passing_null_expression_to_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>().Object);

            Assert.Equal("property", Assert.Throws<ArgumentNullException>(() => propEntry.ComplexProperty((Expression<Func<FakeWithProps, string>>)null)).ParamName);
        }

        [Fact]
        public void Passing_null_string_to_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_generic_method_on_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock> { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<int>((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_generic_method_on_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock> { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<int>("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_generic_method_on_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock> { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty<int>(" ")).Message);
        }

        [Fact]
        public void Passing_null_string_to_non_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty((string)null)).Message);
        }

        [Fact]
        public void Passing_empty_string_to_non_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty("")).Message);
        }

        [Fact]
        public void Passing_whitespace_string_to_non_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("propertyName"), Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(" ")).Message);
        }

        [Fact]
        public void Passing_bad_expression_to_generic_DbPropertyEntry_ComplexProperty_throws()
        {
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new Mock<InternalEntityPropertyEntryForMock>() { CallBase = true }.Object);

            Assert.Equal(new ArgumentException(Strings.DbEntityEntry_BadPropertyExpression("Property", "FakeWithProps"), "property").Message, Assert.Throws<ArgumentException>(() => propEntry.ComplexProperty(e => new FakeEntity())).Message);
        }

        #endregion

        #region Tests for back references from member entries to the entity entry and parent property entry

        [Fact]
        public void EntityEntity_can_be_obtained_from_generic_DbReferenceEntry_back_reference()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            var backEntry = entityEntry.Reference(e => e.Reference).EntityEntry;

            Assert.Same(entityEntry.Entity, backEntry.Entity);
        }

        [Fact]
        public void EntityEntity_can_be_obtained_from_non_generic_DbReferenceEntry_back_reference()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            var backEntry = entityEntry.Reference("Reference").EntityEntry;

            Assert.Same(entityEntry.Entity, backEntry.Entity);
        }

        [Fact]
        public void EntityEntity_can_be_obtained_from_generic_DbCollectionEntry_back_reference()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            var backEntry = entityEntry.Collection(e => e.Collection).EntityEntry;

            Assert.Same(entityEntry.Entity, backEntry.Entity);
        }

        [Fact]
        public void EntityEntity_can_be_obtained_from_non_generic_DbCollectionEntry_back_reference()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            var backEntry = entityEntry.Collection("Collection").EntityEntry;

            Assert.Same(entityEntry.Entity, backEntry.Entity);
        }

        [Fact]
        public void EntityEntity_can_be_obtained_from_generic_DbPropertyEntry_back_reference()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            var backEntry = entityEntry.Property(e => e.ValueTypeProp).EntityEntry;

            Assert.Same(entityEntry.Entity, backEntry.Entity);
        }

        [Fact]
        public void EntityEntity_can_be_obtained_from_non_generic_DbPropertyEntry_back_reference()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            var backEntry = entityEntry.Property("ValueTypeProp").EntityEntry;

            Assert.Same(entityEntry.Entity, backEntry.Entity);
        }

        [Fact]
        public void EntityEntity_can_be_obtained_from_nested_generic_DbPropertyEntry_back_reference()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var backEntry = propEntry.Property(e => e.ValueTypeProp).EntityEntry;

            Assert.Same(mockInternalEntry.Object.Entity, backEntry.Entity);
        }

        [Fact]
        public void EntityEntity_can_be_obtained_from_nested_non_generic_DbPropertyEntry_back_reference()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var backEntry = propEntry.Property("ValueTypeProp").EntityEntry;

            Assert.Same(mockInternalEntry.Object.Entity, backEntry.Entity);
        }

        [Fact]
        public void Parent_PropertyEntity_can_be_obtained_from_nested_generic_DbPropertyEntry_back_reference()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var backEntry = propEntry.Property(e => e.ValueTypeProp).ParentProperty;

            Assert.Same(propEntry.Name, backEntry.Name);
        }

        [Fact]
        public void Parent_PropertyEntity_can_be_obtained_from_nested_non_generic_DbPropertyEntry_back_reference()
        {
            var mockInternalEntry = CreateMockInternalEntry();
            var propEntry = new DbComplexPropertyEntry<FakeWithProps, FakeWithProps>(new InternalEntityPropertyEntry(mockInternalEntry.Object, ComplexPropertyMetadata));

            var backEntry = propEntry.Property("ValueTypeProp").ParentProperty;

            Assert.Same(propEntry.Name, backEntry.Name);
        }

        [Fact]
        public void Parent_PropertyEntity_returns_null_for_non_nested_generic_DbPropertyEntry_back_reference()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            var backEntry = entityEntry.Property(e => e.ValueTypeProp).ParentProperty;

            Assert.Null(backEntry);
        }

        [Fact]
        public void Parent_PropertyEntity_returns_null_for_non_nested_non_generic_DbPropertyEntry_back_reference()
        {
            var entityEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object);

            var backEntry = entityEntry.Property("ValueTypeProp").ParentProperty;

            Assert.Null(backEntry);
        }

        #endregion

        #region Conversions between generic and non-generic

        [Fact]
        public void Generic_DbMemberEntry_for_collection_can_be_converted_to_non_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member<ICollection<FakeEntity>>("Collection");

            var nonGeneric = ImplicitConvert(memberEntry);

            Assert.IsType<DbCollectionEntry>(nonGeneric);
            Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
        }

        [Fact]
        public void Generic_DbMemberEntry_for_reference_can_be_converted_to_non_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member<FakeEntity>("Reference");

            var nonGeneric = ImplicitConvert(memberEntry);

            Assert.IsType<DbReferenceEntry>(nonGeneric);
            Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
        }

        [Fact]
        public void Generic_DbMemberEntry_for_property_can_be_converted_to_non_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member<int>("ValueTypeProp");

            var nonGeneric = ImplicitConvert(memberEntry);

            Assert.IsType<DbPropertyEntry>(nonGeneric);
            Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
        }

        [Fact]
        public void Generic_DbMemberEntry_for_complex_property_can_be_converted_to_non_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member<FakeWithProps>("ComplexProp");

            var nonGeneric = ImplicitConvert(memberEntry);

            Assert.IsType<DbComplexPropertyEntry>(nonGeneric);
            Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
        }
        
        private static DbMemberEntry ImplicitConvert(DbMemberEntry nonGeneric)
        {
            return nonGeneric;
        }

        [Fact]
        public void Generic_DbPropertyEntry_for_property_can_be_converted_to_non_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property(e => e.ValueTypeProp);

            var nonGeneric = ImplicitConvert(memberEntry);

            Assert.IsType<DbPropertyEntry>(nonGeneric);
            Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
        }

        [Fact]
        public void Generic_DbPropertyEntry_for_complex_property_can_be_converted_to_non_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property(e => e.ComplexProp);

            var nonGeneric = ImplicitConvert(memberEntry);

            Assert.IsType<DbComplexPropertyEntry>(nonGeneric);
            Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
        }

        private static DbPropertyEntry ImplicitConvert(DbPropertyEntry nonGeneric)
        {
            return nonGeneric;
        }

        [Fact]
        public void Generic_DbComplexPropertyEntry_for_complex_property_can_be_converted_to_non_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty(e => e.ComplexProp);

            var nonGeneric = ImplicitConvert(memberEntry);

            Assert.IsType<DbComplexPropertyEntry>(nonGeneric);
            Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
        }

        private static DbComplexPropertyEntry ImplicitConvert(DbComplexPropertyEntry nonGeneric)
        {
            return nonGeneric;
        }

        [Fact]
        public void Generic_DbCollectionEntry_for_collection_can_be_converted_to_non_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Collection(e => e.Collection);

            var nonGeneric = ImplicitConvert(memberEntry);

            Assert.IsType<DbCollectionEntry>(nonGeneric);
            Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
        }

        private static DbCollectionEntry ImplicitConvert(DbCollectionEntry nonGeneric)
        {
            return nonGeneric;
        }

        [Fact]
        public void Generic_DbReferenceEntry_for_reference_can_be_converted_to_non_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Reference(e => e.Reference);

            var nonGeneric = ImplicitConvert(memberEntry);

            Assert.IsType<DbReferenceEntry>(nonGeneric);
            Assert.Same(memberEntry.InternalMemberEntry, nonGeneric.InternalMemberEntry);
        }

        private static DbReferenceEntry ImplicitConvert(DbReferenceEntry nonGeneric)
        {
            return nonGeneric;
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_collection_can_be_converted_to_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("Collection");

            var generic = memberEntry.Cast<FakeWithProps, ICollection<FakeEntity>>();

            Assert.IsType<DbCollectionEntry<FakeWithProps, FakeEntity>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_reference_can_be_converted_to_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("Reference");

            var generic = memberEntry.Cast<FakeWithProps, FakeEntity>();

            Assert.IsType<DbReferenceEntry<FakeWithProps, FakeEntity>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_property_can_be_converted_to_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("ValueTypeProp");

            var generic = memberEntry.Cast<FakeWithProps, int>();

            Assert.IsType<DbPropertyEntry<FakeWithProps, int>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_complex_property_can_be_converted_to_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("ComplexProp");

            var generic = memberEntry.Cast<FakeWithProps, FakeWithProps>();

            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbPropertyEntry_for_property_can_be_converted_to_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property("ValueTypeProp");

            var generic = memberEntry.Cast<FakeWithProps, int>();

            Assert.IsType<DbPropertyEntry<FakeWithProps, int>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbPropertyEntry_for_complex_property_can_be_converted_to_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property("ComplexProp");

            var generic = memberEntry.Cast<FakeWithProps, FakeWithProps>();

            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbComplexPropertyEntry_for_complex_property_can_be_converted_to_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            var generic = memberEntry.Cast<FakeWithProps, FakeWithProps>();

            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, FakeWithProps>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbCollectionEntry_for_collection_can_be_converted_to_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Collection("Collection");

            var generic = memberEntry.Cast<FakeWithProps, FakeEntity>();

            Assert.IsType<DbCollectionEntry<FakeWithProps, FakeEntity>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbReferenceEntry_for_reference_can_be_converted_to_generic_version()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Reference("Reference");

            var generic = memberEntry.Cast<FakeWithProps, FakeEntity>();

            Assert.IsType<DbReferenceEntry<FakeWithProps, FakeEntity>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_collection_can_be_converted_to_generic_version_of_base_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("Collection");

            var generic = memberEntry.Cast<object, ICollection<FakeEntity>>();

            Assert.IsType<DbCollectionEntry<object, FakeEntity>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_reference_can_be_converted_to_generic_version_of_base_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("Reference");

            var generic = memberEntry.Cast<object, FakeEntity>();

            Assert.IsType<DbReferenceEntry<object, FakeEntity>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_property_can_be_converted_to_generic_version_of_base_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("ValueTypeProp");

            var generic = memberEntry.Cast<object, int>();

            Assert.IsType<DbPropertyEntry<object, int>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_complex_property_can_be_converted_to_generic_version_of_base_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("ComplexProp");

            var generic = memberEntry.Cast<object, FakeWithProps>();

            Assert.IsType<DbComplexPropertyEntry<object, FakeWithProps>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbPropertyEntry_for_property_can_be_converted_to_generic_version_of_base_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property("ValueTypeProp");

            var generic = memberEntry.Cast<object, int>();

            Assert.IsType<DbPropertyEntry<object, int>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbPropertyEntry_for_complex_property_can_be_converted_to_generic_version_of_base_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property("ComplexProp");

            var generic = memberEntry.Cast<object, FakeWithProps>();

            Assert.IsType<DbComplexPropertyEntry<object, FakeWithProps>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbComplexPropertyEntry_for_complex_property_can_be_converted_to_generic_version_of_base_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            var generic = memberEntry.Cast<object, FakeWithProps>();

            Assert.IsType<DbComplexPropertyEntry<object, FakeWithProps>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbCollectionEntry_for_collection_can_be_converted_to_generic_version_of_base_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Collection("Collection");

            var generic = memberEntry.Cast<object, FakeEntity>();

            Assert.IsType<DbCollectionEntry<object, FakeEntity>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbReferenceEntry_for_reference_can_be_converted_to_generic_version_of_base_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Reference("Reference");

            var generic = memberEntry.Cast<object, FakeEntity>();

            Assert.IsType<DbReferenceEntry<object, FakeEntity>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_collection_cannot_be_converted_to_generic_version_of_base_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("Collection");

            // This cast fails because an ICollection<FakeEntity> is not an IColletion<object>.
            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbMemberEntry).Name, typeof(FakeWithProps).Name, typeof(ICollection<object>).Name, typeof(FakeWithProps).Name, typeof(ICollection<FakeEntity>).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, ICollection<object>>()).Message);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_reference_can_be_converted_to_generic_version_of_base_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("Reference");

            var generic = memberEntry.Cast<FakeWithProps, object>();

            Assert.IsType<DbReferenceEntry<FakeWithProps, object>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_property_can_be_converted_to_generic_version_of_base_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("ValueTypeProp");

            var generic = memberEntry.Cast<FakeWithProps, object>();

            Assert.IsType<DbPropertyEntry<FakeWithProps, object>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_complex_property_can_be_converted_to_generic_version_of_base_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("ComplexProp");

            var generic = memberEntry.Cast<FakeWithProps, object>();

            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, object>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbPropertyEntry_for_property_can_be_converted_to_generic_version_of_base_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property("ValueTypeProp");

            var generic = memberEntry.Cast<FakeWithProps, object>();

            Assert.IsType<DbPropertyEntry<FakeWithProps, object>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbPropertyEntry_for_complex_property_can_be_converted_to_generic_version_of_base_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property("ComplexProp");

            var generic = memberEntry.Cast<FakeWithProps, object>();

            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, object>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbComplexPropertyEntry_for_complex_property_can_be_converted_to_generic_version_of_base_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            var generic = memberEntry.Cast<FakeWithProps, object>();

            Assert.IsType<DbComplexPropertyEntry<FakeWithProps, object>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbCollectionEntry_for_collection_can_be_converted_to_generic_version_of_base_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Collection("Collection");

            var generic = memberEntry.Cast<FakeWithProps, object>();

            Assert.IsType<DbCollectionEntry<FakeWithProps, object>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbReferenceEntry_for_reference_can_be_converted_to_generic_version_of_base_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Reference("Reference");

            var generic = memberEntry.Cast<FakeWithProps, object>();

            Assert.IsType<DbReferenceEntry<FakeWithProps, object>>(generic);
            Assert.Same(memberEntry.InternalMemberEntry, generic.InternalMemberEntry);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_collection_cannot_be_converted_to_generic_version_of_derived_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("Collection");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbMemberEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(ICollection<FakeEntity>).Name, typeof(FakeWithProps).Name, typeof(ICollection<FakeEntity>).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, ICollection<FakeEntity>>()).Message);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_reference_cannot_be_converted_to_generic_version_of_derived_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("Reference");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbMemberEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeEntity).Name, typeof(FakeWithProps).Name, typeof(FakeEntity).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeEntity>()).Message);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_property_cannot_be_converted_to_generic_version_of_derived_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("ValueTypeProp");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbMemberEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(int).Name, typeof(FakeWithProps).Name, typeof(int).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, int>()).Message);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("ComplexProp");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbMemberEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeWithProps).Name, typeof(FakeWithProps).Name, typeof(FakeWithProps).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeWithProps>()).Message);
        }

        [Fact]
        public void Non_generic_DbPropertyEntry_for_property_cannot_be_converted_to_generic_version_of_derived_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property("ValueTypeProp");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbPropertyEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(int).Name, typeof(FakeWithProps).Name, typeof(int).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, int>()).Message);
        }

        [Fact]
        public void Non_generic_DbPropertyEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property("ComplexProp");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbPropertyEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeWithProps).Name, typeof(FakeWithProps).Name, typeof(FakeWithProps).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeWithProps>()).Message);
        }

        [Fact]
        public void Non_generic_DbComplexPropertyEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbComplexPropertyEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeWithProps).Name, typeof(FakeWithProps).Name, typeof(FakeWithProps).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeWithProps>()).Message);
        }

        [Fact]
        public void Non_generic_DbCollectionEntry_for_collection_cannot_be_converted_to_generic_version_of_derived_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Collection("Collection");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbCollectionEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeEntity).Name, typeof(FakeWithProps).Name, typeof(FakeEntity).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeEntity>()).Message);
        }

        [Fact]
        public void Non_generic_DbReferenceEntry_for_reference_cannot_be_converted_to_generic_version_of_derived_entity_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Reference("Reference");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbReferenceEntry).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeEntity).Name, typeof(FakeWithProps).Name, typeof(FakeEntity).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<DerivedFakeWithProps, FakeEntity>()).Message);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_collection_cannot_be_converted_to_generic_version_of_derived_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("Collection");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbMemberEntry).Name, typeof(FakeWithProps).Name, typeof(ICollection<FakeDerivedEntity>).Name, typeof(FakeWithProps).Name, typeof(ICollection<FakeEntity>).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, ICollection<FakeDerivedEntity>>()).Message);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_reference_cannot_be_converted_to_generic_version_of_derived_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("Reference");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbMemberEntry).Name, typeof(FakeWithProps).Name, typeof(FakeDerivedEntity).Name, typeof(FakeWithProps).Name, typeof(FakeEntity).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, FakeDerivedEntity>()).Message);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_property_cannot_be_converted_to_generic_version_of_bad_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("ValueTypeProp");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbMemberEntry).Name, typeof(FakeWithProps).Name, typeof(string).Name, typeof(FakeWithProps).Name, typeof(int).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, string>()).Message);
        }

        [Fact]
        public void Non_generic_DbMemberEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Member("ComplexProp");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbMemberEntry).Name, typeof(FakeWithProps).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeWithProps).Name, typeof(FakeWithProps).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, DerivedFakeWithProps>()).Message);
        }

        [Fact]
        public void Non_generic_DbPropertyEntry_for_property_cannot_be_converted_to_generic_version_of_bad_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property("ValueTypeProp");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbPropertyEntry).Name, typeof(FakeWithProps).Name, typeof(short).Name, typeof(FakeWithProps).Name, typeof(int).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, short>()).Message);
        }

        [Fact]
        public void Non_generic_DbPropertyEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Property("ComplexProp");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbPropertyEntry).Name, typeof(FakeWithProps).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeWithProps).Name, typeof(FakeWithProps).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, DerivedFakeWithProps>()).Message);
        }

        [Fact]
        public void Non_generic_DbComplexPropertyEntry_for_complex_property_cannot_be_converted_to_generic_version_of_derived_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).ComplexProperty("ComplexProp");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbComplexPropertyEntry).Name, typeof(FakeWithProps).Name, typeof(DerivedFakeWithProps).Name, typeof(FakeWithProps).Name, typeof(FakeWithProps).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, DerivedFakeWithProps>()).Message);
        }

        [Fact]
        public void Non_generic_DbCollectionEntry_for_collection_cannot_be_converted_to_generic_version_of_derived_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Collection("Collection");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbCollectionEntry).Name, typeof(FakeWithProps).Name, typeof(FakeDerivedEntity).Name, typeof(FakeWithProps).Name, typeof(FakeEntity).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, FakeDerivedEntity>()).Message);
        }

        [Fact]
        public void Non_generic_DbReferenceEntry_for_reference_cannot_be_converted_to_generic_version_of_derived_property_type()
        {
            var memberEntry = new DbEntityEntry<FakeWithProps>(CreateMockInternalEntry().Object).Reference("Reference");

            Assert.Equal(Strings.DbMember_BadTypeForCast(typeof(DbReferenceEntry).Name, typeof(FakeWithProps).Name, typeof(FakeDerivedEntity).Name, typeof(FakeWithProps).Name, typeof(FakeEntity).Name), Assert.Throws<InvalidCastException>(() => memberEntry.Cast<FakeWithProps, FakeDerivedEntity>()).Message);
        }

        #endregion
    }
}
