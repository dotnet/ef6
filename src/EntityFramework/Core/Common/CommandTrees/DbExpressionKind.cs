namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Describes the different "kinds" (classes) of expressions
    /// </summary>
    public enum DbExpressionKind
    {
        /// <summary>
        /// True for all.
        /// </summary>
        All = 0,

        /// <summary>
        /// Logical And.
        /// </summary>
        And = 1,

        /// <summary>
        /// True for any.
        /// </summary>
        Any = 2,

        /// <summary>
        /// Conditional case statement.
        /// </summary>
        Case = 3,

        /// <summary>
        /// Polymorphic type cast.
        /// </summary>
        Cast = 4,

        /// <summary>
        /// A constant value.
        /// </summary>
        Constant = 5,

        /// <summary>
        /// Cross apply
        /// </summary>
        CrossApply = 6,

        /// <summary>
        /// Cross join
        /// </summary>
        CrossJoin = 7,

        /// <summary>
        /// Dereference.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Deref")]
        Deref = 8,

        /// <summary>
        /// Duplicate removal.
        /// </summary>
        Distinct = 9,

        /// <summary>
        /// Division.
        /// </summary>
        Divide = 10,

        /// <summary>
        /// Set to singleton conversion.
        /// </summary>
        Element = 11,

        /// <summary>
        /// Entity ref value retrieval.
        /// </summary>
        EntityRef = 12,

        /// <summary>
        /// Equality
        /// </summary>
        Equals = 13,

        /// <summary>
        /// Set subtraction
        /// </summary>
        Except = 14,

        /// <summary>
        /// Restriction.
        /// </summary>
        Filter = 15,

        /// <summary>
        /// Full outer join
        /// </summary>
        FullOuterJoin = 16,

        /// <summary>
        /// Invocation of a stand-alone function
        /// </summary>
        Function = 17,

        /// <summary>
        /// Greater than.
        /// </summary>
        GreaterThan = 18,

        /// <summary>
        /// Greater than or equal.
        /// </summary>
        GreaterThanOrEquals = 19,

        /// <summary>
        /// Grouping.
        /// </summary>
        GroupBy = 20,

        /// <summary>
        /// Inner join
        /// </summary>
        InnerJoin = 21,

        /// <summary>
        /// Set intersection.
        /// </summary>
        Intersect = 22,

        /// <summary>
        /// Empty set determination.
        /// </summary>
        IsEmpty = 23,

        /// <summary>
        /// Null determination.
        /// </summary>
        IsNull = 24,

        /// <summary>
        /// Type comparison (specified Type or Subtype).
        /// </summary>
        IsOf = 25,

        /// <summary>
        /// Type comparison (specified Type only).
        /// </summary>
        IsOfOnly = 26,

        /// <summary>
        /// Application of a lambda function
        /// </summary>
        Lambda = 57,

        /// <summary>
        /// Left outer join
        /// </summary>
        LeftOuterJoin = 27,

        /// <summary>
        /// Less than.
        /// </summary>
        LessThan = 28,

        /// <summary>
        /// Less than or equal.
        /// </summary>
        LessThanOrEquals = 29,

        /// <summary>
        /// String comparison.
        /// </summary>
        Like = 30,

        /// <summary>
        /// Result count restriction (TOP n).
        /// </summary>
        Limit = 31,

        /// <summary>
        /// Subtraction.
        /// </summary>
        Minus = 32,

        /// <summary>
        /// Modulo.
        /// </summary>
        Modulo = 33,

        /// <summary>
        /// Multiplication.
        /// </summary>
        Multiply = 34,

        /// <summary>
        /// Instance, row, and set construction.
        /// </summary>
        NewInstance = 35,

        /// <summary>
        /// Logical Not.
        /// </summary>
        Not = 36,

        /// <summary>
        /// Inequality.
        /// </summary>
        NotEquals = 37,

        /// <summary>
        /// Null.
        /// </summary>
        Null = 38,

        /// <summary>
        /// Set members by type (or subtype).
        /// </summary>
        OfType = 39,

        /// <summary>
        /// Set members by (exact) type.
        /// </summary>
        OfTypeOnly = 40,

        /// <summary>
        /// Logical Or.
        /// </summary>
        Or = 41,

        /// <summary>
        /// Outer apply.
        /// </summary>
        OuterApply = 42,

        /// <summary>
        /// A reference to a parameter.
        /// </summary>
        ParameterReference = 43,

        /// <summary>
        /// Addition.
        /// </summary>
        Plus = 44,

        /// <summary>
        /// Projection.
        /// </summary>
        Project = 45,

        /// <summary>
        /// Retrieval of a static or instance property.
        /// </summary>
        Property = 46,

        /// <summary>
        /// Reference.
        /// </summary>
        Ref = 47,

        /// <summary>
        /// Ref key value retrieval.
        /// </summary>
        RefKey = 48,

        /// <summary>
        /// Navigation of a (composition or association) relationship.
        /// </summary>
        RelationshipNavigation = 49,

        /// <summary>
        /// Entity or relationship set scan.
        /// </summary>
        Scan = 50,

        /// <summary>
        /// Skip elements of an ordered collection.
        /// </summary>
        Skip = 51,

        /// <summary>
        /// Sorting.
        /// </summary>
        Sort = 52,

        /// <summary>
        /// Type conversion.
        /// </summary>
        Treat = 53,

        /// <summary>
        /// Negation.
        /// </summary>
        UnaryMinus = 54,

        /// <summary>
        /// Set union (with duplicates).
        /// </summary>
        UnionAll = 55,

        /// <summary>
        /// A reference to a variable.
        /// </summary>
        VariableReference = 56
    }
}
