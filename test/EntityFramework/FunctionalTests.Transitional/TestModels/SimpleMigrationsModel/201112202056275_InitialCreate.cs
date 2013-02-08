// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.SimpleMigrationsModel
{
    using System.Data.Entity.Migrations;

    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Blogs",
                c => new
                         {
                             BlogId = c.Int(nullable: false, identity: true),
                             Name = c.String(),
                         })
                .PrimaryKey(t => t.BlogId);
        }

        public override void Down()
        {
            DropTable("Blogs");
        }
    }
}
