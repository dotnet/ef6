namespace ConsoleTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renamecolname : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChildTests", "teststrchangename", c => c.String(unicode: false));
            DropColumn("dbo.ChildTests", "teststr");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ChildTests", "teststr", c => c.String(unicode: false));
            DropColumn("dbo.ChildTests", "teststrchangename");
        }
    }
}
