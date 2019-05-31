// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    public class ArubaStoredProceduresContext : ArubaContext
    {
        static ArubaStoredProceduresContext()
        {
            Database.SetInitializer(new ArubaStoredProceduresInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ArubaAllTypes>().MapToStoredProcedures();
            modelBuilder.Entity<ArubaBug>().MapToStoredProcedures();
            modelBuilder.Entity<ArubaConfig>().MapToStoredProcedures();
            modelBuilder.Entity<ArubaFailure>().MapToStoredProcedures();
            modelBuilder.Entity<ArubaOwner>().MapToStoredProcedures();
            modelBuilder.Entity<ArubaPerson>().MapToStoredProcedures();
            modelBuilder.Entity<ArubaRun>().MapToStoredProcedures();
            modelBuilder.Entity<ArubaTask>().MapToStoredProcedures();

            modelBuilder
                .Entity<ArubaPerson>().HasMany(p => p.Children).WithMany(p => p.Parents)
                .MapToStoredProcedures();

            modelBuilder.Entity<ArubaPerson>().HasMany(p => p.Children).WithMany(p => p.Parents)
                .MapToStoredProcedures(
                    m => m.Insert(d => d.LeftKeyParameter(l => l.Id, "Left_id").RightKeyParameter(r => r.Id, "Right_Id")));

#if NETCOREAPP3_0
            modelBuilder.Entity<ArubaAllTypes>().Ignore(x => x.c31_geography);
            modelBuilder.Entity<ArubaAllTypes>().Ignore(x => x.c32_geometry);
            modelBuilder.Entity<ArubaAllTypes>().Ignore(x => x.c36_geometry_linestring);
            modelBuilder.Entity<ArubaAllTypes>().Ignore(x => x.c37_geometry_polygon);

            modelBuilder.Entity<ArubaMachineConfig>().Ignore(x => x.Location);

            modelBuilder.Entity<ArubaRun>().Ignore(x => x.Geometry);
#endif
        }
    }
}
