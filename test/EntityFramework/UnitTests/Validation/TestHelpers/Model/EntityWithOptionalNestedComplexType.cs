// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    public class EntityWithOptionalNestedComplexType
    {
        public int ID { get; set; }

        public AirportDetails AirportDetails { get; set; }
    }
}