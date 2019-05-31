// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if NET452

namespace System.Data.Entity.Query.StoredProcedures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Spatial;
    using System.Data.SqlClient;
    using System.Linq;
    using Xunit;

    public class StoredProceduresTests : FunctionalTestBase, IClassFixture<StoredProceduresTestFixture>
    {
        private readonly string _entityConnectionString;
        private const int GeographySrid = 4326;
        private const int GeometrySrid = 32768;

        public StoredProceduresTests(StoredProceduresTestFixture data)
        {
            var esb = new EntityConnectionStringBuilder
                {
                    Metadata =
                        @"res://EntityFramework.FunctionalTests/System.Data.Entity.Query.StoredProcedures.IceAndFireModel.csdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Query.StoredProcedures.IceAndFireModel.ssdl|res://EntityFramework.FunctionalTests/System.Data.Entity.Query.StoredProcedures.IceAndFireModel.msl",
                    Provider = @"System.Data.SqlClient",
                    ProviderConnectionString = ModelHelpers.SimpleConnectionString("IceAndFireContext2"),
                };

            _entityConnectionString = esb.ToString();
            Seed();
        }

        [Fact]
        public void Stored_procedure_with_first_result_set_without_spatial_and_second_with_spatial()
        {
            using (var context = new IceAndFireModel.IceAndFireContext(_entityConnectionString))
            {
                var animals = context.GetAnimalsAndHouses();
                Assert.Equal(3, animals.Count());

                var houses = animals.GetNextResult<IceAndFireModel.House>();

                Assert.Equal(4, houses.Count());
            }
        }

        [Fact]
        public void Stored_procedure_with_first_result_set_with_spatial_and_second_without_spatial()
        {
            using (var context = new IceAndFireModel.IceAndFireContext(_entityConnectionString))
            {
                var houses = context.GetHousesAndAnimals();
                Assert.Equal(4, houses.Count());

                var animals = houses.GetNextResult<IceAndFireModel.Animal>();
                Assert.Equal(3, animals.Count());
            }
        }

        [Fact]
        public void Stored_procedure_with_two_results_being_same_entity_with_spatial()
        {
            using (var context = new IceAndFireModel.IceAndFireContext(_entityConnectionString))
            {
                var houses = context.GetHousesAndHouses();
                Assert.Equal(4, houses.Count());

                var alsoHouses = houses.GetNextResult<IceAndFireModel.House>();
                Assert.Equal(4, alsoHouses.Count());
            }
        }

        [Fact]
        public void Stored_procedure_with_two_results_being_two_child_entities_of_the_same_hierarchy()
        {
            using (var context = new IceAndFireModel.IceAndFireContext(_entityConnectionString))
            {
                var humans = context.GetHumansAndAnimals();
                Assert.Equal(11, humans.Count());

                var animals = humans.GetNextResult<IceAndFireModel.Animal>();
                Assert.Equal(3, animals.Count());
            }
        }

        [Fact]
        public void Stored_procedure_with_two_results_second_being_hierarchy()
        {
            using (var context = new IceAndFireModel.IceAndFireContext(_entityConnectionString))
            {
                var lands = context.GetLandsAndCreatures();
                Assert.Equal(4, lands.Count());

                var creatures = lands.GetNextResult<IceAndFireModel.Creature>();
                var creaturesList = creatures.ToList();
                Assert.Equal(3, creaturesList.OfType<IceAndFireModel.Animal>().Count());
                Assert.Equal(11, creaturesList.OfType<IceAndFireModel.Human>().Count());
            }
        }

        private void Seed()
        {
            using (var context = new IceAndFireModel.IceAndFireContext(_entityConnectionString))
            {
                if (context.Creatures.Count() > 0)
                {
                    return;
                }

                var aryaStark = new IceAndFireModel.Human
                    {
                        Name = "Arya",
                        PlaceOfBirth = DbGeography.FromText("POINT (1 1)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Small,
                    };

                var sansaStark = new IceAndFireModel.Human
                    {
                        Name = "Sansa",
                        PlaceOfBirth = DbGeography.FromText("POINT (1 1)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Small,
                    };

                var branStark = new IceAndFireModel.Human
                    {
                        Name = "Brandon",
                        PlaceOfBirth = DbGeography.FromText("POINT (1 1)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Small,
                    };

                var ricksonStark = new IceAndFireModel.Human
                    {
                        Name = "Rickson",
                        PlaceOfBirth = DbGeography.FromText("POINT (1 1)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Small,
                    };

                var stannisBaratheon = new IceAndFireModel.Human
                    {
                        Name = "Stannis",
                        PlaceOfBirth = DbGeography.FromText("POINT (2 2)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Medium,
                    };

                var tyrionLannister = new IceAndFireModel.Human
                    {
                        Name = "Tyrion",
                        PlaceOfBirth = DbGeography.FromText("POINT (3 3)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Small,
                    };

                var jamieLannister = new IceAndFireModel.Human
                    {
                        Name = "Jamie",
                        PlaceOfBirth = DbGeography.FromText("POINT (3 3)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Medium,
                    };

                var cerseiLannister = new IceAndFireModel.Human
                    {
                        Name = "Cersei",
                        PlaceOfBirth = DbGeography.FromText("POINT (3 3)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Medium,
                    };

                var jonSnow = new IceAndFireModel.Human
                    {
                        Name = "Jon",
                        PlaceOfBirth = DbGeography.FromText("POINT (4 4)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Small,
                    };

                var daenerysTargaryen = new IceAndFireModel.Human
                    {
                        Name = "Daenerys",
                        PlaceOfBirth = DbGeography.FromText("POINT (5 5)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Medium,
                    };

                var aegonTargaryen = new IceAndFireModel.Human
                    {
                        Name = "Aegon",
                        PlaceOfBirth = DbGeography.FromText("POINT (5 5)", GeographySrid),
                        Size = IceAndFireModel.CreatureSize.Medium,
                    };

                var aurochs = new IceAndFireModel.Animal
                    {
                        IsCarnivore = false,
                        IsDangerous = false,
                        Name = "Aurochs",
                        Size = IceAndFireModel.CreatureSize.Large,
                    };

                var direwolf = new IceAndFireModel.Animal
                    {
                        IsCarnivore = true,
                        IsDangerous = true,
                        Name = "Direwolf",
                        Size = IceAndFireModel.CreatureSize.Large,
                    };

                var kraken = new IceAndFireModel.Animal
                    {
                        IsCarnivore = true,
                        IsDangerous = true,
                        Name = "Kraken",
                        Size = IceAndFireModel.CreatureSize.VeryLarge,
                    };

                var houseStark = new IceAndFireModel.House
                    {
                        Name = "Stark",
                        Sigil = DbGeometry.FromText("POINT (1 1)", GeometrySrid),
                        Words = "Winter is coming",
                        ProminentMembers = new List<IceAndFireModel.Human>
                            {
                                aryaStark,
                                sansaStark,
                                branStark,
                                ricksonStark
                            },
                    };

                var houseBaratheon = new IceAndFireModel.House
                    {
                        Name = "Baratheon",
                        Sigil = DbGeometry.FromText("POINT (2 2)", GeometrySrid),
                        Words = "Ours is the fury",
                        ProminentMembers = new List<IceAndFireModel.Human>
                            {
                                stannisBaratheon,
                            },
                    };

                var houseLannister = new IceAndFireModel.House
                    {
                        Name = "Lannister",
                        Sigil = DbGeometry.FromText("POINT (3 3)", GeometrySrid),
                        Words = "Hear me roar!",
                        ProminentMembers = new List<IceAndFireModel.Human>
                            {
                                tyrionLannister,
                                jamieLannister,
                                cerseiLannister,
                            },
                    };

                var houseTargaryen = new IceAndFireModel.House
                    {
                        Name = "Targaryen",
                        Sigil = DbGeometry.FromText("POINT (4 4)", GeometrySrid),
                        Words = "Fire and blood",
                        ProminentMembers = new List<IceAndFireModel.Human>
                            {
                                jonSnow,
                                daenerysTargaryen,
                                aegonTargaryen,
                            },
                    };

                var north = new IceAndFireModel.Land
                    {
                        LocationOnMap = DbGeography.FromText("POINT (1 1)", GeographySrid),
                        Name = "North",
                        RulingHouse = houseStark,
                    };

                var stormlands = new IceAndFireModel.Land
                    {
                        LocationOnMap = DbGeography.FromText("POINT (2 2)", GeographySrid),
                        Name = "Stormlands",
                        RulingHouse = houseBaratheon,
                    };

                var westerlands = new IceAndFireModel.Land
                    {
                        LocationOnMap = DbGeography.FromText("POINT (3 3)", GeographySrid),
                        Name = "Westerlands",
                        RulingHouse = houseLannister,
                    };

                var dragonstone = new IceAndFireModel.Land
                    {
                        LocationOnMap = DbGeography.FromText("POINT (4 4)", GeographySrid),
                        Name = "Dragonstone",
                        RulingHouse = houseTargaryen,
                    };

                context.Lands.Add(north);
                context.Lands.Add(stormlands);
                context.Lands.Add(westerlands);
                context.Lands.Add(dragonstone);

                context.Creatures.Add(aurochs);
                context.Creatures.Add(direwolf);
                context.Creatures.Add(kraken);

                context.SaveChanges();
            }
        }
    }

    public class StoredProceduresTestFixture
    {
        public StoredProceduresTestFixture()
        {
            using (var masterConnection = new SqlConnection(ModelHelpers.SimpleConnectionString("master")))
            {
                masterConnection.Open();

                var databaseName = "IceAndFireContext2";
                var databaseExistsScript = string.Format(
                    "SELECT COUNT(*) FROM sys.databases where name = '{0}'", databaseName);

                var databaseExists = (int)new SqlCommand(databaseExistsScript, masterConnection).ExecuteScalar() == 1;
                if (databaseExists)
                {
                    var dropDatabaseScript = string.Format("drop database {0}", databaseName);
                    new SqlCommand(dropDatabaseScript, masterConnection).ExecuteNonQuery();
                }

                var createDatabaseScript = string.Format("create database {0}", databaseName);


//                var createDatabaseScript = string.Format(
//                    @"if exists(select * from sys.databases where name = '{0}')
//drop database {0}
//create database {0}", "IceAndFireContext2");
                
                
                
                new SqlCommand(createDatabaseScript, masterConnection).ExecuteNonQuery();
            }

            var storeConnectionString = ModelHelpers.SimpleConnectionString("IceAndFireContext2");
            using (var connection = new SqlConnection(storeConnectionString))
            {
                connection.Open();

                //string partitionSchemeStatement = SqlAzureTestHelpers.IsSqlAzure(storeConnectionString) ? "" : " ON [PRIMARY]";

                new SqlCommand(
                    @"CREATE TABLE [dbo].[Lands](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[LocationOnMap] [geography] NULL,
 CONSTRAINT [PK_dbo.Lands] PRIMARY KEY CLUSTERED ([Id] ASC))", connection).ExecuteNonQuery();

                new SqlCommand(
 @"CREATE TABLE [dbo].[Houses](
	[Id] [int] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Words] [nvarchar](max) NULL,
	[Sigil] [geometry] NULL,
 CONSTRAINT [PK_dbo.Houses] PRIMARY KEY CLUSTERED ([Id] ASC))", connection).ExecuteNonQuery();

                new SqlCommand(
@"CREATE TABLE [dbo].[Creatures](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Size] [int] NOT NULL,
	[PlaceOfBirth] [geography] NULL,
	[IsCarnivore] [bit] NULL,
	[IsDangerous] [bit] NULL,
	[Discriminator] [nvarchar](128) NOT NULL,
	[House_Id] [int] NULL,
 CONSTRAINT [PK_dbo.Creatures] PRIMARY KEY CLUSTERED ([Id] ASC))", connection).ExecuteNonQuery();

                new SqlCommand(
                    @"ALTER TABLE [dbo].[Creatures]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Creatures_dbo.Houses_House_Id] FOREIGN KEY([House_Id])
REFERENCES [dbo].[Houses] ([Id])", connection).ExecuteNonQuery();

                new SqlCommand(
                    @"ALTER TABLE [dbo].[Houses]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Houses_dbo.Lands_Id] FOREIGN KEY([Id])
REFERENCES [dbo].[Lands] ([Id])", connection).ExecuteNonQuery();

                new SqlCommand(
                    @"CREATE PROCEDURE [dbo].[GetAnimalsAndHouses] AS 
SELECT * FROM dbo.Creatures AS c WHERE c.Discriminator = 'Animal' 
SELECT * FROM dbo.Houses", connection).ExecuteNonQuery();

                new SqlCommand(
                    @"CREATE PROCEDURE [dbo].[GetHousesAndAnimals] AS 
SELECT * FROM dbo.Houses 
SELECT * FROM dbo.Creatures AS c WHERE c.Discriminator = 'Animal'", connection).ExecuteNonQuery();

                new SqlCommand(
                    @"CREATE PROCEDURE [dbo].[GetHousesAndHouses] AS 
SELECT * FROM dbo.Houses 
SELECT * FROM dbo.Houses", connection).ExecuteNonQuery();

                new SqlCommand(
                    @"CREATE PROCEDURE [dbo].[GetHumansAndAnimals] AS 
SELECT * FROM dbo.Creatures AS c1 WHERE c1.Discriminator = 'Human' 
SELECT * FROM dbo.Creatures AS c2 WHERE c2.Discriminator = 'Animal'", connection).ExecuteNonQuery();

                new SqlCommand(
                    @"CREATE PROCEDURE [dbo].[GetLandsAndCreatures] AS 
SELECT * FROM dbo.Lands  
SELECT * FROM dbo.Creatures", connection).ExecuteNonQuery();
            }
        }
    }
}

#endif
