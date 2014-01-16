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
            modelBuilder.Entity<Gear>().HasTableAnnotation("Annotation_Gear", "Step to West 17");
            
            modelBuilder.Entity<Gear>()
                .Property(g => g.Rank)
                .HasColumnAnnotation("Annotation_Rank", "Love not war!")
                .IsConcurrencyToken();

            modelBuilder.Entity<City>()
                .HasTableAnnotation("Annotation_City1", "The Short Earth")
                .HasTableAnnotation("Annotation_City2", "It's a Joker!")
                .HasTableAnnotation("Annotation_City1", "The Long Earth")
                .HasTableAnnotation("Annotation_City3", "Natural Stepper")
                .HasTableAnnotation("Annotation_City2", null)
                .HasKey(c => c.Name);

            modelBuilder.Entity<Squad>()
                .HasTableAnnotation("Annotation_Squad1", "Happy Place")
                .Property(s => s.Id)
                .HasColumnAnnotation("Annotation_Id", "All you need is love...")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<Squad>()
                .HasTableAnnotation("Annotation_Squad2", "Happy Planet")
                .Property(s => s.InternalNumber)
                .HasColumnAnnotation("Annotation_InternalNumber", "She loves me, yeah, yeah, yeah.")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Weapon>().HasOptional(w => w.SynergyWith).WithOptionalPrincipal();

            modelBuilder.Entity<CogTag>()
                .HasTableAnnotation("Annotation_CogTag", "It's an elf!")
                .Property(t => t.Note)
                .HasColumnAnnotation("Annotation_Note", "...living life in peace.")
                .HasMaxLength(40);

            modelBuilder.ComplexType<WeaponSpecification>()
                .Property(c => c.AmmoPerClip)
                .HasColumnAnnotation("Annotation_AmmoPerClip", "Let It Be");
        }
    }
}
