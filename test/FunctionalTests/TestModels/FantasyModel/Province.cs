// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;

    public class Province
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // spatial property
        public DbGeometry Shape { get; set; }

        // 1 - Many
        public ICollection<City> Cities { get; set; }

        public ICollection<Landmark> Landmarks { get; set; }
        public ICollection<Tower> Towers { get; set; }
    }
}
