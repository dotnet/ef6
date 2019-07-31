// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Spatial;

    public class ArubaRun
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Purpose { get; set; }
        public ArubaOwner RunOwner { get; set; }
        public ICollection<ArubaTask> Tasks { get; set; }
        public DbGeometry Geometry { get; set; }
    }
}