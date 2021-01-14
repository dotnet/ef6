// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.GearsOfWarModel
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Squad
    {
        // non-auto generated key
        public int Id { get; set; }
        public string Name { get; set; }
        
        // auto-generated non-key
        public int InternalNumber { get; set; }

        public virtual ICollection<Gear> Members { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; } 
    }
}
