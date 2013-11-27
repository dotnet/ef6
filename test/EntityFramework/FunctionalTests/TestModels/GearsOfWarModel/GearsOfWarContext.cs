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
            modelBuilder.Entity<Gear>().HasAnnotation("Annotation_Gear", "Step to West 17");
            
            modelBuilder.Entity<Gear>()
                .Property(g => g.Rank)
                .HasAnnotation("Annotation_Rank", "Love not war!")
                .IsConcurrencyToken();

            modelBuilder.Entity<City>()
                .HasAnnotation("Annotation_City1", "The Short Earth")
                .HasAnnotation("Annotation_City2", "It's a Joker!")
                .HasAnnotation("Annotation_City1", "The Long Earth")
                .HasAnnotation("Annotation_City3", "Natural Stepper")
                .HasAnnotation("Annotation_City2", null)
                .HasKey(c => c.Name);

            modelBuilder.Entity<Squad>()
                .HasAnnotation("Annotation_Squad1", "Happy Place")
                .Property(s => s.Id)
                .HasAnnotation("Annotation_Id", "All you need is love...")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<Squad>()
                .HasAnnotation("Annotation_Squad2", "Happy Planet")
                .Property(s => s.InternalNumber)
                .HasAnnotation("Annotation_InternalNumber", "She loves me, yeah, yeah, yeah.")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Weapon>().HasOptional(w => w.SynergyWith).WithOptionalPrincipal();

            modelBuilder.Entity<CogTag>()
                .HasAnnotation("Annotation_CogTag", "It's an elf!")
                .Property(t => t.Note)
                .HasAnnotation("Annotation_Note", "...living life in peace.")
                .HasMaxLength(40);

            modelBuilder.ComplexType<WeaponSpecification>()
                .Property(c => c.AmmoPerClip)
                .HasAnnotation("Annotation_AmmoPerClip", "Let It Be");
        }
    }
}
