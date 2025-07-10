namespace ConsoleTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TestGuidIdentity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OneChildTests", "TestGuid", c => c.Guid(nullable: false, identity: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OneChildTests", "TestGuid");
        }
    }
}
