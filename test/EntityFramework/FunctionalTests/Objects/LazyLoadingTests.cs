// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Objects
{
    using ConcurrencyModel;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class LazyLoadingTests : FunctionalTestBase
    {
        [Fact]
        public void Lazy_loading_of_entity_reference_does_not_work_on_detached_entity()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.FirstOrDefault();
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.Detach(team);

                Assert.Null(team.Engine);
            }
        }

        [Fact]
        public void Lazy_loading_entity_collection_does_not_work_on_detached_entity()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.FirstOrDefault();
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.Detach(team);

                Assert.Equal(0, team.Drivers.Count);
            }
        }

        [Fact]
        public void Lazy_loading_of_entity_reference_does_not_work_on_deleted_entity()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.FirstOrDefault();
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.DeleteObject(team);

                objectContext.DetectChanges();

                Assert.Null(team.Engine);
            }
        }

        [Fact]
        public void Lazy_loading_entity_collection_does_not_work_on_deleted_entity()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.FirstOrDefault();
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.DeleteObject(team);

                Assert.Equal(0, team.Drivers.Count);
            }
        }

        [Fact]
        public void Lazy_loading_of_entity_reference_works_on_modified_entity()
        {
            using (var context = new F1Context())
            {
                var teamId = context.Teams.OrderBy(t => t.Id).AsNoTracking().FirstOrDefault().Id;
                var engineId = context.Teams.Where(t => t.Id == teamId).Select(t => t.Engine).AsNoTracking().FirstOrDefault().Id;

                var team = context.Teams.Where(t => t.Id == teamId).AsNoTracking().Single();
                team.Constructor = "Fooblearius Fooblebar";

                Assert.NotNull(team.Engine);
            }
        }

        [Fact]
        public void Lazy_loading_entity_collection_works_on_modified_entity()
        {
            using (var context = new F1Context())
            {
                var team = context.Teams.FirstOrDefault();
                team.Constructor = "Fooblearius Fooblebar";

                Assert.True(team.Drivers.Count > 0);
            }
        }

        [Fact]
        public void Lazy_loading_does_not_occur_in_the_middle_of_materialization()
        {
            using (var context = new F1Context())
            {
                var teams = context.Teams.OrderBy(t => t.Id).Take(10);
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                objectContext.ObjectStateManager.ObjectStateManagerChanged += ObjectStateManager_ObjectStateManagerChanged;

                foreach (var team in teams)
                {
                    Assert.True(context.Configuration.LazyLoadingEnabled == true);
                }
            }
        }

        void ObjectStateManager_ObjectStateManagerChanged(object sender, ComponentModel.CollectionChangeEventArgs e)
        {
            Assert.True(((Team)e.Element).Drivers.Count == 0);
        }
    }
}
