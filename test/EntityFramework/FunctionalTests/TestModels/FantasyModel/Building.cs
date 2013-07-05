// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    using System.Collections.Generic;

    public class Building
    {
        public int Id { get; set; }
    }

    public class Store : Building
    {
        // 0..1 - 1
        public Npc Owner { get; set; }
    }

    public class Home : Building
    {
        // 0..1 - Many
        public ICollection<Npc> Tenants { get; set; }
    }
}
