// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    /// <summary>
    ///     The operator types. Includes both scalar and relational operators, 
    ///     and physical and logical operators, and rule operators
    /// </summary>
    internal enum OpType
    {
        #region ScalarOpType

        /// <summary>
        ///     Constants
        /// </summary>
        Constant,

        /// <summary>
        ///     An internally generated constant
        /// </summary>
        InternalConstant,

        /// <summary>
        ///     An internally generated constant used as a null sentinel
        /// </summary>
        NullSentinel,

        /// <summary>
        ///     A null constant
        /// </summary>
        Null,

        /// <summary>
        ///     ConstantPredicate
        /// </summary>
        ConstantPredicate,

        /// <summary>
        ///     A Var reference
        /// </summary>
        VarRef,

        /// <summary>
        ///     GreaterThan
        /// </summary>
        GT,

        /// <summary>
        ///     >=
        /// </summary>
        GE,

        /// <summary>
        ///     Lessthan or equals
        /// </summary>
        LE,

        /// <summary>
        ///     Less than
        /// </summary>
        LT,

        /// <summary>
        ///     Equals
        /// </summary>
        EQ,

        /// <summary>
        ///     Not equals
        /// </summary>
        NE,

        /// <summary>
        ///     String comparison
        /// </summary>
        Like,

        /// <summary>
        ///     Addition
        /// </summary>
        Plus,

        /// <summary>
        ///     Subtraction
        /// </summary>
        Minus,

        /// <summary>
        ///     Multiplication
        /// </summary>
        Multiply,

        /// <summary>
        ///     Division
        /// </summary>
        Divide,

        /// <summary>
        ///     Modulus
        /// </summary>
        Modulo,

        /// <summary>
        ///     Unary Minus
        /// </summary>
        UnaryMinus,

        /// <summary>
        ///     And
        /// </summary>
        And,

        /// <summary>
        ///     Or
        /// </summary>
        Or,

        /// <summary>
        ///     Not
        /// </summary>
        Not,

        /// <summary>
        ///     is null
        /// </summary>
        IsNull,

        /// <summary>
        ///     switched case expression
        /// </summary>
        Case,

        /// <summary>
        ///     treat-as
        /// </summary>
        Treat,

        /// <summary>
        ///     is-of
        /// </summary>
        IsOf,

        /// <summary>
        ///     Cast
        /// </summary>
        Cast,

        /// <summary>
        ///     Internal cast
        /// </summary>
        SoftCast,

        /// <summary>
        ///     a basic aggregate
        /// </summary>
        Aggregate,

        /// <summary>
        ///     function call
        /// </summary>
        Function,

        /// <summary>
        ///     Reference to a "relationship" property
        /// </summary>
        RelProperty,

        /// <summary>
        ///     property reference
        /// </summary>
        Property,

        /// <summary>
        ///     entity constructor
        /// </summary>
        NewEntity,

        /// <summary>
        ///     new instance constructor for a named type(other than multiset, record)
        /// </summary>
        NewInstance,

        /// <summary>
        ///     new instance constructor for a named type and sub-types
        /// </summary>
        DiscriminatedNewEntity,

        /// <summary>
        ///     Multiset constructor
        /// </summary>
        NewMultiset,

        /// <summary>
        ///     record constructor
        /// </summary>
        NewRecord,

        /// <summary>
        ///     Get the key from a Ref
        /// </summary>
        GetRefKey,

        /// <summary>
        ///     Get the ref from an entity instance
        /// </summary>
        GetEntityRef,

        /// <summary>
        ///     create a reference
        /// </summary>
        Ref,

        /// <summary>
        ///     exists
        /// </summary>
        Exists,

        /// <summary>
        ///     get the singleton element from a collection
        /// </summary>
        Element,

        /// <summary>
        ///     Builds up a collection
        /// </summary>
        Collect,

        /// <summary>
        ///     gets the target entity pointed at by a reference
        /// </summary>
        Deref,

        /// <summary>
        ///     Traverse a relationship and get the references of the other end
        /// </summary>
        Navigate,

        #endregion

        #region RelOpType

        /// <summary>
        ///     A table scan
        /// </summary>
        ScanTable,

        /// <summary>
        ///     A view scan
        /// </summary>
        ScanView,

        /// <summary>
        ///     Filter
        /// </summary>
        Filter,

        /// <summary>
        ///     Project
        /// </summary>
        Project,

        /// <summary>
        ///     InnerJoin
        /// </summary>
        InnerJoin,

        /// <summary>
        ///     LeftOuterJoin
        /// </summary>
        LeftOuterJoin,

        /// <summary>
        ///     FullOuter join
        /// </summary>
        FullOuterJoin,

        /// <summary>
        ///     Cross join
        /// </summary>
        CrossJoin,

        /// <summary>
        ///     cross apply
        /// </summary>
        CrossApply,

        /// <summary>
        ///     outer apply
        /// </summary>
        OuterApply,

        /// <summary>
        ///     Unnest
        /// </summary>
        Unnest,

        /// <summary>
        ///     Sort
        /// </summary>
        Sort,

        /// <summary>
        ///     Constrained Sort (physical paging - Limit and Skip)
        /// </summary>
        ConstrainedSort,

        /// <summary>
        ///     GroupBy
        /// </summary>
        GroupBy,

        /// <summary>
        ///     GroupByInto (projects the group as well)
        /// </summary>
        GroupByInto,

        /// <summary>
        ///     UnionAll
        /// </summary>
        UnionAll,

        /// <summary>
        ///     Intersect
        /// </summary>
        Intersect,

        /// <summary>
        ///     Except
        /// </summary>
        Except,

        /// <summary>
        ///     Distinct
        /// </summary>
        Distinct,

        /// <summary>
        ///     Select a single row from a subquery
        /// </summary>
        SingleRow,

        /// <summary>
        ///     A table with exactly one row
        /// </summary>
        SingleRowTable,

        #endregion

        #region AncillaryOpType

        /// <summary>
        ///     Variable definition
        /// </summary>
        VarDef,

        /// <summary>
        ///     List of variable definitions
        /// </summary>
        VarDefList,

        #endregion

        #region RulePatternOpType

        /// <summary>
        ///     Leaf
        /// </summary>
        Leaf,

        #endregion

        #region PhysicalOpType

        /// <summary>
        ///     Physical Project
        /// </summary>
        PhysicalProject,

        /// <summary>
        ///     single-stream nest aggregation
        /// </summary>
        SingleStreamNest,

        /// <summary>
        ///     multi-stream nest aggregation
        /// </summary>
        MultiStreamNest,

        #endregion

        /// <summary>
        ///     NotValid
        /// </summary>
        MaxMarker,
        NotValid = MaxMarker
    }
}
