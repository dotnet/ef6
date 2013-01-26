// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns
{
    using System;

    public partial class BuildingMf
    {
        public BuildingMf(Guid buildingId, string name, decimal value, AddressMf address)
            : this()
        {
            BuildingId = buildingId;
            Name = name;
            Value = value;
            Address = address;
        }
    }
}
