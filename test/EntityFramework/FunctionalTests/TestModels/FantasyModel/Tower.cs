// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    // table splitting, different hierarchies
    public class Tower
    {
        public int Id { get; set; }
        public Landmark MatchingLandnmark { get; set; }
        public Province LocatedIn { get; set; }
    }
}
