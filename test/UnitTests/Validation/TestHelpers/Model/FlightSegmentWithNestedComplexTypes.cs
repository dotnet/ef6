// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.ComponentModel.DataAnnotations;

    public class FlightSegmentWithNestedComplexTypes
    {
        public int FlightSegmentId { get; set; }

        [RegularExpression(@"^[A-Z]{2}\d{4}$")]
        public string FlightNumber { get; set; }

        [Required]
        public DepartureArrivalInfoWithNestedComplexType Departure { get; set; }

        [Required]
        public DepartureArrivalInfoWithNestedComplexType Arrival { get; set; }

        public AircraftInfo Aircraft { get; set; }
    }
}