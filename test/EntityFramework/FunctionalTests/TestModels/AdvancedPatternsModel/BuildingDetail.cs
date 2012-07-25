// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace AdvancedPatternsModel
{
    using System;

    public class BuildingDetail
    {
        public Guid BuildingId { get; set; }
        public Building Building { get; set; }
        public string Details { get; set; }
    }
}
