using System.Data.Entity;
using System.Data.Common;
using Xugu.Data.EntityFramework;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    [DbConfigurationType(typeof(XGEFConfiguration))]
    public partial class SakilaDb : DbContext
    {

        public SakilaDb()
        : base(CodeFirstFixture.GetEFConnectionString<SakilaDb>("sakila"))
        {
            //Database.SetInitializer<SakilaDb>(new DropCreateDatabaseAlways<SakilaDb>() );
        }

        public SakilaDb(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        public virtual DbSet<actor> actors { get; set; }
        public virtual DbSet<address> addresses { get; set; }
        public virtual DbSet<category> categories { get; set; }
        public virtual DbSet<city> cities { get; set; }
        public virtual DbSet<country> countries { get; set; }
        public virtual DbSet<customer> customers { get; set; }
        public virtual DbSet<film> films { get; set; }
        public virtual DbSet<film_actor> film_actor { get; set; }
        public virtual DbSet<film_category> film_category { get; set; }
        public virtual DbSet<film_text> film_text { get; set; }
        public virtual DbSet<inventory> inventories { get; set; }
        public virtual DbSet<language> languages { get; set; }
        public virtual DbSet<payment> payments { get; set; }
        public virtual DbSet<rental> rentals { get; set; }
        public virtual DbSet<staff> staffs { get; set; }
        public virtual DbSet<store> stores { get; set; }
        public virtual DbSet<actor_info> actor_info { get; set; }
        public virtual DbSet<customer_list> customer_list { get; set; }
        public virtual DbSet<film_list> film_list { get; set; }
        public virtual DbSet<nicer_but_slower_film_list> nicer_but_slower_film_list { get; set; }
        public virtual DbSet<sales_by_film_category> sales_by_film_category { get; set; }
        public virtual DbSet<sales_by_store> sales_by_store { get; set; }
        public virtual DbSet<staff_list> staff_list { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("sakila");
            modelBuilder.Entity<actor>()
                .Property(e => e.first_name)
                .IsUnicode(false);

            modelBuilder.Entity<actor>()
                .Property(e => e.last_name)
                .IsUnicode(false);

            modelBuilder.Entity<actor>()
                .HasMany(e => e.film_actor)
                .WithRequired(e => e.actor)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<address>()
                .Property(e => e.address1)
                .IsUnicode(false);

            modelBuilder.Entity<address>()
                .Property(e => e.address2)
                .IsUnicode(false);

            modelBuilder.Entity<address>()
                .Property(e => e.district)
                .IsUnicode(false);

            modelBuilder.Entity<address>()
                .Property(e => e.postal_code)
                .IsUnicode(false);

            modelBuilder.Entity<address>()
                .Property(e => e.phone)
                .IsUnicode(false);

            modelBuilder.Entity<address>()
                .HasMany(e => e.customers)
                .WithRequired(e => e.address)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<address>()
                .HasMany(e => e.staffs)
                .WithRequired(e => e.address)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<address>()
                .HasMany(e => e.stores)
                .WithRequired(e => e.address)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<category>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<category>()
                .HasMany(e => e.film_category)
                .WithRequired(e => e.category)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<city>()
                .Property(e => e.city1)
                .IsUnicode(false);

            modelBuilder.Entity<city>()
                .HasMany(e => e.addresses)
                .WithRequired(e => e.city)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<country>()
                .Property(e => e.country1)
                .IsUnicode(false);

            modelBuilder.Entity<country>()
                .HasMany(e => e.cities)
                .WithRequired(e => e.country)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<customer>()
                .Property(e => e.first_name)
                .IsUnicode(false);

            modelBuilder.Entity<customer>()
                .Property(e => e.last_name)
                .IsUnicode(false);

            modelBuilder.Entity<customer>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<customer>()
                .HasMany(e => e.payments)
                .WithRequired(e => e.customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<customer>()
                .HasMany(e => e.rentals)
                .WithRequired(e => e.customer)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<film>()
                .Property(e => e.title)
                .IsUnicode(false);

            modelBuilder.Entity<film>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<film>()
                .Property(e => e.rating)
                .IsUnicode(false);

            modelBuilder.Entity<film>()
                .Property(e => e.special_features)
                .IsUnicode(false);

            modelBuilder.Entity<film>()
                .HasMany(e => e.film_actor)
                .WithRequired(e => e.film)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<film>()
                .HasMany(e => e.film_category)
                .WithRequired(e => e.film)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<film>()
                .HasMany(e => e.inventories)
                .WithRequired(e => e.film)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<film_text>()
                .Property(e => e.title)
                .IsUnicode(false);

            modelBuilder.Entity<film_text>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<inventory>()
                .HasMany(e => e.rentals)
                .WithRequired(e => e.inventory)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<language>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<language>()
                .HasMany(e => e.films)
                .WithRequired(e => e.language)
                .HasForeignKey(e => e.language_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<language>()
                .HasMany(e => e.films1)
                .WithOptional(e => e.language1)
                .HasForeignKey(e => e.original_language_id);

            modelBuilder.Entity<staff>()
                .Property(e => e.first_name)
                .IsUnicode(false);

            modelBuilder.Entity<staff>()
                .Property(e => e.last_name)
                .IsUnicode(false);

            modelBuilder.Entity<staff>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<staff>()
                .Property(e => e.username)
                .IsUnicode(false);

            modelBuilder.Entity<staff>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<staff>()
                .HasMany(e => e.payments)
                .WithRequired(e => e.staff)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<staff>()
                .HasMany(e => e.rentals)
                .WithRequired(e => e.staff)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<staff>()
                .HasMany(e => e.stores)
                .WithRequired(e => e.staff)
                .HasForeignKey(e => e.manager_staff_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<store>()
                .HasMany(e => e.customers)
                .WithRequired(e => e.store)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<store>()
                .HasMany(e => e.inventories)
                .WithRequired(e => e.store)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<store>()
                .HasMany(e => e.staffs)
                .WithRequired(e => e.store)
                .HasForeignKey(e => e.store_id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<actor_info>()
                .Property(e => e.first_name)
                .IsUnicode(false);

            modelBuilder.Entity<actor_info>()
                .Property(e => e.last_name)
                .IsUnicode(false);

            modelBuilder.Entity<actor_info>()
                .Property(e => e.film_info)
                .IsUnicode(false);

            modelBuilder.Entity<customer_list>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<customer_list>()
                .Property(e => e.address)
                .IsUnicode(false);

            modelBuilder.Entity<customer_list>()
                .Property(e => e.zip_code)
                .IsUnicode(false);

            modelBuilder.Entity<customer_list>()
                .Property(e => e.phone)
                .IsUnicode(false);

            modelBuilder.Entity<customer_list>()
                .Property(e => e.city)
                .IsUnicode(false);

            modelBuilder.Entity<customer_list>()
                .Property(e => e.country)
                .IsUnicode(false);

            modelBuilder.Entity<customer_list>()
                .Property(e => e.notes)
                .IsUnicode(false);

            modelBuilder.Entity<film_list>()
                .Property(e => e.title)
                .IsUnicode(false);

            modelBuilder.Entity<film_list>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<film_list>()
                .Property(e => e.category)
                .IsUnicode(false);

            modelBuilder.Entity<film_list>()
                .Property(e => e.rating)
                .IsUnicode(false);

            modelBuilder.Entity<film_list>()
                .Property(e => e.actors)
                .IsUnicode(false);

            modelBuilder.Entity<nicer_but_slower_film_list>()
                .Property(e => e.title)
                .IsUnicode(false);

            modelBuilder.Entity<nicer_but_slower_film_list>()
                .Property(e => e.description)
                .IsUnicode(false);

            modelBuilder.Entity<nicer_but_slower_film_list>()
                .Property(e => e.category)
                .IsUnicode(false);

            modelBuilder.Entity<nicer_but_slower_film_list>()
                .Property(e => e.rating)
                .IsUnicode(false);

            modelBuilder.Entity<nicer_but_slower_film_list>()
                .Property(e => e.actors)
                .IsUnicode(false);

            modelBuilder.Entity<sales_by_film_category>()
                .Property(e => e.category)
                .IsUnicode(false);

            modelBuilder.Entity<sales_by_store>()
                .Property(e => e.store)
                .IsUnicode(false);

            modelBuilder.Entity<sales_by_store>()
                .Property(e => e.manager)
                .IsUnicode(false);

            modelBuilder.Entity<staff_list>()
                .Property(e => e.name)
                .IsUnicode(false);

            modelBuilder.Entity<staff_list>()
                .Property(e => e.address)
                .IsUnicode(false);

            modelBuilder.Entity<staff_list>()
                .Property(e => e.zip_code)
                .IsUnicode(false);

            modelBuilder.Entity<staff_list>()
                .Property(e => e.phone)
                .IsUnicode(false);

            modelBuilder.Entity<staff_list>()
                .Property(e => e.city)
                .IsUnicode(false);

            modelBuilder.Entity<staff_list>()
                .Property(e => e.country)
                .IsUnicode(false);
        }
    }
}
