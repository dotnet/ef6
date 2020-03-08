// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.GearsOfWarModel
{
    using System.ComponentModel.DataAnnotations;

    public abstract class Weapon
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        
        // 1 - 1 self reference
        public virtual Weapon SynergyWith { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}
