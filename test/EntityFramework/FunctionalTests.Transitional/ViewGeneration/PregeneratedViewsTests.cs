// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.MappingViews;
using System.Data.Entity.ViewGeneration;

[assembly: DbMappingViewCacheType(
    typeof(PregeneratedViewsTests.ContextWithHashMissmatch), 
    typeof(PregeneratedViewsTests.ViewCacheWithHashMissmatch))]
[assembly: DbMappingViewCacheType(
    typeof(PregeneratedViewsTests.ContextWithInvalidView), 
    typeof(PregeneratedViewsTests.ViewCacheWithInvalidView))]
[assembly: DbMappingViewCacheType(
    typeof(PregeneratedViewsTests.ContextWithNullView), 
    typeof(PregeneratedViewsTests.ViewCacheWithNullView))]

namespace System.Data.Entity.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Resources;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Xml.Linq;
    using Xunit;

    public class PregeneratedViewsTests : FunctionalTestBase
    {
        [Fact]
        public void Exception_is_thrown_if_pregenerated_view_cache_mapping_hash_does_not_match()
        {
            using (var context = new ContextWithHashMissmatch())
            {
                var exception =
                    Assert.Throws<EntityCommandCompilationException>(
                        () => context.Blogs.ToString());

                Assert.NotNull(exception.InnerException);
                Assert.Equal(
                    Strings.ViewGen_HashOnMappingClosure_Not_Matching("ViewCacheWithHashMissmatch"),
                    exception.InnerException.Message);
            }
        }

        [Fact]
        public void Exception_is_thrown_if_pregenerated_view_cache_returns_view_with_invalid_esql()
        {
            using (var context = new ContextWithInvalidView())
            {
                var exception =
                    Assert.Throws<EntityCommandCompilationException>(
                        () => context.Blogs.ToString());

                Assert.NotNull(exception.InnerException);
                Assert.Equal(
                    Strings.Mapping_Invalid_QueryView(
                        "Blogs", 
                        Strings.CouldNotResolveIdentifier("Invalid") 
                            + " Near simple identifier, line 1, column 1."),
                    exception.InnerException.Message);
            }
        }

        [Fact]
        public void Exception_is_thrown_if_pregenerated_view_cache_returns_null_view_for_EntitySetBase_that_requires_esql()
        {
            using (var context = new ContextWithNullView())
            {
                var exception =
                    Assert.Throws<EntityCommandCompilationException>(
                        () => context.Blogs.ToString());

                Assert.NotNull(exception.InnerException);
                Assert.Equal(
                    Strings.Mapping_Views_For_Extent_Not_Generated("EntitySet", "Blogs"), 
                    exception.InnerException.Message);
            }
        }

        public class PregenBlog
        {
            public int Id { get; set; }
        }

        public class ContextWithHashMissmatch : DbContext
        {
            static ContextWithHashMissmatch()
            {
                Database.SetInitializer<ContextWithHashMissmatch>(null);
            }

            public DbSet<PregenBlog> Blogs { get; set; }
        }

        public class ViewCacheWithHashMissmatch : DbMappingViewCache
        {
            public override string MappingHashValue
            {
                get { return "Missmatch"; }
            }

            public override DbMappingView GetView(EntitySetBase extent)
            {
                throw new NotImplementedException();
            }
        }

        public class ContextWithInvalidView : DbContext
        {
            static ContextWithInvalidView()
            {
                Database.SetInitializer<ContextWithInvalidView>(null);
            }

            public DbSet<PregenBlog> Blogs { get; set; }
        }

        public class ViewCacheWithInvalidView : DbMappingViewCache
        {
            public override string MappingHashValue
            {
                get { return "15d7c7e9868caaf4966b8ef383979b6dbbeccffe1a12e3c1427de366f557cbe1"; }
            }

            public override DbMappingView GetView(EntitySetBase extent)
            {
                return new DbMappingView("Invalid");
            }
        }

        public class ContextWithNullView : DbContext
        {
            static ContextWithNullView()
            {
                Database.SetInitializer<ContextWithNullView>(null);
            }

            public DbSet<PregenBlog> Blogs { get; set; }
        }

        public class ViewCacheWithNullView : DbMappingViewCache
        {
            public override string MappingHashValue
            {
                get { return "073bf2d4f3ff4b869adf2adf400785826ba08e02a178374c013f35992856df6a"; }
            }

            public override DbMappingView GetView(EntitySetBase extent)
            {
                if (extent.Name == "PregenBlog")
                {
                    return new DbMappingView(@"
SELECT VALUE -- Constructing PregenBlog
    [CodeFirstDatabaseSchema.PregenBlog](T1.PregenBlog_Id)
FROM (
    SELECT 
        T.Id AS PregenBlog_Id, 
        True AS _from0
    FROM ContextWithNullView.Blogs AS T
) AS T1");
                }

                return null;
            }
        }
    }
}
