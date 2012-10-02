// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity;

public class ContextWithNoNamespace : DbContext
{
    public ContextWithNoNamespace()
    {
        Database.SetInitializer<ContextWithNoNamespace>(null);
    }

    public DbSet<ForMetadataLookup> ForMetadataLookup { get; set; }
}

public class ForMetadataLookup
{
    public int Id { get; set; }
}

namespace This.Is.A.Normal.Namespace
{
    using System.Data.Entity.Infrastructure;

    public class ContextInNormalNamespace : DbContext
    {
        public ContextInNormalNamespace()
        {
            Database.SetInitializer<ContextInNormalNamespace>(null);
        }

        public DbSet<ForMetadataLookup> ForMetadataLookup { get; set; }
    }

    public class ContextInNormalNamespaceWithConventionRemoved : DbContext
    {
        public ContextInNormalNamespaceWithConventionRemoved()
        {
            Database.SetInitializer<ContextInNormalNamespaceWithConventionRemoved>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<ModelNamespaceConvention>();
        }

        public DbSet<ForMetadataLookup> ForMetadataLookup { get; set; }
    }

    public class ContextWithContainerConventionRemoved : DbContext
    {
        public ContextWithContainerConventionRemoved()
        {
            Database.SetInitializer<ContextWithContainerConventionRemoved>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<ModelContainerConvention>();
        }

        public DbSet<ForMetadataLookup> ForMetadataLookup { get; set; }
    }

    public class GenericContext<T1, T2> : DbContext
    {
        public GenericContext()
        {
            Database.SetInitializer<GenericContext<T1, T2>>(null);
        }

        public class NestedGenericContext<TN1, TN2> : DbContext
        {
            public NestedGenericContext()
            {
                Database.SetInitializer<NestedGenericContext<TN1, TN2>>(null);
            }
        }

        public DbSet<ForMetadataLookup> ForMetadataLookup { get; set; }
    }
}

namespace ____._____.__
{
    public class _3_1_4_1_5_9_ : DbContext
    {
        public _3_1_4_1_5_9_()
        {
            Database.SetInitializer<_3_1_4_1_5_9_>(null);
        }

        public DbSet<ForMetadataLookup> ForMetadataLookup { get; set; }
    }
}

namespace __This.Is_3_Not.Compl3t3ly.Invalid
{
    public class __Context_In_PartiallyInvalidNam3spac3 : DbContext
    {
        public __Context_In_PartiallyInvalidNam3spac3()
        {
            Database.SetInitializer<__Context_In_PartiallyInvalidNam3spac3>(null);
        }

        public DbSet<ForMetadataLookup> ForMetadataLookup { get; set; }
    }
}

namespace _3Unicorns.Starts.With.Underscrore.Digit
{
    public class _3UnicornsContextInUnderscoreDigitNamespace : DbContext
    {
        public _3UnicornsContextInUnderscoreDigitNamespace()
        {
            Database.SetInitializer<_3UnicornsContextInUnderscoreDigitNamespace>(null);
        }

