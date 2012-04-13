namespace FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;

    internal class AdvancedPatternsModelFirstInitializer : DropCreateDatabaseAlways<AdvancedPatternsModelFirstContext>
    {
        public static readonly Guid KnownBuildingGuid = new Guid("21EC2020-3AEA-1069-A2DD-08002B30309D");

        protected override void Seed(AdvancedPatternsModelFirstContext context)
        {
            context.Database.ExecuteSqlCommand(@"CREATE PROCEDURE dbo.AllOffices
                                                 AS
                                                 SET NOCOUNT ON;
                                                 SELECT Number, BuildingId, Description
                                                 FROM dbo.Offices");

            context.Database.ExecuteSqlCommand(@"CREATE PROCEDURE dbo.OfficesInBuilding
                                                 @BuildingId uniqueidentifier
                                                 AS 
                                                 SET NOCOUNT ON;
                                                 SELECT Number, BuildingId, Description
                                                 FROM dbo.Offices
                                                 WHERE BuildingId = @BuildingId");

            context.Database.ExecuteSqlCommand(@"CREATE PROCEDURE dbo.SkimOffLeaveBalance
                                                 @First nvarchar(4000),
                                                 @Last nvarchar(4000)
                                                 AS 
                                                 SET NOCOUNT ON;
                                                 UPDATE dbo.Employees
                                                 SET LeaveBalance = 0 
                                                 WHERE FirstName = @First And LastName = @Last");

            context.Database.ExecuteSqlCommand(@"CREATE PROCEDURE dbo.EmployeeIdsInOffice
                                                 @OfficeNumber nvarchar(128),
                                                 @BuildingId uniqueidentifier
                                                 AS 
                                                 SET NOCOUNT ON;
                                                 SELECT EmployeeId
                                                 FROM dbo.Employees
                                                 WHERE OfficeBuildingId = @BuildingId And OfficeNumber = @OfficeNumber");

            context.Database.ExecuteSqlCommand(@"CREATE PROCEDURE dbo.AllSiteInfo
                                                 AS 
                                                 SET NOCOUNT ON;
                                                 SELECT Zone, Environment
                                                 FROM dbo.Buildings");

            var buildings = new List<BuildingMf>
            {
                new BuildingMf(KnownBuildingGuid, "Building One", 1500000m,
                               new AddressMf("100 Work St", "Redmond", "WA", "98052", 1, "Clean")),
                new BuildingMf(Guid.NewGuid(), "Building Two", 1000000m,
                               new AddressMf("200 Work St", "Redmond", "WA", "98052", 2, "Contaminated")),
            };
            buildings.ForEach(b => context.Buildings.Add(b));

            var offices = new List<OfficeMf>
            {
                new OfficeMf { BuildingId = buildings[0].BuildingId, Number = "1/1221" },
                new OfficeMf { BuildingId = buildings[0].BuildingId, Number = "1/1223" },
                new OfficeMf { BuildingId = buildings[1].BuildingId, Number = "2/1458" },
                new OfficeMf { BuildingId = buildings[1].BuildingId, Number = "2/1789" },
            };
            offices.ForEach(o => context.Offices.Add(o));

            new List<EmployeeMf>
            {
                new CurrentEmployeeMf("Rowan", "Miller") { EmployeeId = 1, LeaveBalance = 45, Office = offices[0] },
                new CurrentEmployeeMf("Arthur", "Vickers") { EmployeeId = 2, LeaveBalance = 62, Office = offices[1] },
                new PastEmployeeMf("John", "Doe") { EmployeeId = 3, TerminationDate = new DateTime(2006, 1, 23) },
            }.ForEach(e => context.Employees.Add(e));

            new List<WhiteboardMf>
            {
                new WhiteboardMf { AssetTag = "WB1973", iD = new byte[] { 1, 9, 7, 3 }, Office = offices[0] },
                new WhiteboardMf { AssetTag = "WB1977", iD = new byte[] { 1, 9, 7, 7 }, Office = offices[0] },
                new WhiteboardMf { AssetTag = "WB1970", iD = new byte[] { 1, 9, 7, 0 }, Office = offices[2] },
            }.ForEach(w => context.Whiteboards.Add(w));
        }
    }
}