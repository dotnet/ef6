// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    using System.Collections.Generic;

    public class Perk
    {
        // identity guid, key by convention
        public Guid PerkId { get; set; }

        // foreign composite key
        public SkillArchetype SkillArchetype { get; set; }
        public int? SkillOrdinal { get; set; }

        public virtual Skill Skill { get; set; }

        public string Name { get; set; }

        // many - many self reference, 2 properties
        public virtual ICollection<Perk> RequiredPerks { get; set; }
        public virtual ICollection<Perk> RequiredBy { get; set; }

        public int? RequiredSkillValue { get; set; }
    }
}
