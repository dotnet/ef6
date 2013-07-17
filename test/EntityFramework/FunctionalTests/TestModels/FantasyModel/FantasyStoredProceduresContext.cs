// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    public class FantasyStoredProceduresContext : FantasyContext
    {
        static FantasyStoredProceduresContext()
        {
            Database.SetInitializer(new FantasyStoredProceduresInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Building>().MapToStoredProcedures();
            modelBuilder.Entity<City>().MapToStoredProcedures();
            modelBuilder.Entity<Creature>().MapToStoredProcedures();
            modelBuilder.Entity<Province>().MapToStoredProcedures();
            modelBuilder.Entity<Npc>().MapToStoredProcedures();
            modelBuilder.Entity<Perk>().MapToStoredProcedures();
            modelBuilder.Entity<Race>().MapToStoredProcedures();
            modelBuilder.Entity<Skill>().MapToStoredProcedures();
            modelBuilder.Entity<Spell>().MapToStoredProcedures();
            modelBuilder.Entity<Landmark>().MapToStoredProcedures();
            modelBuilder.Entity<Tower>().MapToStoredProcedures();
            modelBuilder.Entity<Carnivore>().HasMany(c => c.Eats).WithMany().MapToStoredProcedures();
            modelBuilder.Entity<Omnivore>().HasMany(c => c.Eats).WithMany().MapToStoredProcedures();
            modelBuilder.Entity<Perk>().HasMany(p => p.RequiredBy).WithMany(p => p.RequiredPerks).MapToStoredProcedures();
            modelBuilder.Entity<Race>().HasMany(r => r.SkillBonuses).WithMany().MapToStoredProcedures();
            modelBuilder.Entity<Spell>().HasMany(s => s.SynergyWith).WithMany().MapToStoredProcedures();
        }
    }
}
