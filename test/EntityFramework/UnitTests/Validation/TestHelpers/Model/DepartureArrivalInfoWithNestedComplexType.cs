// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class DepartureArrivalInfoWithNestedComplexType : IValidatableObject
    {
        public DepartureArrivalInfoWithNestedComplexType()
        {
            Time = DateTime.MaxValue;
        }

        [Required]
        public AirportDetails Airport { get; set; }

        public DateTime Time { get; set; }

        internal IEnumerable<ValidationResult> ValidationResults { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            return ValidationResults ?? (Time <= DateTime.Now
                                             ? new[] { new ValidationResult("Date cannot be in the past.") }
                                             : Enumerable.Empty<ValidationResult>());
        }
    }
}