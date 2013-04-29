// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.GearsOfWarModel
{
    using System.Collections.Generic;

    public class Gear
    {
        // composite key
        public string Nickname { get; set; } 
        public int SquadId { get; set; }

        public string FullName { get; set; }

        // concurrecy token
        public MilitaryRank Rank { get; set; }

        public virtual CogTag Tag { get; set; } 
        public virtual Squad Squad { get; set; } 

        public virtual City CityOfBirth { get; set; }
        public virtual ICollection<Weapon> Weapons { get; set; }
        
        // 1 - many self reference
        public virtual ICollection<Gear> Reports { get; set; } 
    }
}
