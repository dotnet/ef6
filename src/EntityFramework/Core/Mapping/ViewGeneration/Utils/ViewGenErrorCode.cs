// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Utils
{
    // This file contains an enum for the errors generated by ViewGen

    // There is almost a one-to-one correspondence between these error codes
    // and the resource strings - so if you need more insight into what the
    // error code means, please see the code that uses the particular enum
    // AND the corresponding resource string

    // error numbers end up being hard coded in test cases; they can be removed, but should not be changed.
    // reusing error numbers is probably OK, but not recommended.
    //
    // The acceptable range for this enum is
    // 3000 - 3999
    //
    // The Range 10,000-15,000 is reserved for tools
    //
    internal enum ViewGenErrorCode
    {
        Value = 3000, // ViewGenErrorBase

        // Filter condition on cell is invalid
        InvalidCondition = Value + 1,
        // Key constraint violation: C does not imply S
        KeyConstraintViolation = Value + 2,
        // Key constraint violation due to update's requirements: S does not
        // imply C approximately
        KeyConstraintUpdateViolation = Value + 3,
        // Some attributes of an extent are not present in the cells 
        AttributesUnrecoverable = Value + 4,
        // The partitions (from multiconstants) cannot be differentiated
        AmbiguousMultiConstants = Value + 5,
        //Unused: 6
        // Non-key projected multiple times (denormalized)
        NonKeyProjectedWithOverlappingPartitions = Value + 7,
        // New concurrency tokens defined in derived class
        ConcurrencyDerivedClass = Value + 8,
        // Concurrency token has a condition on it
        ConcurrencyTokenHasCondition = Value + 9,
        //Unused: 10
        // Domain constraint violated
        DomainConstraintViolation = Value + 12,
        // Foreign key constraint - child or parent table is not mapped
        ForeignKeyMissingTableMapping = Value + 13,
        // Foreign key constraint - C-space does not ensure that child is
        // contained in parent
        ForeignKeyNotGuaranteedInCSpace = Value + 14,
        // Expected foreign key to be mapped to some relationship
        ForeignKeyMissingRelationshipMapping = Value + 15,
        // Foreign key mapped to relationship - expected upper bound to be 1
        ForeignKeyUpperBoundMustBeOne = Value + 16,
        // Foreign key mapped to relationship - expected low bound to be 1
        ForeignKeyLowerBoundMustBeOne = Value + 17,
        // Foreign key mapped to relationship - but parent table not mapped
        // to any end of relationship
        ForeignKeyParentTableNotMappedToEnd = Value + 18,
        // Foreign key mapping to C-space does not preserve colum order
        ForeignKeyColumnOrderIncorrect = Value + 19,
        // Disjointness constraint violated in C-space
        DisjointConstraintViolation = Value + 20,
        // Columns of a table mapped to multiple C-side properties
        DuplicateCPropertiesMapped = Value + 21,
        // Field has not null condition but is not mapped
        NotNullNoProjectedSlot = Value + 22,
        // Column is not nullable and has no default value
        NoDefaultValue = Value + 23,
        // All key properties of association set or entity set not mapped
        KeyNotMappedForCSideExtent = Value + 24,
        // All key properties of table not mapped
        KeyNotMappedForTable = Value + 25,
        // Partition constraint violated in C-space
        PartitionConstraintViolation = Value + 26,
        // Mapping for C-side extent not specified
        MissingExtentMapping = Value + 27,
        //Unused: 28
        //Unused: 29
        // Mapping condition that is not possible according to S-side constraints
        ImpossibleCondition = Value + 30,
        // NonNullable S-Side member is mapped to nullable C-Side member
        NullableMappingForNonNullableColumn = Value + 31,
        //Error specifying Conditions, caught during Error Pattern Matching
        ErrorPatternConditionError = Value + 32,
        //Invalid ways of splitting Extents, caught during Error Pattern Matching
        ErrorPatternSplittingError = Value + 33,
        //Invalid mapping in terms of equality/disjointness constraint, caught during Error Pattern Matching
        ErrorPatternInvalidPartitionError = Value + 34,
        //Some type does not have mapping specified
        ErrorPatternMissingMappingError = Value + 35,
        //Mapping fragments don't overlap on a key or foreign key under read-only scenario
        NoJoinKeyOrFKProvidedInMapping = Value + 36,
        //If there is a fragment with distinct flag, there should be no other fragment between that C and S extent
        MultipleFragmentsBetweenCandSExtentWithDistinct = Value + 37,
    }
}
