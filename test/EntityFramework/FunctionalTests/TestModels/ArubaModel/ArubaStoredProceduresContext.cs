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
        }
    }
}
