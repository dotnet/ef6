// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    using System.Collections.Generic;

    public enum SkillArchetype
    {
        Mage,
        Warrior,
        Thief,
    }

    public class Skill
    {
        // composite key, string + enum
        public SkillArchetype Archetype { get; set; }
        public int Ordinal { get; set; }

        public string Name { get; set; }

        // 1 - Many
        public virtual ICollection<Perk> Perks { get; set; }
    }
}
