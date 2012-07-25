// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using AdvancedPatternsModel;
    using Xunit;

    /// <summary>
    /// Tests for the Include extension methods on IQueryable.
    /// </summary>
    public class IncludeTests : FunctionalTestBase
    {
        #region Basic positive Include(String) tests

        [Fact]
        public void String_Include_used_on_root_DbSet_spans_in_collections()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllOfficesIncluded(context.Buildings.Include("Offices").ToList());
            }
        }

        [Fact]
        public void String_Include_used_on_root_non_generic_DbSet_with_Cast_spans_in_collections()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllOfficesIncluded(context.Set(typeof(Building)).Cast<Building>().Include("Offices").ToList());
            }
        }

        [Fact]
        public void String_Include_used_on_root_ObjectSet_spans_in_collections()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllOfficesIncluded(CreateObjectSet<Building>(context).Include("Offices").ToList());
            }
        }

        [Fact]
        public void String_Include_used_on_root_DbSet_spans_in_references()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllBuildingsIncluded(context.Offices.Include("Building").ToList());
            }
        }

        [Fact]
        public void String_Include_used_on_root_non_generic_DbSet_with_Cast_spans_in_references()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllBuildingsIncluded(context.Set(typeof(Office)).Cast<Office>().Include("Building").ToList());
            }
        }

        [Fact]
        public void String_Include_used_on_root_ObjectSet_spans_in_references()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllBuildingsIncluded(CreateObjectSet<Office>(context).Include("Building").ToList());
            }
        }

        [Fact]
        public void String_Include_used_on_LINQ_query_created_from_DbSet_spans_in_collections()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Building> query = context.Buildings.Where(b => b.BuildingId != Guid.Empty);
                AssertAllOfficesIncluded(query.Include("Offices").ToList());
            }
        }

        [Fact]
        public void String_Include_used_on_LINQ_query_created_from_ObjectSet_spans_in_collections()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Building> query = CreateObjectSet<Building>(context).Where(b => b.BuildingId != Guid.Empty);
                AssertAllOfficesIncluded(query.Include("Offices").ToList());
            }
        }

        [Fact]
        public void String_Include_used_on_LINQ_query_created_from_DbSet_spans_in_references()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Office> query = context.Offices.Where(o => o.Number != null);
                AssertAllBuildingsIncluded(query.Include("Building").ToList());
            }
        }

        [Fact]
        public void String_Include_used_on_LINQ_query_created_from_ObjectSet_spans_in_references()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Office> query = CreateObjectSet<Office>(context).Where(o => o.Number != null);
                AssertAllBuildingsIncluded(query.Include("Building").ToList());
            }
        }

        #endregion

        #region Test for Include defined on the non-generic IQueryable

        [Fact]
        public void String_Include_used_on_root_non_generic_DbSet_works()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllBuildingsIncluded(context.Set(typeof(Office)).Include("Building").ToList<Office>());
            }
        }

        [Fact]
        public void String_Include_used_on_query_created_from_non_generic_DbSet_works()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var expression = context.Offices.Where(o => o.Number != null).Expression;
                var query = ((IQueryable)context.Set(typeof(Office))).Provider.CreateQuery(expression);

                AssertAllBuildingsIncluded(query.Include("Building").ToList<Office>());
            }
        }

        [Fact]
        public void Non_generic_String_Include_used_on_root_DbSet_works()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllBuildingsIncluded(((IQueryable)context.Offices).Include("Building").ToList<Office>());
            }
        }

        [Fact]
        public void Non_generic_String_Include_used_on_LINQ_query_created_from_DbSet_works()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var query = (IQueryable)context.Offices.Where(o => o.Number != null);
                AssertAllBuildingsIncluded(query.Include("Building").ToList<Office>());
            }
        }

        [Fact]
        public void Non_generic_String_Include_used_on_root_ObjectSet_works()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var query = (IQueryable)CreateObjectSet<Office>(context);
                AssertAllBuildingsIncluded(query.Include("Building").ToList<Office>());
            }
        }

        [Fact]
        public void Non_generic_String_Include_used_on_LINQ_query_created_from_ObjectSet_works()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var query = (IQueryable)CreateObjectSet<Office>(context).Where(o => o.Number != null);
                AssertAllBuildingsIncluded(query.Include("Building").ToList<Office>());
            }
        }

        #endregion

        #region Basic positive Include(lambda) tests

        [Fact]
        public void Lambda_Include_used_on_root_DbSet_spans_in_collections()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllOfficesIncluded(context.Buildings.Include(b => b.Offices).ToList());
            }
        }

        [Fact]
        public void Lambda_Include_used_on_root_non_generic_DbSet_using_Cast_spans_in_collections()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllOfficesIncluded(context.Set(typeof(Building)).Cast<Building>().Include(b => b.Offices).ToList());
            }
        }

        [Fact]
        public void Lambda_Include_used_on_root_ObjectSet_spans_in_collections()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllOfficesIncluded(CreateObjectSet<Building>(context).Include(b => b.Offices).ToList());
            }
        }

        [Fact]
        public void Lambda_Include_used_on_root_DbSet_spans_in_references()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllBuildingsIncluded(context.Offices.Include(o => o.Building).ToList());
            }
        }

        [Fact]
        public void Lambda_Include_used_on_root_non_generic_DbSet_with_Cast_spans_in_references()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllBuildingsIncluded(context.Set(typeof(Office)).Cast<Office>().Include(o => o.Building).ToList());
            }
        }

        [Fact]
        public void Lambda_Include_used_on_root_ObjectSet_spans_in_references()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                AssertAllBuildingsIncluded(CreateObjectSet<Office>(context).Include(o => o.Building).ToList());
            }
        }

        [Fact]
        public void Lambda_Include_used_on_LINQ_query_created_from_DbSet_spans_in_collections()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Building> query = context.Buildings.Where(b => b.BuildingId != Guid.Empty);
                AssertAllOfficesIncluded(query.Include(b => b.Offices).ToList());
            }
        }

        [Fact]
        public void Lambda_Include_used_on_LINQ_query_created_from_ObjectSet_spans_in_collections()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Building> query = CreateObjectSet<Building>(context).Where(b => b.BuildingId != Guid.Empty);
                AssertAllOfficesIncluded(query.Include(b => b.Offices).ToList());
            }
        }

        [Fact]
        public void Lambda_Include_used_on_LINQ_query_created_from_DbSet_spans_in_references()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Office> query = context.Offices.Where(o => o.Number != null);
                AssertAllBuildingsIncluded(query.Include(o => o.Building).ToList());
            }
        }

        [Fact]
        public void Lambda_Include_used_on_LINQ_query_created_from_ObjectSet_spans_in_references()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Office> query = CreateObjectSet<Office>(context).Where(o => o.Number != null);
                AssertAllBuildingsIncluded(query.Include(o => o.Building).ToList());
            }
        }

        #endregion

        #region Multiple level Include(String) positive tests

        [Fact]
        public void Multiple_level_string_Include_used_on_root_DbSet_spans_in_multiple_levels()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var buildings = context.Buildings.Include("Offices.Whiteboards").ToList();
                AssertAllOfficesAndWhiteboardsIncluded(buildings);
            }
        }

        [Fact]
        public void Multiple_level_string_Include_used_on_root_ObjectSet_spans_in_multiple_levels()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var buildings = CreateObjectSet<Building>(context).Include("Offices.Whiteboards").ToList();
                AssertAllOfficesAndWhiteboardsIncluded(buildings);
            }
        }

        [Fact]
        public void Multiple_level_string_Include_used_on_LINQ_query_created_from_DbSet_spans_in_multiple_levels()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Building> query = context.Buildings.Where(b => b.BuildingId != Guid.Empty);
                var buildings = query.Include("Offices.Whiteboards").ToList();
                AssertAllOfficesAndWhiteboardsIncluded(buildings);
            }
        }

        [Fact]
        public void Multiple_level_string_Include_used_on_LINQ_query_created_from_ObjectSet_spans_in_multiple_levels()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Building> query = CreateObjectSet<Building>(context).Where(b => b.BuildingId != Guid.Empty);
                var buildings = query.Include("Offices.Whiteboards").ToList();
                AssertAllOfficesAndWhiteboardsIncluded(buildings);
            }
        }

        #endregion

        #region Multiple level Include(Lambda) positive tests

        [Fact]
        public void Multiple_level_lambda_Include_used_on_root_DbSet_spans_in_multiple_levels()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var buildings = context.Buildings.Include(b => b.Offices.Select(o => o.WhiteBoards)).ToList();
                AssertAllOfficesAndWhiteboardsIncluded(buildings);
            }
        }

        [Fact]
        public void Multiple_level_lambda_Include_used_on_root_ObjectSet_spans_in_multiple_levels()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var buildings =
                    CreateObjectSet<Building>(context).Include(b => b.Offices.Select(o => o.WhiteBoards)).ToList();
                AssertAllOfficesAndWhiteboardsIncluded(buildings);
            }
        }

        [Fact]
        public void Multiple_level_lambda_Include_used_on_LINQ_query_created_from_DbSet_spans_in_multiple_levels()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Building> query = context.Buildings.Where(b => b.BuildingId != Guid.Empty);
                var buildings = query.Include(b => b.Offices.Select(o => o.WhiteBoards)).ToList();
                AssertAllOfficesAndWhiteboardsIncluded(buildings);
            }
        }

        [Fact]
        public void Multiple_level_lambda_Include_used_on_LINQ_query_created_from_ObjectSet_spans_in_multiple_levels()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Building> query = CreateObjectSet<Building>(context).Where(b => b.BuildingId != Guid.Empty);
                var buildings = query.Include(b => b.Offices.Select(o => o.WhiteBoards)).ToList();
                AssertAllOfficesAndWhiteboardsIncluded(buildings);
            }
        }

        #endregion

        #region Multiple calls to Include(String) positive tests

        [Fact]
        public void Multiple_string_includes_on_same_original_DbSet_applies_all_spans()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var offices = context.Offices.Include("Building").Include("Whiteboards").ToList();
                AssertAllBuildingsIncluded(offices);
                AssertAllWhiteboardsIncluded(offices);
            }
        }

        [Fact]
        public void Multiple_string_includes_on_same_original_ObjectSet_applies_all_spans()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var offices = CreateObjectSet<Office>(context).Include("Building").Include("Whiteboards").ToList();
                AssertAllBuildingsIncluded(offices);
                AssertAllWhiteboardsIncluded(offices);
            }
        }

        [Fact]
        public void Multiple_string_includes_on_same_LINQ_query_created_from_a_DbSet_applies_all_spans()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Office> query = context.Offices.Where(o => o.Number != null);
                var offices = query.Include("Building").Include("Whiteboards").ToList();
                AssertAllBuildingsIncluded(offices);
                AssertAllWhiteboardsIncluded(offices);
            }
        }

        [Fact]
        public void Multiple_string_includes_on_same_LINQ_query_created_from_a_ObjectSet_applies_all_spans()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Office> query = CreateObjectSet<Office>(context).Where(o => o.Number != null);
                var offices = query.Include("Building").Include("Whiteboards").ToList();
                AssertAllBuildingsIncluded(offices);
                AssertAllWhiteboardsIncluded(offices);
            }
        }

        #endregion

        #region Multiple calls to Include(Lambda) positive tests

        [Fact]
        public void Multiple_lambda_includes_on_same_original_DbSet_applies_all_spans()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var offices = context.Offices.Include(o => o.Building).Include(o => o.WhiteBoards).ToList();
                AssertAllBuildingsIncluded(offices);
                AssertAllWhiteboardsIncluded(offices);
            }
        }

        [Fact]
        public void Multiple_lambda_includes_on_same_original_ObjectSet_applies_all_spans()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                var offices =
                    CreateObjectSet<Office>(context).Include(o => o.Building).Include(o => o.WhiteBoards).ToList();
                AssertAllBuildingsIncluded(offices);
                AssertAllWhiteboardsIncluded(offices);
            }
        }

        [Fact]
        public void Multiple_lambda_includes_on_same_LINQ_query_created_from_a_DbSet_applies_all_spans()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Office> query = context.Offices.Where(o => o.Number != null);
                var offices = query.Include(o => o.Building).Include(o => o.WhiteBoards).ToList();
                AssertAllBuildingsIncluded(offices);
                AssertAllWhiteboardsIncluded(offices);
            }
        }

        [Fact]
        public void Multiple_lambda_includes_on_same_LINQ_query_created_from_a_ObjectSet_applies_all_spans()
        {
            using (var context = AdvancedContextWithNoLazyLoading())
            {
                IQueryable<Office> query = CreateObjectSet<Office>(context).Where(o => o.Number != null);
                var offices = query.Include(o => o.Building).Include(o => o.WhiteBoards).ToList();
                AssertAllBuildingsIncluded(offices);
                AssertAllWhiteboardsIncluded(offices);
            }
        }

        #endregion

        #region Helpers

        private void AssertAllOfficesIncluded(List<Building> buildings)
        {
            Assert.True(buildings.TrueForAll(b => b.Offices != null));
            Assert.Equal(AdvancedModelOfficeCount, buildings.Sum(b => b.Offices.Count));
        }

        private void AssertAllWhiteboardsIncluded(List<Office> offices)
        {
            Assert.True(offices.TrueForAll(o => o.WhiteBoards != null));
            Assert.Equal(AdvancedModelWhiteboardCount, offices.Sum(o => o.WhiteBoards.Count));
        }

        private void AssertAllOfficesAndWhiteboardsIncluded(List<Building> buildings)
        {
            AssertAllOfficesIncluded(buildings);
            Assert.True(buildings.TrueForAll(b => b.Offices.All(o => o.WhiteBoards != null)));
            Assert.Equal(AdvancedModelWhiteboardCount, buildings.Sum(b => b.Offices.Sum(o => o.WhiteBoards.Count)));
        }

        private void AssertAllBuildingsIncluded(List<Office> offices)
        {
            Assert.True(offices.TrueForAll(o => o.Building != null));
        }

        private AdvancedPatternsMasterContext AdvancedContextWithNoLazyLoading()
        {
            var context = new AdvancedPatternsMasterContext();
            context.Configuration.LazyLoadingEnabled = false;
            return context;
        }

        private ObjectSet<TEntity> CreateObjectSet<TEntity>(DbContext context) where TEntity : class
        {
            return GetObjectContext(context).CreateObjectSet<TEntity>();
        }

        #endregion
    }
}