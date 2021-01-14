// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations.Infrastructure.FunctionsModel;
    using System.Data.Entity.Spatial;
    using System.Linq;
    using System.Xml;
    using Xunit;

    public class ModificationCommandTreeGeneratorTests : TestBase
    {
        private class WorldContext_Identity : WorldContext_Fk
        {
            static WorldContext_Identity()
            {
                Database.SetInitializer<WorldContext_Identity>(null);
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Thing>().Property(t => t.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            }
        }

        [Fact]
        public void Can_generate_insert_tree_when_self_ref_direct()
        {
            DbModel model;

            using (var context = new WorldContext_Identity())
            {
                model = context.InternalContext.CodeFirstModel.CachedModelBuilder.BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator = new ModificationCommandTreeGenerator(model);

            var commandTrees = commandTreeGenerator.GenerateInsert(GetType().Namespace + ".Thing").ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_update_tree_when_self_ref_direct()
        {
            DbModel model;

            using (var context = new WorldContext_Identity())
            {
                model = context.InternalContext.CodeFirstModel.CachedModelBuilder.BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator = new ModificationCommandTreeGenerator(model);

            var commandTrees = commandTreeGenerator.GenerateUpdate(GetType().Namespace + ".Thing").ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_tree_when_self_ref_direct()
        {
            DbModel model;

            using (var context = new WorldContext_Identity())
            {
                model = context.InternalContext.CodeFirstModel.CachedModelBuilder.BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator = new ModificationCommandTreeGenerator(model);

            var commandTrees = commandTreeGenerator.GenerateDelete(GetType().Namespace + ".Thing").ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        public abstract class MessageBase
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Contents { get; set; }
        }

        public class Message : MessageBase
        {
        }

        public class Comment : MessageBase
        {
            public MessageBase Parent { get; set; }
            public int ParentId { get; set; }
        }

        public class SelfRefInheritanceContext : DbContext
        {
            public DbSet<Comment> Comments { get; set; }
            public DbSet<MessageBase> MessageBases { get; set; }
            public DbSet<Message> Messages { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<MessageBase>().MapToStoredProcedures();
            }
        }

        [Fact]
        public void Can_generate_insert_tree_when_self_ref_inheritance()
        {
            DbModel model;

            using (var context = new SelfRefInheritanceContext())
            {
                model = context.InternalContext.CodeFirstModel.CachedModelBuilder.BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator = new ModificationCommandTreeGenerator(model);

            var commandTrees = commandTreeGenerator.GenerateInsert(GetType().Namespace + ".Comment").ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_update_tree_when_self_ref_inheritance()
        {
            DbModel model;

            using (var context = new SelfRefInheritanceContext())
            {
                model = context.InternalContext.CodeFirstModel.CachedModelBuilder.BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator = new ModificationCommandTreeGenerator(model);

            var commandTrees = commandTreeGenerator.GenerateUpdate(GetType().Namespace + ".Comment").ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_tree_when_self_ref_inheritance()
        {
            DbModel model;

            using (var context = new SelfRefInheritanceContext())
            {
                model = context.InternalContext.CodeFirstModel.CachedModelBuilder.BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator = new ModificationCommandTreeGenerator(model);

            var commandTrees = commandTreeGenerator.GenerateDelete(GetType().Namespace + ".Comment").ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        public class Landmark
        {
            public int Id { get; set; }
            
            [Required]
            public string Name { get; set; }
            
            public StandingStone MatchingStone { get; set; }
        }

        public class StandingStone
        {
            public int Id { get; set; }
            public Landmark MatchingLandmark { get; set; }

            [Required]
            public string Rune { get; set; }

            public decimal Height { get; set; }
        }

        public class OneToOneFkContext : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Landmark>()
                    .HasRequired(l => l.MatchingStone)
                    .WithRequiredPrincipal(s => s.MatchingLandmark);

                modelBuilder.Entity<Landmark>().MapToStoredProcedures();
                modelBuilder.Entity<StandingStone>().MapToStoredProcedures();
            }
        }

        [Fact]
        public void Can_generate_insert_tree_when_one_to_one_fk_principal()
        {
            DbModel model;

            using (var context = new OneToOneFkContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".Landmark")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_update_tree_when_one_to_one_fk_principal()
        {
            DbModel model;

            using (var context = new OneToOneFkContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".Landmark")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_tree_when_one_to_one_fk_principal()
        {
            DbModel model;

            using (var context = new OneToOneFkContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".Landmark")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_insert_tree_when_one_to_one_fk_dependent()
        {
            DbModel model;

            using (var context = new OneToOneFkContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".StandingStone")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_update_tree_when_one_to_one_fk_dependent()
        {
            DbModel model;

            using (var context = new OneToOneFkContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".StandingStone")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_tree_when_one_to_one_fk_dependent()
        {
            DbModel model;

            using (var context = new OneToOneFkContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".StandingStone")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        public class TableSplittingContext : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Landmark>().ToTable("Landmarks");
                modelBuilder.Entity<StandingStone>().ToTable("Landmarks");

                modelBuilder.Entity<Landmark>().HasRequired(l => l.MatchingStone).WithRequiredPrincipal(s => s.MatchingLandmark);
                modelBuilder.Entity<Landmark>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
                modelBuilder.Entity<StandingStone>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

                modelBuilder.Entity<Landmark>().MapToStoredProcedures();
                modelBuilder.Entity<StandingStone>().MapToStoredProcedures();
            }
        }

        [Fact]
        public void Can_generate_insert_tree_when_table_splitting_principal()
        {
            DbModel model;

            using (var context = new TableSplittingContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".Landmark")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_update_tree_when_table_splitting_principal()
        {
            DbModel model;

            using (var context = new TableSplittingContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".Landmark")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_tree_when_table_splitting_principal()
        {
            DbModel model;

            using (var context = new TableSplittingContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".Landmark")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_insert_tree_when_table_splitting_dependent()
        {
            DbModel model;

            using (var context = new TableSplittingContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".StandingStone")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
            Assert.IsType<DbUpdateCommandTree>(commandTrees.Single());
        }

        [Fact]
        public void Can_generate_update_tree_when_table_splitting_dependent()
        {
            DbModel model;

            using (var context = new TableSplittingContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".StandingStone")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
            Assert.IsType<DbUpdateCommandTree>(commandTrees.Single());
        }

        [Fact]
        public void Can_generate_delete_tree_when_table_splitting_dependent()
        {
            DbModel model;

            using (var context = new TableSplittingContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".StandingStone")
                    .ToList();

            Assert.Equal(0, commandTrees.Count());
        }

        public class ArubaRun
        {
            public string Id { get; set; }
            public ICollection<ArubaTask> Tasks { get; set; }
        }

        public class ArubaTask
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class ArubaContext : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ArubaRun>().HasMany(r => r.Tasks).WithRequired();
            }
        }

        [Fact]
        public void Can_generate_insert_tree_when_ia_required_to_many()
        {
            DbModel model;

            using (var context = new ArubaContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".ArubaTask")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_update_tree_when_ia_required_to_many()
        {
            DbModel model;

            using (var context = new ArubaContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".ArubaTask")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_tree_when_ia_required_to_many()
        {
            DbModel model;

            using (var context = new ArubaContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".ArubaTask")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        public enum MilitaryRank
        {
            Private,
            Major,
            Colonel,
            General,
        };

        public class EnumKey
        {
            public MilitaryRank Id { get; set; }
            public string Name { get; set; }
        }

        public class EnumKeyContext : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<EnumKey>().MapToStoredProcedures();
            }
        }

        [Fact]
        public void Can_generate_insert_tree_when_enum_key()
        {
            DbModel model;

            using (var context = new EnumKeyContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".EnumKey")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_update_tree_when_enum_key()
        {
            DbModel model;

            using (var context = new EnumKeyContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".EnumKey")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_tree_when_enum_key()
        {
            DbModel model;

            using (var context = new EnumKeyContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".EnumKey")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        public class ArubaPerson
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ArubaPerson Partner { get; set; }
            public ICollection<ArubaPerson> Children { get; set; }
            public ICollection<ArubaPerson> Parents { get; set; }
        }

        public class ManyToManySelfRef : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ArubaPerson>().HasOptional(p => p.Partner).WithOptionalPrincipal();
                modelBuilder.Entity<ArubaPerson>().HasMany(p => p.Children).WithMany(p => p.Parents);
            }
        }

        [Fact]
        public void Can_generate_insert_association_tree_when_many_to_many_self_ref()
        {
            DbModel model;

            using (var context = new ManyToManySelfRef())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateAssociationInsert(GetType().Namespace + ".ArubaPerson_Children")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_association_tree_when_many_to_many_self_ref()
        {
            DbModel model;

            using (var context = new ManyToManySelfRef())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateAssociationDelete(GetType().Namespace + ".ArubaPerson_Children")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        public abstract class WeaponBase
        {
            public int Id { get; set; }

            // 1 - 1 self reference
            public virtual WeaponBase SynergyWith { get; set; }

            public Ammo Ammo { get; set; }
        }

        public class HeavyishWeapon : WeaponBase
        {
            public bool Overheats { get; set; }
        }

        [ComplexType]
        public class Ammo
        {
            public string MagazineType { get; set; }
        }

        public class GearsModelOneToOneSelfRef : DbContext
        {
            public DbSet<WeaponBase> Weapons { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<WeaponBase>().HasOptional(w => w.SynergyWith).WithOptionalPrincipal();
            }
        }

        [Fact]
        public void Can_generate_insert_tree_when_one_to_one_self_ref()
        {
            DbModel model;

            using (var context = new GearsModelOneToOneSelfRef())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".WeaponBase")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_update_tree_when_one_to_one_self_ref()
        {
            DbModel model;

            using (var context = new GearsModelOneToOneSelfRef())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".WeaponBase")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_tree_when_one_to_one_self_ref()
        {
            DbModel model;

            using (var context = new GearsModelOneToOneSelfRef())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".WeaponBase")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        public abstract class GearBase
        {
            public int Id { get; set; }
            public virtual ICollection<Weapon> Weapons { get; set; }
        }

        public class AdvancedGear : GearBase
        {
        }

        public abstract class Weapon
        {
            public int Id { get; set; }
        }

        public abstract class HeavyWeapon : Weapon
        {
            public bool Overheats { get; set; }
        }

        public class TheBfg : HeavyWeapon
        {
        }

        public class GearsModelManyToMany : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<GearBase>().HasMany(g => g.Weapons).WithMany().MapToStoredProcedures();
            }
        }

        [Fact]
        public void Can_generate_insert_association_tree_when_many_to_many_with_abstract_target_end()
        {
            DbModel model;

            using (var context = new GearsModelManyToMany())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateAssociationInsert(GetType().Namespace + ".GearBase_Weapons")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_association_tree_when_many_to_many_with_abstract_end()
        {
            DbModel model;

            using (var context = new GearsModelManyToMany())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateAssociationDelete(GetType().Namespace + ".GearBase_Weapons")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        public class Gear2
        {
            public int Id { get; set; }
            public virtual ICollection<Weapon> Weapons { get; set; }
        }

        public class StandardWeapon : Weapon
        {
        }

        public class GearsModel1289 : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Gear2>().HasMany(g => g.Weapons).WithMany().MapToStoredProcedures();
            }
        }

        [Fact] // CodePlex 1289
        public void Can_generate_insert_association_tree_when_many_to_many_with_non_abstract_base_and_abstract_target()
        {
            DbModel model;

            using (var context = new GearsModel1289())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateAssociationInsert(GetType().Namespace + ".Gear2_Weapons")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact] // CodePlex 1289
        public void Can_generate_insert_association_tree_when_many_to_many_with_non_abstract_base_and_abstract_end()
        {
            DbModel model;

            using (var context = new GearsModel1289())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateAssociationDelete(GetType().Namespace + ".Gear2_Weapons")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        public class CogTag
        {
            public string Key1 { get; set; }
            public DbGeometry Key2 { get; set; }
            public DbGeography Key3 { get; set; }
            public byte[] Key4 { get; set; }
            public short Key5 { get; set; }
            public DateTime Key6 { get; set; }
            public bool Key7 { get; set; }
            public virtual Gear Gear { get; set; }
        }

        public class Gear
        {
            public int Id { get; set; }
            public string Nickname { get; set; }
            public virtual CogTag Tag { get; set; }
        }

        public class GearsOfWarContextSPBug : DbContext
        {
            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Gear>()
                    .MapToStoredProcedures()
                    .HasRequired(g => g.Tag)
                    .WithOptional(t => t.Gear);

                modelBuilder.Entity<CogTag>()
                    .HasKey(k => new { k.Key1, k.Key2, k.Key3, k.Key4, k.Key5, k.Key6, k.Key7 });
            }
        }

        [Fact]
        public void Can_generate_insert_tree_when_one_to_one_ia()
        {
            DbModel model;

            using (var context = new GearsOfWarContextSPBug())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".Gear")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_update_tree_when_one_to_one_ia()
        {
            DbModel model;

            using (var context = new GearsOfWarContextSPBug())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".Gear")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_delete_tree_when_one_to_one_ia()
        {
            DbModel model;

            using (var context = new GearsOfWarContextSPBug())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".Gear")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        public class WorldContext : DbContext
        {
            static WorldContext()
            {
                Database.SetInitializer<WorldContext>(null);
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Thing>()
                    .MapToStoredProcedures();
            }
        }

        public class WorldContext_Fk : WorldContext
        {
            static WorldContext_Fk()
            {
                Database.SetInitializer<WorldContext_Fk>(null);
            }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder
                    .Entity<Thing>()
                    .HasRequired(t => t.Container)
                    .WithMany()
                    .HasForeignKey(t => t.ContainerFk);
            }
        }

        public class Thing
        {
            public int Id { get; set; }
            public Thing Container { get; set; }
            public int ContainerFk { get; set; }
        }

        [Fact]
        public void Can_generate_dynamic_insert_command_trees_for_self_ref_fk()
        {
            DbModel model;

            using (var context = new WorldContext_Fk())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".Thing")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_dynamic_update_command_trees_for_self_ref_fk()
        {
            DbModel model;

            using (var context = new WorldContext_Fk())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".Thing")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_dynamic_delete_command_trees_for_self_ref_fk()
        {
            DbModel model;

            using (var context = new WorldContext_Fk())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".Thing")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_dynamic_insert_command_trees_for_self_ref_ia()
        {
            DbModel model;

            using (var context = new WorldContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".Thing")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_dynamic_update_command_trees_for_self_ref_ia()
        {
            DbModel model;

            using (var context = new WorldContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".Thing")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_dynamic_delete_command_trees_for_self_ref_ia()
        {
            DbModel model;

            using (var context = new WorldContext())
            {
                model
                    = context
                        .InternalContext
                        .CodeFirstModel
                        .CachedModelBuilder
                        .BuildDynamicUpdateModel(ProviderRegistry.Sql2008_ProviderInfo);
            }

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".Thing")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());
        }

        [Fact]
        public void Can_generate_dynamic_insert_command_trees_for_many_to_many_association()
        {
            var model = TestContext.CreateDynamicUpdateModel();

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateAssociationInsert(GetType().Namespace + ".FunctionsModel.OrderThing_Orders")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());

            var commandTree = commandTrees.First();

            Assert.Equal(5, commandTree.SetClauses.Count);
            Assert.Equal("OrderThingOrder", commandTree.Target.VariableType.EdmType.Name);
            Assert.Null(commandTree.Returning);
        }

        [Fact]
        public void Can_generate_dynamic_delete_command_trees_for_many_to_many_association()
        {
            var model = TestContext.CreateDynamicUpdateModel();

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateAssociationDelete(GetType().Namespace + ".FunctionsModel.OrderThing_Orders")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());

            var commandTree = commandTrees.First();

            Assert.Equal("OrderThingOrder", commandTree.Target.VariableType.EdmType.Name);
            Assert.NotNull(commandTree.Predicate);
        }

        [Fact]
        public void Can_generate_dynamic_insert_command_trees()
        {
            var model = TestContext.CreateDynamicUpdateModel();

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".FunctionsModel.SpecialOrder")
                    .ToList();

            Assert.Equal(2, commandTrees.Count());

            var commandTree = (DbInsertCommandTree)commandTrees.First();

            Assert.Equal(8, commandTree.SetClauses.Count);
            Assert.Equal("Order", commandTree.Target.VariableType.EdmType.Name);
            Assert.NotNull(commandTree.Returning);

            commandTree = (DbInsertCommandTree)commandTrees.Last();

            Assert.Equal(8, commandTree.SetClauses.Count);
            Assert.NotNull(commandTree.Returning);
            Assert.Equal("special_orders", commandTree.Target.VariableType.EdmType.Name);

            commandTrees
                = commandTreeGenerator
                    .GenerateInsert(GetType().Namespace + ".FunctionsModel.Customer")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());

            commandTree = (DbInsertCommandTree)commandTrees.Single();

            Assert.Equal(1, commandTree.SetClauses.Count);
            Assert.Equal("Customer", commandTree.Target.VariableType.EdmType.Name);
            Assert.NotNull(commandTree.Returning);
        }

        [Fact]
        public void Can_generate_dynamic_update_command_trees()
        {
            var model = TestContext.CreateDynamicUpdateModel();

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".FunctionsModel.SpecialOrder")
                    .ToList();

            Assert.Equal(2, commandTrees.Count());

            var commandTree = (DbUpdateCommandTree)commandTrees.First();

            Assert.Equal(6, commandTree.SetClauses.Count);
            Assert.NotNull(commandTree.Predicate);
            Assert.NotNull(commandTree.Returning);
            Assert.Equal("Order", commandTree.Target.VariableType.EdmType.Name);

            commandTree = (DbUpdateCommandTree)commandTrees.Last();

            Assert.Equal(4, commandTree.SetClauses.Count);
            Assert.NotNull(commandTree.Predicate);
            Assert.NotNull(commandTree.Returning);
            Assert.Equal("special_orders", commandTree.Target.VariableType.EdmType.Name);

            commandTrees
                = commandTreeGenerator
                    .GenerateUpdate(GetType().Namespace + ".FunctionsModel.Customer")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());

            commandTree = (DbUpdateCommandTree)commandTrees.Single();

            Assert.Equal(1, commandTree.SetClauses.Count);
            Assert.Equal("Customer", commandTree.Target.VariableType.EdmType.Name);
            Assert.Null(commandTree.Returning);
        }

        [Fact]
        public void Can_generate_dynamic_delete_command_trees()
        {
            var model = TestContext.CreateDynamicUpdateModel();

            var commandTreeGenerator
                = new ModificationCommandTreeGenerator(model);

            var commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".FunctionsModel.SpecialOrder")
                    .ToList();

            Assert.Equal(2, commandTrees.Count());

            var commandTree = (DbDeleteCommandTree)commandTrees.First();

            Assert.NotNull(commandTree.Predicate);
            Assert.Equal("special_orders", commandTree.Target.VariableType.EdmType.Name);

            commandTree = (DbDeleteCommandTree)commandTrees.Last();

            Assert.NotNull(commandTree.Predicate);
            Assert.Equal("Order", commandTree.Target.VariableType.EdmType.Name);

            commandTrees
                = commandTreeGenerator
                    .GenerateDelete(GetType().Namespace + ".FunctionsModel.Customer")
                    .ToList();

            Assert.Equal(1, commandTrees.Count());

            commandTree = (DbDeleteCommandTree)commandTrees.Single();

            Assert.NotNull(commandTree.Predicate);
            Assert.Equal("Customer", commandTree.Target.VariableType.EdmType.Name);
        }
    }
}
