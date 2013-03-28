// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Data.SqlServerCe;

    public class MigrationsCustomerBase
    {
        public int Id { get; private set; }
    }

    public class MigrationsCustomer : MigrationsCustomerBase
    {
        public MigrationsCustomer()
        {
            DateOfBirth = DateTime.Now;
            HomeAddress = new MigrationsAddress();
            WorkAddress = new MigrationsAddress();
        }

        public long CustomerNumber { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public byte Age { get; set; }
        public MigrationsAddress HomeAddress { get; set; }
        public MigrationsAddress WorkAddress { get; set; }
        public ICollection<Order> Orders { get; set; }

        [MaxLength(1024)]
        public byte[] Photo { get; set; }
    }

    public class SpecialCustomer : MigrationsCustomer
    {
        public int Points { get; set; }
    }

    public class GoldCustomer : MigrationsCustomer
    {
        [Column("Gold_Points")]
        public int Points { get; set; }
    }

    [ComplexType]
    public class MigrationsAddress
    {
        [MaxLength(150)]
        public string City { get; set; }
    }

    [Table("Orders", Schema = "ordering")]
    public class Order
    {
        public int OrderId { get; set; }
        public string Type { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        public ICollection<OrderLine> OrderLines { get; set; }
    }

    public class OrderLine
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public short Quantity { get; set; }
        public decimal Price { get; set; }

        [Column(TypeName = "money")]
        public decimal Total { get; set; }

        public bool? IsShipped { get; set; }
        public int ProductId { get; set; }

        [MaxLength(128)]
        public string Sku { get; set; }
    }

    public class MigrationsProduct
    {
        [Key]
        [Column(Order = 0)]
        public int ProductId { get; set; }

        [Key]
        [Column(Order = 1)]
        [MaxLength(128)]
        public string Sku { get; set; }

        public string Name { get; set; }

        public byte[] Image { get; set; }
    }

    public class MigrationsStore
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public MigrationsAddress Address { get; set; }
        public DbGeography Location { get; set; }
        public DbGeometry FloorPlan { get; set; }
        public StoreKind Kind { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public class WithGuidKey
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Foo { get; set; }
    }

    public enum StoreKind
    {
        Mall,
        HighStreet,
        Embedded,
    }

    public class ShopContext_v1 : DbContext
    {
        public ShopContext_v1()
        {
        }

        public ShopContext_v1(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<MigrationsCustomer> Customers { get; set; }
        public DbSet<MigrationsProduct> Products { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderLine>().Ignore(c => c.Total);
            modelBuilder.Entity<Order>().Property(o => o.Type).IsUnicode(false);
            modelBuilder.Entity<MigrationsProduct>();
            
            if (!(Database.Connection is SqlCeConnection))
            {
                // NotSupported in CE
                modelBuilder.Entity<MigrationsCustomer>().MapToStoredProcedures();
            }
        }
    }

    public class ShopContext_v2 : ShopContext_v1
    {
        public ShopContext_v2()
        {
        }

        public ShopContext_v2(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MigrationsCustomer>().ToTable("tbl_customers", "crm");

            if (!(Database.Connection is SqlCeConnection))
            {
                // NotSupported in CE
                modelBuilder.Entity<MigrationsCustomer>().Property(c => c.Name).HasColumnName("new_name");
            }

            modelBuilder.Entity<MigrationsCustomer>().Property(c => c.FullName).HasMaxLength(25);

            modelBuilder.Entity<Order>().Property(o => o.Type).IsUnicode(false);
            modelBuilder.Entity<Order>().HasMany(o => o.OrderLines).WithOptional().WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderLine>().Ignore(ol => ol.IsShipped);
            modelBuilder.Entity<OrderLine>().HasKey(
                ol => new
                          {
                              ol.Id,
                              ol.OrderId
                          });
            modelBuilder.Entity<MigrationsProduct>();
        }
    }

    public class ShopContext_v3 : ShopContext_v2
    {
        public ShopContext_v3()
        {
        }

        public ShopContext_v3(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<MigrationsStore> Stores { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            this.IgnoreSpatialTypesOnSqlCe(modelBuilder);
        }
    }

    public class ShopContext_v4 : ShopContext_v3
    {
        public ShopContext_v4()
        {
        }

        public ShopContext_v4(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<MigrationsStore>()
                .Property(s => s.Name)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }

    public class ShopContext_v5 : ShopContext_v1
    {
        public ShopContext_v5()
        {
        }

        public ShopContext_v5(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("non_default_schema");
        }
    }

    public class EmptyModel : DbContext
    {
    }

    public class NonEmptyModel : DbContext
    {
        public DbSet<MigrationsBlog> Blogs { get; set; }
    }

    public class MigrationsBlog
    {
        public int MigrationsBlogId { get; set; }
        public string Url { get; set; }
    }

    internal class MappingScenariosContext : DbContext
    {
        public DbSet<MigrationsEmployee> Employees { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MigrationsEmployee>()
                        .HasOptional(e => e.Manager)
                        .WithMany(m => m.DirectReports);
        }
    }

    internal class MigrationsEmployee
    {
        public int Id { get; set; }

        public int? ManagerId { get; set; }
        public MigrationsEmployee Manager { get; set; }

        public ICollection<MigrationsEmployee> DirectReports { get; set; }
    }

    public class TypeCasts
    {
        public int Id { get; set; }

        public byte ByteToInt16 { get; set; }
        public byte ByteToInt32 { get; set; }
        public byte ByteToInt64 { get; set; }
        public short Int16ToInt32 { get; set; }
        public short Int16ToInt64 { get; set; }
        public int Int32ToInt64 { get; set; }
        public decimal Decimal6ToSingle { get; set; }
        public decimal Decimal6ToDouble { get; set; }
        public decimal Decimal7ToSingle { get; set; }
        public float SingleToDecimal11 { get; set; }
        public float SingleToDouble { get; set; }
        public float SingleToDecimal16 { get; set; }
        public decimal Decimal15ToDouble { get; set; }
        public double DoubleToDecimal16 { get; set; }

        #region Version 2

        [Column("ByteToInt16")]
        public short ByteToInt16_v2 { get; set; }

        [Column("ByteToInt32")]
        public int ByteToInt32_v2 { get; set; }

        [Column("ByteToInt64")]
        public long ByteToInt64_v2 { get; set; }

        [Column("Int16ToInt32")]
        public int Int16ToInt32_v2 { get; set; }

        [Column("Int16ToInt64")]
        public long Int16ToInt64_v2 { get; set; }

        [Column("Int32ToInt64")]
        public long Int32ToInt64_v2 { get; set; }

        [Column("Decimal6ToSingle")]
        public float Decimal6ToSingle_v2 { get; set; }

        [Column("Decimal6ToDouble")]
        public double Decimal6ToDouble_v2 { get; set; }

        [Column("Decimal7ToSingle")]
        public float Decimal7ToSingle_v2 { get; set; }

        [Column("SingleToDecimal11")]
        public decimal SingleToDecimal11_v2 { get; set; }

        [Column("SingleToDouble")]
        public double SingleToDouble_v2 { get; set; }

        [Column("SingleToDecimal16")]
        public decimal SingleToDecimal16_v2 { get; set; }

        [Column("Decimal15ToDouble")]
        public double Decimal15ToDouble_v2 { get; set; }

        [Column("DoubleToDecimal16")]
        public decimal DoubleToDecimal16_v2 { get; set; }

        #endregion
    }

    public class Comment
    {
        public int Id { get; set; }
        public int? MigrationsBlogId { get; set; }
        public MigrationsBlog Blog { get; set; }
    }

    namespace UserRoles_v1
    {
        public class Role
        {
            public long Id { get; set; }
            public virtual ICollection<User> AssignedUsers { get; set; }
        }

        public class User
        {
            public long Id { get; set; }
            public virtual ICollection<Role> AssignedRoles { get; set; }
        }
    }

    namespace UserRoles_v2
    {
        public class Role
        {
            public long Id { get; set; }
            public virtual ICollection<User2> AssignedUsers { get; set; }
        }

        public class User2
        {
            public long Id { get; set; }
            public virtual ICollection<Role> AssignedRoles { get; set; }
        }
    }

    public class ProcessedTransactionContext : DbContext
    {
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<ProcessedTransaction>()
                .HasMany(e => e.ChildTransactions)
                .WithOptional()
                .HasForeignKey(e => e.ParentTransactionId);
        }
    }

    public class ProcessedTransaction
    {
        public int Id { get; set; }

        public int? ParentTransactionId { get; set; }

        public virtual ProcessedTransaction ParentTransaction { get; set; }

        public virtual ICollection<ProcessedTransaction> ChildTransactions { get; set; }
    }
}
