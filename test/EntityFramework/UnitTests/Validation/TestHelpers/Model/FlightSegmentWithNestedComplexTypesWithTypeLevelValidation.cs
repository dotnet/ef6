// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    [CustomValidation(typeof(FlightSegmentWithNestedComplexTypesWithTypeLevelValidation), "FailOnRequest")]
    public class FlightSegmentWithNestedComplexTypesWithTypeLevelValidation : FlightSegmentWithNestedComplexTypes, IValidatableObject
    {
        public static bool ShouldFail { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Aircraft != null && Aircraft.Code == "A380" && FlightNumber == "QF0006"
                       ? new[]
                             {
                                 new ValidationResult("Your trip may end in Singapore.", new[] { "Aircraft.Code", "FlightNumber" })
                             }
                       : Enumerable.Empty<ValidationResult>();
        }

        public static ValidationResult FailOnRequest(object entity, ValidationContext validationContex)
        {
            return ShouldFail ? new ValidationResult("Validation failed.") : ValidationResult.Success;
        }
    }
}