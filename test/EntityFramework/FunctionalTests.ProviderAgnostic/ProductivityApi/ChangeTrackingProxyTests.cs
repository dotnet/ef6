// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Data.Entity.Configuration;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.TestHelpers;
    using System.Linq;
    using Xunit;

    public class ChangeTrackingProxyTests
    {
        #region Infrastructure/setup

        private readonly bool _isSqlAzure;

        public ChangeTrackingProxyTests()
        {
            using (var context = new GranniesContext())
            {
                context.Database.Initialize(force: false);
            }

            using (var context = new HaveToDoContext())
            {
                _isSqlAzure = DatabaseTestHelpers.IsSqlAzure(context.Database.Connection.ConnectionString);
                if (!_isSqlAzure)
                {
                    context.Database.Initialize(force: false);
                }
            }
        }

        #endregion

        #region DeleteObject throws a collection modified exception with change tracking proxies (Dev11 71937, 209773)

        [Fact]
        public void Deleting_object_when_relationships_have_not_been_all_enumerated_should_not_cause_collection_modified_exception_71937()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new GranniesContext())
                {
                    using (context.Database.BeginTransaction())
                    {
                        var g = context.Grannys.Add(context.Grannys.Create());

                        var c = context.Children.Add(context.Children.Create());
                        c.Name = "Child";

                        var h = context.Houses.Add(context.Houses.Create());

                        g.Children.Add(c);
                        g.House = h;

                        context.SaveChanges();

                        context.Children.Remove(c); // This would previously throw

                        Assert.Equal(EntityState.Deleted, context.Entry(c).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(g).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(h).State);

                        Assert.Null(c.House);
                        Assert.Null(c.Granny);

                        Assert.Equal(0, g.Children.Count);
                        Assert.Same(h, g.House);

                        Assert.Equal(0, h.Children.Count);
                        Assert.Same(g, h.Granny);

                        context.SaveChanges();

                        Assert.Equal(EntityState.Detached, context.Entry(c).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(g).State);
                        Assert.Equal(EntityState.Unchanged, context.Entry(h).State);

                        Assert.Null(c.House);
                        Assert.Null(c.Granny);

                        Assert.Equal(0, g.Children.Count);
                        Assert.Same(h, g.House);

                        Assert.Equal(0, h.Children.Count);
                        Assert.Same(g, h.Granny);
                    }
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        public class GranniesContext : DbContext
        {
            public GranniesContext()
            {
                Database.SetInitializer(new DropCreateDatabaseIfModelChanges<GranniesContext>());
            }

            public DbSet<Granny> Grannys { get; set; }
            public DbSet<Child> Children { get; set; }
            public DbSet<House> Houses { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Granny>().HasRequired(g => g.House).WithRequiredPrincipal(h => h.Granny);
                modelBuilder.Entity<Granny>().HasMany(g => g.Children).WithRequired(c => c.Granny);
                modelBuilder.Entity<House>().HasMany(h => h.Children).WithOptional(c => c.House);
            }
        }

        public class Granny
        {
            public virtual int Id { get; set; }
            public virtual House House { get; set; }
            public virtual ICollection<Child> Children { get; set; }
        }

        public class Child
        {
            public virtual int Id { get; set; }
            public virtual string Name { get; set; }
            public virtual Granny Granny { get; set; }
            public virtual House House { get; set; }
        }

        public class House
        {
            public virtual int Id { get; set; }
            public virtual ICollection<Child> Children { get; set; }
            public virtual Granny Granny { get; set; }
        }

        [Fact]
        public void Deleting_object_when_relationships_have_not_been_all_enumerated_should_not_cause_collection_modified_exception_209773()
        {
            if (!_isSqlAzure)
            {
                try
                {
                    ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                    using (var context = new HaveToDoContext())
                    {
                        using (context.Database.BeginTransaction())
                        {
                            var group = context.ComplexExams.Where(e => e.Code == "group").Single();
                            var _ = group.Training.Id;

                            context.ComplexExams.Remove(group); // This would previously throw

                            Assert.Equal(EntityState.Deleted, context.Entry(group).State);
                            Assert.Equal(0, group.Exams.Count);

                            context.SaveChanges();

                            Assert.Equal(EntityState.Detached, context.Entry(group).State);
                            Assert.Equal(0, group.Exams.Count);
                        }
                    }
                }
                finally
                {
                    ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
                }
            }
        }

        public class HaveToDoContext : DbContext
        {
            public HaveToDoContext()
            {
                Database.SetInitializer(new HaveToDoInitializer());
            }

            public DbSet<Training> Training { get; set; }
            public DbSet<Exam> Exams { get; set; }
            public DbSet<ComplexExam> ComplexExams { get; set; }
        }

        public class HaveToDoInitializer : DropCreateDatabaseAlways<HaveToDoContext>
        {
            protected override void Seed(HaveToDoContext context)
            {
                var training = context.Training.Add(context.Training.Create());
                training.Code = "training";
                training.Todos.Add(
                    new ComplexExam
                    {
                        Code = "group"
                    });
            }
        }

        public class Training
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public virtual Guid Id { get; set; }

            public virtual string Code { get; set; }
            public virtual ICollection<HaveToDo> Todos { get; set; }
        }

        public class HaveToDo
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public virtual Guid Id { get; set; }

            public virtual string Code { get; set; }
            public virtual Training Training { get; set; }
        }

        public class Exam : HaveToDo
        {
            public virtual ComplexExam Parent { get; set; }
        }

        public class ComplexExam : HaveToDo
        {
            public virtual ICollection<Exam> Exams { get; set; }
        }

        #endregion

        #region Re-parenting 1:0..1 Added dependent (263813)

        [Fact]
        public void Re_parenting_one_to_zero_or_one_Added_dependent_should_cause_existing_Added_dependnent_to_be_Detached()
        {
            Re_parenting_one_to_zero_or_one_Added_dependent_should_cause_existing_dependnent_to_be_Deleted_or_Detached(EntityState.Added, useFK: false);
        }

        [Fact]
        public void Re_parenting_one_to_zero_or_one_Added_dependent_should_cause_existing_Unchanged_dependnent_to_be_Deleted()
        {
            Re_parenting_one_to_zero_or_one_Added_dependent_should_cause_existing_dependnent_to_be_Deleted_or_Detached(EntityState.Unchanged, useFK: false);
        }

        [Fact]
        public void Re_parenting_one_to_zero_or_one_Added_dependent_by_changing_FK_should_cause_existing_Added_dependnent_to_be_Detached()
        {
            Re_parenting_one_to_zero_or_one_Added_dependent_should_cause_existing_dependnent_to_be_Deleted_or_Detached(EntityState.Added, useFK: true);
        }

        [Fact]
        public void Re_parenting_one_to_zero_or_one_Added_dependent_by_changing_FK_should_cause_existing_Unchanged_dependnent_to_be_Deleted()
        {
            Re_parenting_one_to_zero_or_one_Added_dependent_should_cause_existing_dependnent_to_be_Deleted_or_Detached(EntityState.Unchanged, useFK: true);
        }

        private void Re_parenting_one_to_zero_or_one_Added_dependent_should_cause_existing_dependnent_to_be_Deleted_or_Detached(EntityState dependentState, bool useFK)
        {
            using (var context = new YummyContext())
            {
                var apple = context.Products.Create();
                apple.Id = 1;
                apple.Name = "Apple";

                var appleDetail = context.Details.Create();
                appleDetail.Id = apple.Id;
                appleDetail.ExtraInfo = "Good for your health!";

                var chocolate = context.Products.Create();
                chocolate.Id = 2;
                chocolate.Name = "Chocolate";

                var chocolateDetail = context.Details.Create();
                chocolateDetail.Id = chocolate.Id;
                chocolateDetail.ExtraInfo = "Probably not so good for your health!";

                context.Products.Attach(apple);
                context.Products.Attach(chocolate);
                context.Entry(chocolateDetail).State = dependentState;
                context.Details.Add(appleDetail);

                Assert.Same(apple, appleDetail.Product);
                Assert.Same(appleDetail, apple.ProductDetail);
                Assert.Same(chocolate, chocolateDetail.Product);
                Assert.Same(chocolateDetail, chocolate.ProductDetail);

                if (useFK)
                {
                    appleDetail.Id = chocolate.Id;
                }
                else
                {
                    appleDetail.Product = chocolate;
                }

                Assert.Same(chocolate, appleDetail.Product);
                Assert.Same(appleDetail, chocolate.ProductDetail);
                Assert.Null(chocolateDetail.Product);
                Assert.Null(apple.ProductDetail);

                Assert.Equal(
                    dependentState == EntityState.Added ? EntityState.Detached : EntityState.Deleted,
                    context.Entry(chocolateDetail).State);
                Assert.Equal(EntityState.Added, context.Entry(appleDetail).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(chocolate).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(apple).State);
            }
        }

        public class YummyContext : DbContext
        {
            public YummyContext()
            {
                Database.SetInitializer(new DropCreateDatabaseAlways<YummyContext>());
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<YummyDetail>().HasRequired(d => d.Product).WithOptional(p => p.ProductDetail);
            }

            public DbSet<YummyProduct> Products { get; set; }
            public DbSet<YummyDetail> Details { get; set; }
        }

        public class YummyProduct
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public virtual int Id { get; set; }

            public virtual string Name { get; set; }
            public virtual YummyDetail ProductDetail { get; set; }
        }

        public class YummyDetail
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public virtual int Id { get; set; }

            public virtual string ExtraInfo { get; set; }
            public virtual YummyProduct Product { get; set; }
        }

        #endregion

        [Fact]
        public void RelationshipManager_can_be_accessed_by_casting_proxy_entity_to_IEntityWithRelationships()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new GranniesContext())
                {
                    using (context.Database.BeginTransaction())
                    {
                        var granny = context.Grannys.Create();
                        context.Grannys.Attach(granny);

                        var relationshipManager = ((IEntityWithRelationships)granny).RelationshipManager;
                        Assert.NotNull(relationshipManager);
                    }
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }

        [Fact]
        public void SetChangeTracker_can_be_called_by_casting_proxy_entity_to_IEntityWithChangeTracker()
        {
            try
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = true;
                using (var context = new GranniesContext())
                {
                    using (context.Database.BeginTransaction())
                    {
                        var granny = context.Grannys.Create();
                        context.Grannys.Attach(granny);

                        ((IEntityWithChangeTracker)granny).SetChangeTracker(null);
                    }
                }
            }
            finally
            {
                ProviderAgnosticConfiguration.SuspendExecutionStrategy = false;
            }
        }
    }
}
