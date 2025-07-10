namespace ConsoleTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OneToOne : DbMigration
    {
        public override void Up()
        {
            AddColumn("SYSDBA.OneChildren", "ChildTestId", c => c.String(unicode: false));
            AddColumn("dbo.OneChildTests", "OneChildId", c => c.String(unicode: false));
            CreateIndex("SYSDBA.OneChildren", "OneChildId");
            AddForeignKey("SYSDBA.OneChildren", "OneChildId", "dbo.OneChildTests", "OneChildTestId");
        }
        
        public override void Down()
        {
            DropForeignKey("SYSDBA.OneChildren", "OneChildId", "dbo.OneChildTests");
            DropIndex("SYSDBA.OneChildren", new[] { "OneChildId" });
            DropColumn("dbo.OneChildTests", "OneChildId");
            DropColumn("SYSDBA.OneChildren", "ChildTestId");
        }
    }
}
