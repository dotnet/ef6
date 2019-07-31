// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.SimpleMigrationsModel
{
    using System.Data.Entity;

    public class MigrateInitializerContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
    }
}
