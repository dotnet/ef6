namespace ConsoleTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddForeignKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ChildTests", "teststr", c => c.String(unicode: false));
            AddColumn("dbo.ChildTests", "ChildId", c => c.String(maxLength: 128, unicode: false));
            CreateIndex("dbo.ChildTests", "ChildId");
            AddForeignKey("dbo.ChildTests", "ChildId", "SYSDBA.Children", "ChildId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChildTests", "ChildId", "SYSDBA.Children");
            DropIndex("dbo.ChildTests", new[] { "ChildId" });
            DropColumn("dbo.ChildTests", "ChildId");
            DropColumn("dbo.ChildTests", "teststr");
        }
    }
}
