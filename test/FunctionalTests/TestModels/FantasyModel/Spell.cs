// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    using System.Collections.Generic;

    public abstract class Spell
    {
        public int Id { get; set; }
        
        public int MagickaCost { get; set; }

        // many - many self reference 1 property
        public virtual ICollection<Spell> SynergyWith { get; set; }
    }

    public class CombatSpell : Spell
    {
        public int Damage { get; set; }

        // enum property
        public DamageType DamageType { get; set; }
    }

    public class SupportSpell : Spell
    {
        public string Description { get; set; }
    }
}
