// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ConcurrencyModel
{
    using System.ComponentModel.DataAnnotations;

    public class Location
    {
        [ConcurrencyCheck]
        public double Latitude { get; set; }

        [ConcurrencyCheck]
        public double Longitude { get; set; }
    }
}
