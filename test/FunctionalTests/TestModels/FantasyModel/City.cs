// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    using System.Collections.Generic;

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int ProvinceId { get; set; }
        public Province Province { get; set; }

        public ICollection<Building> Buildings { get; set; }
    }
}
