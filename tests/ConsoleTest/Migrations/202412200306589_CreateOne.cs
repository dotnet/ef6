namespace ConsoleTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CreateOne : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "SYSDBA.OneChildren",
                c => new
                    {
                        OneChildId = c.String(nullable: false, maxLength: 128, unicode: false),
                        Name = c.String(unicode: false),
                        Label = c.Guid(nullable: false),
                        BirthTime = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.OneChildId);
            
            CreateTable(
                "dbo.OneChildTests",
                c => new
                    {
                        OneChildTestId = c.String(nullable: false, maxLength: 128, unicode: false),
                        Name = c.String(unicode: false),
                        Label = c.Guid(nullable: false),
                        teststrchangename = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.OneChildTestId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OneChildTests");
            DropTable("SYSDBA.OneChildren");
        }
    }
}
