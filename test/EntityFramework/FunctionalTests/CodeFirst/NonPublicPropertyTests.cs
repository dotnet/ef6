// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.CodeFirst
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Xunit;

    public class NonPublicPropertyTests : FunctionalTestBase
    {
        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_queried()
        {
            using (var context = new PrivacyContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var one = context.Ones.Include(o => o.ColRel).Single();

                Assert.NotEqual(0, one.Id);
                Assert.Equal(16, one.Info.Image.Length);
                Assert.Equal("Right Here, Right Now", one.Name);
                Assert.Equal(AnEnum.Beat, one.AnEnum);

                var two = one.ColRel.Single();
                Assert.Equal(747, two.Id);
                Assert.Equal("Sunset", two.Name);
                Assert.Equal(AnEnum.Beat, two.AnEnum);
                Assert.Same(one, two.RefRel);
                Assert.Equal(one.Id, two.RefRelId);
            }
        }

        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_updated_and_inserted()
        {
            using (var context = new PrivacyContext())
            {
                var one = context.Ones.Include(o => o.ColRel).Single();

                one.SetName("Praise You");
                one.Info.SetImage(new byte[32]);
                one.SetAnEnum(AnEnum.Big);
                one.ColRel.Single().SetName("Wonderful Night");
                one.SetColRel(
                    new List<ActualEntity2>
                        {
                            one.ColRel.Single(),
                            new ActualEntity2(748, "Slash Dot Dash", AnEnum.Beat, 0, null)
                        });

                using (context.Database.BeginTransaction())
                {
                    context.SaveChanges();

                    var twos = context.Twos.AsNoTracking().ToList();
                    Assert.Equal(new[] { "Slash Dot Dash", "Wonderful Night" }, twos.Select(t => t.Name).OrderBy(n => n));
                    Assert.Equal(new[] { 747, 748 }, twos.Select(t => t.Id).OrderBy(n => n));

                    one = context.Ones.AsNoTracking().Single();
                    Assert.Equal("Praise You", one.Name);
                    Assert.Equal(AnEnum.Big, one.AnEnum);
                    Assert.Equal(32, one.Info.Image.Length);
                }
            }
        }

        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_obtained_with_Find()
        {
            using (var context = new PrivacyContext())
            {
                var two = context.Twos.Find(747);

                Assert.Equal(747, two.Id);
                Assert.Equal("Sunset", two.Name);
                Assert.Equal(AnEnum.Beat, two.AnEnum);
                Assert.Same(two, context.Twos.Find(747));
            }
        }

        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_lazy_loaded_by_collection()
        {
            using (var context = new PrivacyContext())
            {
                var one = context.Ones.Single();
                var two = one.ColRel.Single();

                Assert.Equal(747, two.Id);
                Assert.Equal("Sunset", two.Name);
                Assert.Equal(AnEnum.Beat, two.AnEnum);
                Assert.Same(one, two.RefRel);
                Assert.Equal(one.Id, two.RefRelId);
            }
        }

        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_lazy_loaded_by_reference()
        {
            using (var context = new PrivacyContext())
            {
                var two = context.Twos.Single();
                var one = two.RefRel;

                Assert.NotEqual(0, one.Id);
                Assert.Equal("Right Here, Right Now", one.Name);
                Assert.Equal(AnEnum.Beat, two.AnEnum);
                Assert.Same(one, two.RefRel);
                Assert.Equal(one.Id, two.RefRelId);
            }
        }

        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_explicitly_loaded_by_collection()
        {
            using (var context = new PrivacyContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var one = context.Ones.Single();
                context.Entry(one).Collection(e => e.ColRel).Load();
                var two = one.ColRel.Single();

                Assert.Equal(747, two.Id);
                Assert.Equal("Sunset", two.Name);
                Assert.Equal(AnEnum.Beat, two.AnEnum);
                Assert.Same(one, two.RefRel);
                Assert.Equal(one.Id, two.RefRelId);
            }
        }

        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_explicitly_loaded_by_reference()
        {
            using (var context = new PrivacyContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                var two = context.Twos.Single();
                context.Entry(two).Reference(e => e.RefRel).Load();
                var one = two.RefRel;

                Assert.NotEqual(0, one.Id);
                Assert.Equal("Right Here, Right Now", one.Name);
                Assert.Equal(AnEnum.Beat, one.AnEnum);
                Assert.Same(one, two.RefRel);
                Assert.Equal(one.Id, two.RefRelId);
            }
        }

        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_manipulated()
        {
            using (var context = new PrivacyContext())
            {
                var one = context.Ones.Single();
                var two = context.Twos.Single();

                var nameProperty = context.Entry(one).Property(e => e.Name);
                nameProperty.CurrentValue = "Praise You";
                nameProperty.OriginalValue = "Right Now, Right Here";

                Assert.Equal("Praise You", one.Name);
                Assert.Equal("Praise You", nameProperty.CurrentValue);
                Assert.Equal("Right Now, Right Here", nameProperty.OriginalValue);

                var enumProperty = context.Entry(one).Property(e => e.AnEnum);
                enumProperty.CurrentValue = AnEnum.Big;
                enumProperty.OriginalValue = AnEnum.Big;

                Assert.Equal(AnEnum.Big, one.AnEnum);
                Assert.Equal(AnEnum.Big, enumProperty.CurrentValue);
                Assert.Equal(AnEnum.Big, enumProperty.OriginalValue);

                var imageProperty = context.Entry(one).ComplexProperty(e => e.Info).Property(c => c.Image);
                imageProperty.CurrentValue = new byte[8];
                imageProperty.OriginalValue = new byte[24];

                Assert.Equal(8, one.Info.Image.Length);
                Assert.Equal(8, imageProperty.CurrentValue.Length);
                Assert.Equal(24, imageProperty.OriginalValue.Length);

                var colRel = context.Entry(one).Collection(e => e.ColRel);
                var newCol = new List<ActualEntity2>
                    {
                        new ActualEntity2(748, "Slash Dot Dash", AnEnum.Beat, 0, null)
                    };
                colRel.CurrentValue = newCol;

                Assert.Same(newCol, one.ColRel);
                Assert.Same(newCol, colRel.CurrentValue);

                var refRel = context.Entry(two).Reference(e => e.RefRel);
                var newRef = new ActualEntity1(0, "Slash Dot Dash", AnEnum.Beat, new ActualComplex(new byte[10]), null);
                refRel.CurrentValue = newRef;

                Assert.Same(newRef, two.RefRel);
                Assert.Same(newRef, refRel.CurrentValue);

                var fkProperty = context.Entry(two).Property(e => e.RefRelId);
                fkProperty.CurrentValue = 678;
                fkProperty.OriginalValue = 789;

                Assert.Equal(678, two.RefRelId);
                Assert.Equal(678, fkProperty.CurrentValue);
                Assert.Equal(789, fkProperty.OriginalValue);
            }
        }

        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_queried_to_non_entity_type()
        {
            using (var context = new PrivacyContext())
            {
                var non = context.Database.SqlQuery<NonEntity>("select * from ActualEntity2").Single();
                Assert.Equal(747, non.Id);
                Assert.Equal("Sunset", non.Name);
                Assert.Equal(AnEnum.Beat, non.AnEnum);
            }
        }

        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_queried_with_no_set_raw_SQL()
        {
            using (var context = new PrivacyContext())
            {
                var two = context.Database.SqlQuery<ActualEntity2>("select * from ActualEntity2").Single();
                Assert.Equal(747, two.Id);
                Assert.Equal("Sunset", two.Name);
                Assert.Equal(AnEnum.Beat, two.AnEnum);
            }
        }

        [Fact] // CodePlex 137
        public void Entities_with_unmapped_base_class_with_private_property_setters_can_be_queried_with_set_raw_SQL()
        {
            using (var context = new PrivacyContext())
            {
                context.Configuration.LazyLoadingEnabled = false;
                var one = context.Ones.Single();

                var two = context.Twos.SqlQuery("select * from ActualEntity2").Single();
                Assert.Equal(747, two.Id);
                Assert.Equal("Sunset", two.Name);
                Assert.Same(one, two.RefRel);
                Assert.Equal(AnEnum.Beat, two.AnEnum);
                Assert.Equal(one.Id, two.RefRelId);
            }
        }

        public class PrivacyContext : DbContext
        {
            static PrivacyContext()
            {
                Database.SetInitializer(new PrivacyInitializer());
            }

            public DbSet<ActualEntity1> Ones { get; set; }
            public DbSet<ActualEntity2> Twos { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ActualEntity2>().Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            }
        }

        public class PrivacyInitializer : DropCreateDatabaseIfModelChanges<PrivacyContext>
        {
            protected override void Seed(PrivacyContext context)
            {
                context.Ones.Add(
                    new ActualEntity1(
                        1, "Right Here, Right Now", AnEnum.Beat, new ActualComplex(new byte[16]), new List<ActualEntity2>
                            {
                                new ActualEntity2(747, "Sunset", AnEnum.Beat, 1, null)
                            }));
            }
        }

        public enum AnEnum
        {
            Big = 2,
            Beat = 3,
        }

        public class EntityBaseBase
        {
            public EntityBaseBase()
            {
            }

            public EntityBaseBase(int id, string name, AnEnum anEnum)
            {
                Id = id;
                Name = name;
                AnEnum = anEnum;
            }

            public int Id { get; private set; }
            public string Name { get; private set; }
            public AnEnum AnEnum { get; private set; }

            public void SetName(string name)
            {
                Name = name;
            }

            public void SetAnEnum(AnEnum anEnum)
            {
                AnEnum = anEnum;
            }
        }

        public class EntityBase1 : EntityBaseBase
        {
            public EntityBase1()
            {
            }

            public EntityBase1(int id, string name, AnEnum anEnum, ActualComplex info, ICollection<ActualEntity2> colRel)
                : base(id, name, anEnum)
            {
                Info = info;
                ColRel = colRel;
            }

            public ActualComplex Info { get; private set; }
            public virtual ICollection<ActualEntity2> ColRel { get; private set; }

            public void SetInfo(ActualComplex info)
            {
                Info = info;
            }

            public void SetColRel(ICollection<ActualEntity2> colRel)
            {
                ColRel = colRel;
            }
        }

        public class EntityBase2 : EntityBaseBase
        {
            public EntityBase2()
            {
            }

            public EntityBase2(int id, string name, AnEnum anEnum, int refRelId, ActualEntity1 refRel)
                : base(id, name, anEnum)
            {
                RefRelId = refRelId;
                RefRel = refRel;
            }

            public int RefRelId { get; private set; }
            public virtual ActualEntity1 RefRel { get; private set; }

            public void SetRefRelId(int refRelId)
            {
                RefRelId = refRelId;
            }

            public void SetRefRel(ActualEntity1 refRel)
            {
                RefRel = refRel;
            }
        }

        public class ActualEntity1 : EntityBase1
        {
            public ActualEntity1()
            {
            }

            public ActualEntity1(int id, string name, AnEnum anEnum, ActualComplex info, ICollection<ActualEntity2> colRel)
                : base(id, name, anEnum, info, colRel)
            {
            }
        }

        public class ActualEntity2 : EntityBase2
        {
            public ActualEntity2()
            {
            }

            public ActualEntity2(int id, string name, AnEnum anEnum, int refRelId, ActualEntity1 refRel)
                : base(id, name, anEnum, refRelId, refRel)
            {
            }
        }

        public class NonEntity : EntityBaseBase
        {
        }

        public class ComplexBase
        {
            public ComplexBase()
            {
            }

            public ComplexBase(byte[] image)
            {
                Image = image;
            }

            public byte[] Image { get; private set; }

            public void SetImage(byte[] image)
            {
                Image = image;
            }
        }

        public class ActualComplex : ComplexBase
        {
            public ActualComplex()
            {
            }

            public ActualComplex(byte[] image)
                : base(image)
            {
            }
        }
    }
}
