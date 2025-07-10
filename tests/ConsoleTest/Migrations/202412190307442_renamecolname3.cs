namespace ConsoleTest.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renamecolname3 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "SYSDBA.Children", name: "Name_TestRename", newName: "Name");
        }
        
        public override void Down()
        {
            RenameColumn(table: "SYSDBA.Children", name: "Name", newName: "Name_TestRename");
        }
    }
}
