// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ConcurrencyModel
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Engine
    {
        public int Id { get; set; }

        [ConcurrencyCheck]
        public string Name { get; set; }

        public Location StorageLocation { get; set; }

        [ConcurrencyCheck]
        public int EngineSupplierId { get; set; }

        public virtual EngineSupplier EngineSupplier { get; set; }

        public virtual ICollection<Team> Teams { get; set; }

        public virtual ICollection<Gearbox> Gearboxes { get; set; } // Uni-directional
    }
}
