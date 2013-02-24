' Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
Imports System.Data.Entity

Namespace AdvancedPatternsVB

    Friend Class AdvancedPatternsModelFirstInitializer
        Inherits DropCreateDatabaseAlways(Of AdvancedPatternsModelFirstContext)
        Private Shared ReadOnly _knownBuildingGuid As New Guid("21EC2020-3AEA-1069-A2DD-08002B30309D")

        Public Shared ReadOnly Property KnownBuildingGuid() As Guid
            Get
                Return _knownBuildingGuid
            End Get
        End Property

        Protected Overrides Sub Seed(context As AdvancedPatternsModelFirstContext)
            context.Database.ExecuteSqlCommand("CREATE PROCEDURE dbo.AllOffices AS SET NOCOUNT ON; SELECT Number, BuildingId, Description FROM dbo.Offices")
            context.Database.ExecuteSqlCommand("CREATE PROCEDURE dbo.OfficesInBuilding @BuildingId uniqueidentifier AS SET NOCOUNT ON; SELECT Number, BuildingId, Description FROM dbo.Offices WHERE BuildingId = @BuildingId")
            context.Database.ExecuteSqlCommand("CREATE PROCEDURE dbo.SkimOffLeaveBalance @First nvarchar(4000), @Last nvarchar(4000) AS SET NOCOUNT ON; UPDATE dbo.Employees SET LeaveBalance = 0 WHERE FirstName = @First And LastName = @Last")
            context.Database.ExecuteSqlCommand("CREATE PROCEDURE dbo.EmployeeIdsInOffice @OfficeNumber nvarchar(128), @BuildingId uniqueidentifier AS SET NOCOUNT ON; SELECT EmployeeId FROM dbo.Employees WHERE OfficeBuildingId = @BuildingId And OfficeNumber = @OfficeNumber")
            context.Database.ExecuteSqlCommand("CREATE PROCEDURE dbo.AllSiteInfo AS SET NOCOUNT ON; SELECT Zone, Environment FROM dbo.Buildings")

            Dim buildings = New List(Of BuildingMf)()
            buildings.Add(New BuildingMf(_knownBuildingGuid, "Building One", 1500000D, New AddressMf("100 Work St", "Redmond", "WA", "98052", 1, "Clean")))
            buildings.Add(New BuildingMf(Guid.NewGuid(), "Building Two", 1000000D, New AddressMf("200 Work St", "Redmond", "WA", "98052", 2, "Contaminated")))
            buildings.ForEach(Function(b) context.Buildings.Add(b))

            Dim offices = New List(Of OfficeMf)()
            offices.Add(New OfficeMf() With {.BuildingId = buildings(0).BuildingId, .Number = "1/1221"})
            offices.Add(New OfficeMf() With {.BuildingId = buildings(0).BuildingId, .Number = "1/1223"})
            offices.Add(New OfficeMf() With {.BuildingId = buildings(1).BuildingId, .Number = "2/1458"})
            offices.Add(New OfficeMf() With {.BuildingId = buildings(1).BuildingId, .Number = "2/1789"})

            offices.ForEach(Function(o) context.Offices.Add(o))

            Dim employees = New List(Of EmployeeMf)()

            employees.Add(New CurrentEmployeeMf("Rowan", "Miller") With { _
                    .EmployeeId = 1, _
                    .LeaveBalance = 45, _
                    .Office = offices(0) _
                })

            employees.Add(New CurrentEmployeeMf("Arthur", "Vickers") With { _
                    .EmployeeId = 2, _
                    .LeaveBalance = 62, _
                    .Office = offices(1) _
                })

            employees.Add(New PastEmployeeMf("John", "Doe") With { _
                    .EmployeeId = 3, _
                    .TerminationDate = New DateTime(2006, 1, 23) _
                })

            employees.ForEach(Function(e) context.Employees.Add(e))

            Dim boards = New List(Of WhiteboardMf)()

            boards.Add(New WhiteboardMf() With { _
                    .AssetTag = "WB1973", _
                    .iD = New Byte() {1, 9, 7, 3}, _
                    .Office = offices(0) _
                })

            boards.Add(New WhiteboardMf() With { _
                    .AssetTag = "WB1977", _
                    .iD = New Byte() {1, 9, 7, 7}, _
                    .Office = offices(0) _
                })

            boards.Add(New WhiteboardMf() With { _
                    .AssetTag = "WB1970", _
                    .iD = New Byte() {1, 9, 7, 0}, _
                    .Office = offices(2) _
                })

            boards.ForEach(Function(w) context.Whiteboards.Add(w))
        End Sub
    End Class

End Namespace
