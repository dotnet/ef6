// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class FantasyContext : DbContext
    {
        static FantasyContext()
        {
            Database.SetInitializer(new FantasyInitializer());
        }

        public DbSet<Building> Buildings { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Creature> Creatures { get; set; }
        public DbSet<Npc> Npcs { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Landmark> Landmarks { get; set; }
        public DbSet<Perk> Perks { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Tower> Towers { get; set; }
        public DbSet<Spell> Spells { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Creature>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            
            // if this is not added, we throw bad exception
            modelBuilder.Entity<Carnivore>().HasMany(c => c.Eats).WithMany();
            modelBuilder.Entity<Omnivore>().HasMany(c => c.Eats).WithMany();

            modelBuilder.Entity<Skill>().HasKey(s => new { s.Archetype, s.Ordinal });

            // identity guid
            modelBuilder.Entity<Perk>().Property(p => p.PerkId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            // composite foreign key
            modelBuilder.Entity<Perk>()
                .HasRequired(p => p.Skill)
                .WithMany(s => s.Perks)
                .HasForeignKey(p => new { p.SkillArchetype, p.SkillOrdinal });

            // many - many self reference, 2 properties
            modelBuilder.Entity<Perk>().HasMany(p => p.RequiredBy).WithMany(p => p.RequiredPerks);

            modelBuilder.Entity<Race>().HasKey(r => r.RaceName);

            // many - many uni-directional
            modelBuilder.Entity<Race>().HasMany(r => r.SkillBonuses).WithMany();

            modelBuilder.Entity<Spell>().ToTable("Spells");
            modelBuilder.Entity<CombatSpell>().ToTable("CombatSpells");
            modelBuilder.Entity<SupportSpell>().ToTable("SupportSpells");

            // many - many self reference 1 property
            modelBuilder.Entity<Spell>().HasMany(s => s.SynergyWith).WithMany();

            modelBuilder.Entity<Province>().HasMany(h => h.Cities).WithRequired(c => c.Province).HasForeignKey(c => c.ProvinceId);

            // table splitting
            modelBuilder.Entity<Landmark>().ToTable("Landmarks");
            modelBuilder.Entity<Tower>().ToTable("Landmarks");

            // 1 -1 relationship needed for table splitting with different hierarchies
            modelBuilder.Entity<Landmark>().HasRequired(l => l.MatchingTower).WithRequiredPrincipal(s => s.MatchingLandnmark);
            modelBuilder.Entity<Landmark>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<Tower>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
        }
    }
}
