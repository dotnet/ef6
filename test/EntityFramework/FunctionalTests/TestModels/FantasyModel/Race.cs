// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    using System.Collections.Generic;

    public class Race
    {
        public string RaceName { get; set; }

        // Many - Many unidirectional
        public virtual ICollection<Skill> SkillBonuses { get; set; }
    }
}
