namespace ConsoleTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "SYSDBA.Children",
                c => new
                    {
                        ChildId = c.String(nullable: false, maxLength: 128, unicode: false),
                        Name = c.String(unicode: false),
                        Label = c.Guid(nullable: false),
                        BirthTime = c.DateTime(nullable: false, precision: 0),
                    })
                .PrimaryKey(t => t.ChildId);
            
            CreateTable(
                "dbo.ChildTests",
                c => new
                    {
                        ChildTestId = c.String(nullable: false, maxLength: 128, unicode: false),
                        Name = c.String(unicode: false),
                        Label = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.ChildTestId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ChildTests");
            DropTable("SYSDBA.Children");
        }
    }
}
