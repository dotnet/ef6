// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.GearsOfWarModel
{
    using System.Data.Entity.TestModels.ArubaModel;

    public class GearsOfWarStoredProceduresContext : GearsOfWarContext
    {
        static GearsOfWarStoredProceduresContext()
        {
            Database.SetInitializer(new GearsOfWarStoredProceduresInitializer());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<City>().MapToStoredProcedures();
            modelBuilder.Entity<CogTag>().MapToStoredProcedures(
                m => m.Insert(i => i.HasName("Cog_Insert")));

            modelBuilder.Entity<CogTag>().MapToStoredProcedures(
                m => m.Update(i => i.HasName("Cog_Update")));

            modelBuilder.Entity<CogTag>().MapToStoredProcedures(
                m => m.Delete(i => i.HasName("Cog_Delete")));

            // issue 1351
            //modelBuilder.Entity<Gear>().MapToStoredProcedures();

            modelBuilder.Entity<Squad>().MapToStoredProcedures(
                m => m.Insert(i => i.Parameter(p => p.Name, "Squad_Name")));

            // issue 1155
            //modelBuilder.Entity<Weapon>().MapToStoredProcedures();

            // issue 1155
            //modelBuilder.Entity<Gear>().HasMany(g => g.Weapons).WithMany().MapToStoredProcedures();
        }
    }
}
