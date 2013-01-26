// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.SimpleMigrationsModel
{
    using System.Data.Entity.Migrations;

    public partial class AddUrlToBlog : DbMigration
    {
        public override void Up()
        {
            AddColumn("Blogs", "Url", c => c.String());
        }

        public override void Down()
        {
            DropColumn("Blogs", "Url");
        }
    }
}
