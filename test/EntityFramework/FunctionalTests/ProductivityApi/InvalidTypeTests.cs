namespace ProductivityApiTests
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Linq;
    using System.Reflection;
    using AdvancedPatternsModel;
    using InvalidTypeModel;
    using Microsoft.CSharp;
    using SimpleModel;
    using Xunit;

    public class InvalidTypeTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        /// <summary>
        /// Asserts that an operation on a set throws if the type of the set is not valid for the model.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity for which a set should be created.</typeparam>
        /// <param name="test">The test to run on the set.</param>
        private void ThrowsForInvalidType<TEntity>(Action<IDbSet<TEntity>> test) where TEntity : class
        {
            using (var context = new SimpleModelContext())
            {
                var set = context.Set<TEntity>();
                Assert.Throws<InvalidOperationException>(() => test(set)).ValidateMessage("DbSet_EntityTypeNotInModel",
                                                                                          typeof(TEntity).Name);
            }
        }

        /// <summary>
        /// Asserts that an operation on a set throws if the type of the set is not valid for the model.
        /// </summary>
        /// <param name="entityType">The type of entity for which the set is for.</param>
        /// <param name="test">The test to run on the set.</param>
        private void ThrowsForInvalidTypeNonGeneric(Type entityType, Action<DbSet> test)
        {
            using (var context = new SimpleModelContext())
            {
                var set = context.Set(entityType);
                Assert.Throws<InvalidOperationException>(() => test(set)).ValidateMessage("DbSet_EntityTypeNotInModel",
                                                                                          entityType.Name);
            }
        }

        #endregion

        #region Invalid type tests

        [Fact]
        public void Set_throws_only_when_used_if_type_not_in_the_model()
        {
            ThrowsForInvalidType<Login>(set => set.FirstOrDefault());
        }

        [Fact]
        public void Non_generic_Set_throws_only_when_used_if_type_not_in_the_model()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(Login), set => set.Cast<Login>().FirstOrDefault());
        }

        [Fact]
        public void Non_generic_Set_throws_when_attempting_to_create_set_for_value_type()
        {
            using (var context = new SimpleModelContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Set(typeof(int))).ValidateMessage(
                    "DbSet_EntityTypeNotInModel", typeof(int).Name);
            }
        }

        [Fact]
        public void Non_generic_Set_throws_when_attempting_to_create_set_for_special_type()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(String), set => set.Cast<String>().FirstOrDefault());
        }

        [Fact]
        public void Set_throws_only_when_used_if_type_is_unmapped_base_type()
        {
            using (var ctx = new AdvancedPatternsMasterContext())
            {
                var set = ctx.Set<UnMappedPersonBase>();

                Assert.Throws<InvalidOperationException>(() => set.FirstOrDefault()).ValidateMessage(
                    "DbSet_EntityTypeNotInModel", typeof(UnMappedPersonBase).Name);
            }
        }

        [Fact]
        public void Non_generic_Set_throws_only_when_used_if_type_is_unmapped_base_type()
        {
            using (var ctx = new AdvancedPatternsMasterContext())
            {
                var set = ctx.Set(typeof(UnMappedPersonBase));
                Assert.Throws<InvalidOperationException>(() => set.Cast<UnMappedPersonBase>().FirstOrDefault()).
                    ValidateMessage("DbSet_EntityTypeNotInModel", typeof(UnMappedPersonBase).Name);
            }
        }

        [Fact]
        public void Set_throws_only_when_used_if_type_is_unmapped_abstract_base_type()
        {
            using (var ctx = new AdvancedPatternsMasterContext())
            {
                var set = ctx.Set<UnMappedOfficeBase>();

                Assert.Throws<InvalidOperationException>(() => set.FirstOrDefault()).ValidateMessage(
                    "DbSet_EntityTypeNotInModel", typeof(UnMappedOfficeBase).Name);
            }
        }

        [Fact]
        public void Set_throws_only_when_used_if_type_derives_from_valid_type()
        {
            // Create a derived type in a new assembly
            var provider = new CSharpCodeProvider();
            var result = provider.CompileAssemblyFromSource(
                new CompilerParameters(new string[] { this.GetType().Assembly.Location }),
                "public class DerivedCategory : SimpleModel.Category { }");

            var derivedCategoryType = result.CompiledAssembly.GetTypes().Single();
            var dbContextSet = typeof(DbContext).GetMethod("Set", Type.EmptyTypes);
            var dbContextSetOfDerivedCategory = dbContextSet.MakeGenericMethod(derivedCategoryType);

            using (var ctx = new AdvancedPatternsMasterContext())
            {
                // Create a set for the new type
                var set = dbContextSetOfDerivedCategory.Invoke(ctx, null);
                var iQueryableFirstOrDefault = typeof(System.Linq.Queryable)
                    .GetMethods()
                    .Single(m => m.Name == "FirstOrDefault" && m.GetParameters().Count() == 1);
                var iQueryableOfDerivedCategoryFirstOrDefault =
                    iQueryableFirstOrDefault.MakeGenericMethod(derivedCategoryType);

                // Attempt to use the set
                try
                {
                    iQueryableOfDerivedCategoryFirstOrDefault.Invoke(null, new object[] { set });
                }
                catch (TargetInvocationException ex)
                {
                    Assert.IsType<InvalidOperationException>(ex.InnerException);
                    new StringResourceVerifier(new AssemblyResourceLookup(typeof(DbModelBuilder).Assembly,
                                                                          "System.Data.Entity.Properties.Resources"))
                        .VerifyMatch("DbSet_EntityTypeNotInModel", ex.InnerException.Message,
                                     new[] { "DerivedCategory" });
                }
            }
        }

        [Fact]
        public void Set_throws_only_when_used_if_type_is_a_complex_type_Dev10_885806()
        {
            using (var ctx = new AdvancedPatternsMasterContext())
            {
                var set = ctx.Set<Address>();
                Assert.Throws<InvalidOperationException>(() => set.FirstOrDefault()).ValidateMessage(
                    "DbSet_DbSetUsedWithComplexType", typeof(Address).Name);
            }
        }

        [Fact]
        public void Non_generic_Set_throws_only_when_used_if_type_is_a_complex_type_Dev10_885806()
        {
            using (var ctx = new AdvancedPatternsMasterContext())
            {
                var set = ctx.Set(typeof(Address));
                Assert.Throws<InvalidOperationException>(() => set.Cast<Address>().FirstOrDefault()).ValidateMessage(
                    "DbSet_DbSetUsedWithComplexType", typeof(Address).Name);
            }
        }

        [Fact]
        public void Set_throws_only_when_used_if_type_not_in_the_model_even_if_type_is_interface()
        {
            ThrowsForInvalidType<ICollection>(set => set.FirstOrDefault());
        }

        [Fact]
        public void Non_generic_Set_throws_only_when_used_if_type_not_in_the_model_even_if_type_is_interface()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(ICollection), set => set.Cast<ICollection>().FirstOrDefault());
        }

        [Fact]
        public void Set_throws_only_when_used_if_type_not_in_the_model_even_if_type_is_generic()
        {
            ThrowsForInvalidType<List<Product>>(set => set.FirstOrDefault());
        }

        [Fact]
        public void Non_generic_Set_throws_only_when_used_if_type_not_in_the_model_even_if_type_is_generic()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(List<Product>), set => set.Cast<List<Product>>().FirstOrDefault());
        }

        [Fact]
        public void Set_throws_only_when_used_if_type_not_in_the_model_even_if_type_is_anonymous()
        {
            var anon = new { Id = 4, Name = "" };

            using (var ctx = new SimpleModelContext())
            {
                var set = CreateDbSet(anon, ctx);
                Assert.Throws<InvalidOperationException>(() => set.FirstOrDefault()).ValidateMessage(
                    "DbSet_EntityTypeNotInModel", anon.GetType().Name);
            }
        }

        private static IDbSet<T> CreateDbSet<T>(T type, DbContext ctx)
            where T : class
        {
            return ctx.Set<T>();
        }

        [Fact]
        public void Non_generic_Set_throws_only_when_used_if_type_not_in_the_model_even_if_type_is_anonymous()
        {
            var anon = new { Id = 4, Name = "" };

            using (var ctx = new SimpleModelContext())
            {
                var set = ctx.Set(anon.GetType());
                Assert.Throws<InvalidOperationException>(() => set.Add(anon)).ValidateMessage(
                    "DbSet_EntityTypeNotInModel", anon.GetType().Name);
            }
        }

        [Fact]
        public void Set_throws_only_when_used_if_type_is_POCO_but_is_in_attributed_assembly_Dev10_883031()
        {
            var assembly = new DynamicAssembly();
            assembly.HasAttribute(new EdmSchemaAttribute());
            assembly.DynamicType("PocoEntity").Property("Id").HasType(typeof(int));
            assembly.DynamicType("EocoEntity").Property("Id").HasType(typeof(int)).HasAttribute(
                new EdmEntityTypeAttribute());
            var modelBuilder = assembly.ToBuilder();
            var model = modelBuilder.Build(ProviderRegistry.Sql2008_ProviderInfo).Compile();

            var pocoType = assembly.Types.Single(t => t.Name == "PocoEntity");
            var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes).MakeGenericMethod(pocoType);
            var findMethod = typeof(DbSet<>).MakeGenericType(pocoType).GetMethod("Find");

            using (var context = new DbContext("MixedPocoEocoContext", model))
            {
                var set = setMethod.Invoke(context, null);
                Assert.Throws<InvalidOperationException>(() =>
                                                         {
                                                             try
                                                             {
                                                                 findMethod.Invoke(set,
                                                                                   new object[] { new object[] { 1 } });
                                                             }
                                                             catch (TargetInvocationException tie)
                                                             {
                                                                 throw tie.InnerException;
                                                             }
                                                         }).ValidateMessage("DbSet_PocoAndNonPocoMixedInSameAssembly",
                                                                            "PocoEntity");
            }
        }

        [Fact]
        public void Add_throws_when_type_not_in_model()
        {
            ThrowsForInvalidType<Login>(set => set.Add(new Login()));
        }

        [Fact]
        public void Non_generic_Add_throws_when_type_not_in_model()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(Login), set => set.Add(new Login()));
        }

        [Fact]
        public void Add_throws_when_type_is_interface()
        {
            ThrowsForInvalidType<ICollection>(set => set.Add(new ArrayList()));
        }

        [Fact]
        public void Non_generic_Add_throws_when_type_is_interface()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(ICollection), set => set.Add(new ArrayList()));
        }

        [Fact]
        public void Add_throws_when_type_is_generic()
        {
            ThrowsForInvalidType<List<Product>>(set => set.Add(new List<Product>()));
        }

        [Fact]
        public void Non_generic_Add_throws_when_type_is_generic()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(List<Product>), set => set.Add(new List<Product>()));
        }

        [Fact]
        public void Add_throws_when_type_does_not_have_key_defined()
        {
            using (var context = new PersonContext())
            {
                Assert.Throws<ModelValidationException>(
                    () => context.People.Add(new Person { FirstName = "Joe", LastName = "McKellor" }));
            }
        }

        [Fact]
        public void Attach_throws_when_type_does_not_have_key_defined()
        {
            using (var context = new PersonContext())
            {
                Assert.Throws<ModelValidationException>(
                    () => context.People.Attach(new Person { FirstName = "Joe", LastName = "McKellor" }));
            }
        }

        [Fact]
        public void Add_throws_when_type_is_unmapped_derived_entity()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var office = new UnMappedOffice();
                Assert.Throws<InvalidOperationException>(() => context.Offices.Add(office)).ValidateMessage(
                    "ObjectContext_NoMappingForEntityType", typeof(UnMappedOffice).FullName);
            }
        }

        [Fact]
        public void Non_generic_Add_throws_when_type_is_unmapped_derived_entity()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var office = new UnMappedOffice();
                Assert.Throws<InvalidOperationException>(() => context.Set(typeof(Office)).Add(office)).ValidateMessage(
                    "ObjectContext_NoMappingForEntityType", typeof(UnMappedOffice).FullName);
            }
        }

        [Fact]
        public void Attach_throws_when_type_not_in_model()
        {
            ThrowsForInvalidType<Login>(set => set.Attach(new Login()));
        }

        [Fact]
        public void Non_generic_Attach_throws_when_type_not_in_model()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(Login), set => set.Attach(new Login()));
        }

        [Fact]
        public void Attach_throws_when_type_is_interface()
        {
            ThrowsForInvalidType<ICollection>(set => set.Attach(new ArrayList()));
        }

        [Fact]
        public void Non_generic_Attach_throws_when_type_is_interface()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(ICollection), set => set.Attach(new ArrayList()));
        }

        [Fact]
        public void Attach_throws_when_type_is_generic()
        {
            ThrowsForInvalidType<List<Product>>(set => set.Attach(new List<Product>()));
        }

        [Fact]
        public void Non_generic_Attach_throws_when_type_is_generic()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(List<Product>), set => set.Attach(new List<Product>()));
        }

        [Fact]
        public void Attach_throws_when_type_is_unmapped_derived_entity()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var office = new UnMappedOffice();
                Assert.Throws<InvalidOperationException>(() => context.Offices.Attach(office)).ValidateMessage(
                    "ObjectContext_NoMappingForEntityType", typeof(UnMappedOffice).FullName);
            }
        }

        [Fact]
        public void Non_generic_Attach_throws_when_type_is_unmapped_derived_entity()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var office = new UnMappedOffice();
                Assert.Throws<InvalidOperationException>(() => context.Set(typeof(Office)).Attach(office)).
                    ValidateMessage("ObjectContext_NoMappingForEntityType", typeof(UnMappedOffice).FullName);
            }
        }

        [Fact]
        public void Remove_throws_when_type_not_in_model()
        {
            ThrowsForInvalidType<Login>(set => set.Remove(new Login()));
        }

        [Fact]
        public void Non_generic_Remove_throws_when_type_not_in_model()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(Login), set => set.Remove(new Login()));
        }

        [Fact]
        public void Remove_throws_when_type_is_interface()
        {
            ThrowsForInvalidType<ICollection>(set => set.Remove(new ArrayList()));
        }

        [Fact]
        public void Non_generic_Remove_throws_when_type_is_interface()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(ICollection), set => set.Remove(new ArrayList()));
        }

        [Fact]
        public void Remove_throws_when_type_is_generic()
        {
            ThrowsForInvalidType<List<Product>>(set => set.Remove(new List<Product>()));
        }

        [Fact]
        public void Non_generic_Remove_throws_when_type_is_generic()
        {
            ThrowsForInvalidTypeNonGeneric(typeof(List<Product>), set => set.Remove(new List<Product>()));
        }

        #endregion
    }
}