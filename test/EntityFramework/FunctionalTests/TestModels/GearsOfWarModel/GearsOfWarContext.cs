// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.GearsOfWarModel
{
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Created for the purpose of update tests
    /// </summary>
    public class GearsOfWarContext : DbContext
    {
        public GearsOfWarContext()
        {
            Configuration.ValidateOnSaveEnabled = false;
        }

        static GearsOfWarContext()
        {
            Database.SetInitializer(new GearsOfWarInitializer());
        }

        public DbSet<Gear> Gears { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<CogTag> Tags { get; set; }
        public DbSet<Weapon> Weapons { get; set; }
        public DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Gear>().HasKey(g => new { g.SquadId, g.Nickname });
            modelBuilder.Entity<Gear>().HasRequired(g => g.Tag).WithOptional(t => t.Gear);
            modelBuilder.Entity<Gear>().HasMany(g => g.Weapons).WithMany();
            modelBuilder.Entity<Gear>().HasOptional(g => g.CityOfBirth).WithMany();
            modelBuilder.Entity<Gear>().HasRequired(g => g.Squad).WithMany(g => g.Members);
            modelBuilder.Entity<Gear>().Property(g => g.Rank).IsConcurrencyToken();

            modelBuilder.Entity<City>().HasKey(c => c.Name);

            modelBuilder.Entity<Squad>().Property(s => s.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<Squad>().Property(s => s.InternalNumber)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Weapon>().HasOptional(w => w.SynergyWith).WithOptionalPrincipal();

            modelBuilder.Entity<CogTag>().Property(t => t.Note).HasMaxLength(40);
        }
    }
}
