// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.GearsOfWarModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;

    public class GearsOfWarInitializer : DropCreateDatabaseIfModelChanges<GearsOfWarContext>
    {
        private const int GeographySrid = 4326;

        protected override void Seed(GearsOfWarContext context)
        {
            var lancer = new StandardWeapon
            {
                Name = "Lancer",
                Specs = new WeaponSpecification
                {
                    AmmoPerClip = 60,
                    ClipsCount = 8,
                }
            };

            var gnasher = new StandardWeapon
            {
                Name = "Gnasher",
                Specs = new WeaponSpecification
                {
                    AmmoPerClip = 8,
                    ClipsCount = 6,
                },
                
                SynergyWith = lancer,
            };

            var hammerburst = new StandardWeapon
            {
                Name = "Hammerburst",
                Specs = new WeaponSpecification
                {
                    AmmoPerClip = 20,
                    ClipsCount = 7,
                }
            };

            var markza = new StandardWeapon
            {
                Name = "Markza",
                Specs = new WeaponSpecification
                {
                AmmoPerClip = 10,
                ClipsCount = 12,
                },
                
                SynergyWith = gnasher,
            };

            var mulcher = new HeavyWeapon
            {
                Name = "Mulcher",
                Overheats = true,
            };

            context.Weapons.AddRange(new List<Weapon> { lancer, gnasher, hammerburst, markza, mulcher, });

            var deltaSquad = new Squad
            {
                Id = 1,
                Name = "Delta",
            };

            var kiloSquad = new Squad
            {
                Id = 2,
                Name = "Kilo",
            };

            context.Squads.AddRange(new[] { deltaSquad, kiloSquad });

            var jacinto = new City
            {
                Location = DbGeography.FromText("POINT(1 1)", GeographySrid),
                Name = "Jacinto",
            };

            var ephyra = new City
            {
                Location = DbGeography.FromText("POINT(2 2)", GeographySrid),
                Name = "Ephyra",
            };

            var hanover = new City
            {
                Location = DbGeography.FromText("POINT(3 3)", GeographySrid),
                Name = "Hanover",
            };

            context.Cities.AddRange(new[] { jacinto, ephyra, hanover });

            var marcusTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Marcus's Tag",
            };

            var domsTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Dom's Tag",
            };

            var colesTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Cole's Tag",
            };

            var bairdsTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Bairds's Tag",
            };

            var paduksTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Paduk's Tag",
            };

            var kiaTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "K.I.A.",
            };

            context.Tags.AddRange(
                new[] { marcusTag, domsTag, colesTag, bairdsTag, paduksTag, kiaTag });

            var marcus = new Gear
            {
                Nickname = "Marcus",
                FullName = "Marcus Fenix",
                Squad = deltaSquad,
                Rank = MilitaryRank.Sergeant,
                Tag = marcusTag,
                CityOfBirth = jacinto,
                Weapons = new List<Weapon> { lancer, gnasher },
            };

            var dom = new Gear
            {
                Nickname = "Dom",
                FullName = "Dominic Santiago",
                Squad = deltaSquad,
                Rank = MilitaryRank.Corporal,
                Tag = domsTag,
                CityOfBirth = ephyra,
                Weapons = new List<Weapon> { hammerburst, gnasher }
            };

            var cole = new Gear
            {
                Nickname = "Cole Train",
                FullName = "Augustus Cole",
                Squad = deltaSquad,
                Rank = MilitaryRank.Private,
                Tag = colesTag,
                CityOfBirth = hanover,
                Weapons = new List<Weapon> { gnasher, mulcher }
            };

            var baird = new Gear
            {
                Nickname = "Baird",
                FullName = "Damon Baird",
                Squad = deltaSquad,
                Rank = MilitaryRank.Corporal,
                Tag = bairdsTag,
                Weapons = new List<Weapon> { lancer, gnasher }
            };

            var paduk = new Gear
            {
                Nickname = "Paduk",
                FullName = "Garron Paduk",
                Squad = kiloSquad,
                Rank = MilitaryRank.Private,
                Tag = paduksTag,
                Weapons = new List<Weapon> { markza },
            };

            marcus.Reports = new List<Gear> { dom, cole, baird };
            baird.Reports = new List<Gear> { paduk };

            context.Gears.AddRange(new[] { marcus, dom, cole, baird, paduk });
        }
    }
}
