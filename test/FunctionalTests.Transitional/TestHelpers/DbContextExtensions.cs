// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Migrations;

#if NET452
    using System.Data.SqlServerCe;
#endif

    public static class DbContextExtensions
    {
        public static TypeAssertion<TStructuralType> Assert<TStructuralType>(this DbContext context)
            where TStructuralType : class
        {
            return new TypeAssertion<TStructuralType>(context);
        }

        public static void IgnoreSpatialTypesOnSqlCe(this DbContext context, DbModelBuilder modelBuilder)
        {
            if (context.IsSqlCe())
            {
                modelBuilder.Entity<MigrationsStore>().Ignore(e => e.Location);
                modelBuilder.Entity<MigrationsStore>().Ignore(e => e.FloorPlan);
            }
        }

        public static bool IsSqlCe(this DbContext context)
        {
#if NET452
            return context.Database.Connection is SqlCeConnection;
#else
            return false;
#endif
        }
    }
}
