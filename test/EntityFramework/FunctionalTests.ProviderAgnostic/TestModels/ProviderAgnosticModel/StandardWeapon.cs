// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ProviderAgnosticModel
{
    public class StandardWeapon : Weapon
    {
        public WeaponSpecification Specs { get; set; }
        
        // computed property
        public int TotalAmmo
        {
            get
            {
                return Specs.AmmoPerClip * Specs.ClipsCount;
            }
        }
    }

    // complex type
    public class WeaponSpecification
    {
        public int AmmoPerClip { get; set; }
        public int ClipsCount { get; set; }
    }
}
