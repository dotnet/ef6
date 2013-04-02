// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.StoredProcedures
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Spatial;

    public class IceAndFireModel
    {
        public class House
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Words { get; set; }
            public DbGeometry Sigil { get; set; }
            public Land LandOwned { get; set; }
            public List<Human> ProminentMembers { get; set; }
        }

        public class Land
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DbGeography LocationOnMap { get; set; }
            public House RulingHouse { get; set; }
        }

        public class Creature
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public CreatureSize Size { get; set; }
        }

        public class Human : Creature
        {
            public DbGeography PlaceOfBirth { get; set; }
        }

        public class Animal : Creature
        {
            public bool IsCarnivore { get; set; }
            public bool IsDangerous { get; set; }
        }

        public enum CreatureSize
        {
            Small,
            Medium,
            Large,
            VeryLarge,
        };

        public class IceAndFireContext : DbContext
        {
            public IceAndFireContext(string nameOrConnectionString)
                : base(nameOrConnectionString)
            {
            }

            public DbSet<House> Houses { get; set; }
            public DbSet<Land> Lands { get; set; }
            public DbSet<Creature> Creatures { get; set; }

            public virtual ObjectResult<Animal> GetAnimalsAndHouses()
            {
                return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Animal>("GetAnimalsAndHouses");
            }

            public virtual ObjectResult<House> GetHousesAndAnimals()
            {
                return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<House>("GetHousesAndAnimals");
            }

            public virtual ObjectResult<House> GetHousesAndHouses()
            {
                return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<House>("GetHousesAndHouses");
            }

            public virtual ObjectResult<Human> GetHumansAndAnimals()
            {
                return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Human>("GetHumansAndAnimals");
            }

            public virtual ObjectResult<Land> GetLandsAndCreatures()
            {
                return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Land>("GetLandsAndCreatures");
            }
        }
    }
}
