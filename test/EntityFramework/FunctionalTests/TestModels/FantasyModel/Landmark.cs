// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    // table splitting, different hierarchies
    public class Landmark
    {
        public int Id { get; set; }
        public Tower MatchingTower { get; set; }
        public Province LocatedIn { get; set; }
    }
}
