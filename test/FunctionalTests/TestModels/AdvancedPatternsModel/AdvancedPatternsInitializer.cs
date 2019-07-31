// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace AdvancedPatternsModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;

    public class AdvancedPatternsInitializer : DropCreateDatabaseIfModelChanges<AdvancedPatternsMasterContext>
    {
        protected override void Seed(AdvancedPatternsMasterContext context)
        {
            var buildings = new List<Building>
                                {
                                    new Building
                                        {
                                            BuildingId = new Guid("21EC2020-3AEA-1069-A2DD-08002B30309D"),
                                            Name = "Building One",
                                            Value = 1500000m,
                                            Address = new Address
                                                          {
                                                              Street = "100 Work St",
                                                              City = "Redmond",
                                                              State = "WA",
                                                              ZipCode = "98052",
                                                              SiteInfo = new SiteInfo
                                                                             {
                                                                                 Zone = 1,
                                                                                 Environment = "Clean"
                                                                             }
                                                          },
                                        },
                                    new Building
                                        {
                                            BuildingId = Guid.NewGuid(),
                                            Name = "Building Two",
                                            Value = 1000000m,
                                            Address = new Address
                                                          {
                                                              Street = "200 Work St",
                                                              City = "Redmond",
                                                              State = "WA",
                                                              ZipCode = "98052",
                                                              SiteInfo = new SiteInfo
                                                                             {
                                                                                 Zone = 2,
                                                                                 Environment = "Contaminated"
                                                                             }
                                                          },
                                        },
                                };

            foreach (var building in buildings)
            {
                context.Buildings.Add(building);
            }
            
            var offices = new List<Office>
                              {
                                  new Office
                                      {
                                          BuildingId = buildings[0].BuildingId,
                                          Number = "1/1221"
                                      },
                                  new Office
                                      {
                                          BuildingId = buildings[0].BuildingId,
                                          Number = "1/1223"
                                      },
                                  new Office
                                      {
                                          BuildingId = buildings[0].BuildingId,
                                          Number = "2/1458"
                                      },
                                  new Office
                                      {
                                          BuildingId = buildings[0].BuildingId,
                                          Number = "2/1789"
                                      },
                              };

            foreach (var office in offices)
            {
                context.Offices.Add(office);
            }

            var employees = new List<Employee>
                {
                    new CurrentEmployee
                        {
                            EmployeeId = 1,
                            FirstName = "Rowan",
                            LastName = "Miller",
                            LeaveBalance = 45,
                            Office = offices[0]
                        },
                    new CurrentEmployee
                        {
                            EmployeeId = 2,
                            FirstName = "Arthur",
                            LastName = "Vickers",
                            LeaveBalance = 62,
                            Office = offices[1]
                        },
                    new PastEmployee
                        {
                            EmployeeId = 3,
                            FirstName = "John",
                            LastName = "Doe",
                            TerminationDate = new DateTime(2006, 1, 23)
                        },
                };

            foreach (var employee in employees)
            {
                context.Employees.Add(employee);
            }

            var whiteboards = new List<Whiteboard>
                {
                    new Whiteboard
                        {
                            AssetTag = "WB1973",
                            iD = new byte[] { 1, 9, 7, 3 },
                            Office = offices[0]
                        },
                    new Whiteboard
                        {
                            AssetTag = "WB1977",
                            iD = new byte[] { 1, 9, 7, 7 },
                            Office = offices[0]
                        },
                    new Whiteboard
                        {
                            AssetTag = "WB1970",
                            iD = new byte[] { 1, 9, 7, 0 },
                            Office = offices[2]
                        },
                };

            foreach (var whiteboard in whiteboards)
            {
                context.Whiteboards.Add(whiteboard);
            }
        }
    }
}
