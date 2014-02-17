// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace AdvancedPatternsModel
{
    using System.Data.Entity;

    public class AdvancedPatternsMasterContext : DbContext
    {
        public AdvancedPatternsMasterContext()
            : base("AdvancedPatternsDatabase")
        {
        }

        protected AdvancedPatternsMasterContext(string suffix)
            : base("AdvancedPatternsDatabase_" + suffix)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            SetupModel(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        internal static void SetupModel(DbModelBuilder builder)
        {
            builder.Entity<Employee>();
            builder.Entity<Building>()
                .HasOptional(b => b.PrincipalMailRoom)
                .WithMany()
                .HasForeignKey(b => b.PrincipalMailRoomId);

            builder.ComplexType<Address>();
            builder.ComplexType<SiteInfo>();
            builder.Entity<MailRoom>()
                .HasRequired(m => m.Building)
                .WithMany(b => b.MailRooms)
                .HasForeignKey(m => m.BuildingId);

            builder.Entity<Office>().HasKey(
                o => new
                         {
                             o.Number,
                             o.BuildingId
                         });
            builder.Ignore<UnMappedOffice>();
            builder.Entity<BuildingDetail>()
                .HasKey(d => d.BuildingId)
                .HasRequired(d => d.Building).WithOptional();

            builder.Entity<Building>().Ignore(b => b.NotInModel);
            builder.ComplexType<Address>().Ignore(a => a.County);
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<BuildingDetail> BuildingDetails { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<Whiteboard> Whiteboards { get; set; }
    }

    public class AdvancedPatternsMasterContext_2012 : AdvancedPatternsMasterContext 
    { 
        public AdvancedPatternsMasterContext_2012()
            : base("2012")
        {
        }
    }
}
