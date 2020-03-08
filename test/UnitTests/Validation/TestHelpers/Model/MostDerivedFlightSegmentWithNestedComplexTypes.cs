// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Validation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class MostDerivedFlightSegmentWithNestedComplexTypes : FlightSegmentWithNestedComplexTypesWithTypeLevelValidation,
                                                                  IValidatableObject
    {
        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            return null;
        }
    }
}