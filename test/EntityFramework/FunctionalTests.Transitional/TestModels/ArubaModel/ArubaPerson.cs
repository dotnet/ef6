// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    using System.Collections.Generic;

    public class ArubaPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ArubaPerson Partner { get; set; }
        public ICollection<ArubaPerson> Children { get; set; }
        public ICollection<ArubaPerson> Parents { get; set; }
    }
}
