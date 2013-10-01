// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ProviderAgnosticModel
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class ProviderAgnosticContext : DbContext
    {
        public ProviderAgnosticContext()
            : base("ProviderAgnosticContext")
        {
            Configuration.ValidateOnSaveEnabled = false;
        }

        static ProviderAgnosticContext()
        {
            Database.SetInitializer(new ProviderAgnosticContextInitializer());
        }

        public DbSet<Bug> Bugs { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<Failure> Failures { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Run> Runs { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<AllTypes> AllTypes { get; set; }

        public DbSet<Gear> Gears { get; set; }
        public DbSet<Squad> Squads { get; set; }
        public DbSet<CogTag> Tags { get; set; }
        public DbSet<Weapon> Weapons { get; set; }
        public DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // entity splitting
            modelBuilder.Entity<Bug>().Map(m =>
            {
                m.Properties(p => new { p.Id, p.Comment });
                m.ToTable("Bugs1");
            });
            modelBuilder.Entity<Bug>().Map(m =>
            {
                m.Properties(p => new { p.Number, p.Resolution });
                m.ToTable("Bugs2");
            });

            modelBuilder.Entity<Task>().HasKey(k => new { k.Id, k.Name });
            modelBuilder.Entity<Run>().HasMany(r => r.Tasks).WithRequired().Map(m => { });
            modelBuilder.Entity<Owner>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<Run>().Property(p => p.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            modelBuilder.Entity<Run>().HasRequired(r => r.RunOwner).WithRequiredDependent(o => o.OwnedRun);

            modelBuilder.Entity<AllTypes>().Property(p => p.DecimalProperty).HasPrecision(28, 4);
            modelBuilder.Entity<AllTypes>().Property(p => p.FixedLengthBinaryProperty).HasMaxLength(255).IsFixedLength();
            modelBuilder.Entity<AllTypes>().Property(p => p.FixedLengthStringProperty).HasMaxLength(255).IsFixedLength().IsUnicode(false);
            modelBuilder.Entity<AllTypes>().Property(p => p.FixedLengthUnicodeStringProperty).HasMaxLength(255).IsFixedLength();
            modelBuilder.Entity<AllTypes>().Property(p => p.VariableLengthBinaryProperty).HasMaxLength(255).IsVariableLength();
            modelBuilder.Entity<AllTypes>().Property(p => p.VariableLengthStringProperty).HasMaxLength(255).IsVariableLength().IsUnicode(false);
            modelBuilder.Entity<AllTypes>().Property(p => p.VariableLengthUnicodeStringProperty).HasMaxLength(255).IsVariableLength();

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