        public DbSet<ForMetadataLookup> ForMetadataLookup { get; set; }
    }
}

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using This.Is.A.Normal.Namespace;
    using Xunit;
    using _3Unicorns.Starts.With.Underscrore.Digit;
    using __This.Is_3_Not.Compl3t3ly.Invalid;
    using ____._____.__;

    public class ModelNamespaceAndContainerTests : FunctionalTestBase
    {
        #region Model namespace tests

        [Fact]
        public void Context_namespace_is_used_for_c_space_namespace()
        {
            using (var context = new ContextInNormalNamespace())
            {
                Assert.Equal("This.Is.A.Normal.Namespace", GetModelNamespace(context));
            }
        }

        [Fact]
        public void Context_namespace_used_for_c_space_namespace_has_invalid_character_removed()
        {
            using (var context = new __Context_In_PartiallyInvalidNam3spac3())
            {
                Assert.Equal("This.Is_3_Not.Compl3t3ly.Invalid", GetModelNamespace(context));
            }
        }

        [Fact]
        public void
            Context_namespace_starting_with_underscore_digit_used_for_c_space_namespace_has_invalid_character_removed()
        {
            using (var context = new _3UnicornsContextInUnderscoreDigitNamespace())
            {
                Assert.Equal("Unicorns.Starts.With.Underscrore.Digit", GetModelNamespace(context));
            }
        }

        [Fact]
        public void Default_Code_First_namespace_is_used_for_c_space_namespace_if_convention_is_removed()
        {
            using (var context = new ContextInNormalNamespaceWithConventionRemoved())
            {
                Assert.Equal("CodeFirstNamespace", GetModelNamespace(context));
                Assert.Equal("ContextInNormalNamespaceWithConventionRemoved", GetModelContainerName(context));
            }
        }

        [Fact]
        public void Default_Code_First_namespace_is_used_for_c_space_namespace_if_context_has_no_namespace()
        {
            using (var context = new ContextWithNoNamespace())
            {
                Assert.Equal("CodeFirstNamespace", GetModelNamespace(context));
            }
        }

        [Fact]
        public void Default_Code_First_namespace_is_used_for_c_space_namespace_if_context_has_invalid_namespace()
        {
            using (var context = new _3_1_4_1_5_9_())
            {
                Assert.Equal("CodeFirstNamespace", GetModelNamespace(context));
            }
        }

        private static string GetModelNamespace(DbContext context)
        {
            return GetEntityType(context, typeof(ForMetadataLookup)).NamespaceName;
        }

        #endregion

        #region Model container name tests

        [Fact]
        public void Context_name_is_used_for_container_name()
        {
            using (var context = new ContextInNormalNamespace())
            {
                Assert.Equal("ContextInNormalNamespace", GetModelContainerName(context));
            }
        }

        [Fact]
        public void Context_name_used_for_container_name_has_invalid_characters_removed()
        {
            using (var context = new __Context_In_PartiallyInvalidNam3spac3())
            {
                Assert.Equal("Context_In_PartiallyInvalidNam3spac3", GetModelContainerName(context));
            }
        }

        [Fact]
        public void Context_name_starting_with_underscore_digit_used_for_container_name_has_invalid_characters_removed()
        {
            using (var context = new _3UnicornsContextInUnderscoreDigitNamespace())
            {
                Assert.Equal("UnicornsContextInUnderscoreDigitNamespace", GetModelContainerName(context));
            }
        }

        [Fact]
        public void Default_Code_First_container_name_is_used_for_container_name_if_convention_is_removed()
        {
            using (var context = new ContextWithContainerConventionRemoved())
            {
                Assert.Equal("CodeFirstContainer", GetModelContainerName(context));
                Assert.Equal("This.Is.A.Normal.Namespace", GetModelNamespace(context));
            }
        }

        [Fact]
        public void Context_name_is_used_for_container_name_if_context_has_no_namespace()
        {
            using (var context = new ContextWithNoNamespace())
            {
                Assert.Equal("ContextWithNoNamespace", GetModelContainerName(context));
            }
        }

        [Fact]
        public void Default_Code_First_container_name_is_used_for_container_name_if_context_has_invalid_namespace()
        {
            using (var context = new _3_1_4_1_5_9_())
            {
                Assert.Equal("CodeFirstContainer", GetModelContainerName(context));
            }
        }

        [Fact]
        public void Generic_context_name_used_for_container_name_has_invalid_characters_removed()
        {
            using (var context = new GenericContext<string, ICollection<int>>())
            {
                Assert.Equal("GenericContext2", GetModelContainerName(context));
            }
        }

        [Fact]
        public void Nested_generic_context_name_used_for_container_name_has_invalid_characters_removed()
        {
            using (var context = new GenericContext<string, int>.NestedGenericContext<Random, byte>())
            {
                Assert.Equal("NestedGenericContext2", GetModelContainerName(context));
            }
        }

        private static string GetModelContainerName(DbContext context)
        {
            return ((IObjectContextAdapter)context).ObjectContext.DefaultContainerName;
        }

        #endregion
    }
}
