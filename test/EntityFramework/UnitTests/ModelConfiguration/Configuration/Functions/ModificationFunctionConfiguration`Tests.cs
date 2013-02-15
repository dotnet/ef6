// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Functions
{
    using System.Data.Entity.Spatial;

    public abstract class ModificationFunctionConfigurationTTests
    {
        protected class Entity
        {
            public int Int { get; set; }
            public short? Nullable { get; set; }
            public string String { get; set; }
            public byte[] Bytes { get; set; }
            public DbGeography Geography { get; set; }
            public DbGeometry Geometry { get; set; }
            public ComplexType ComplexType { get; set; }
        }

        protected class ComplexType
        {
            public int Int { get; set; }
        }
    }
}
