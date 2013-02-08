// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    using System.Data.Entity.Spatial;

    public class ArubaMachineConfig : ArubaConfig
    {
        public string Host { get; set; }
        public Guid Address { get; set; }
        public DbGeography Location { get; set; }
    }
}
