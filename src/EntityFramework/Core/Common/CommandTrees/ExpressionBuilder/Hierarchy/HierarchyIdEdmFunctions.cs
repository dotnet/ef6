// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder.Hierarchy
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Provides an API to construct <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" />s that invoke hierarchyid realted canonical EDM functions, and, where appropriate, allows that API to be accessed as extension methods on the expression type itself.
    /// </summary>
    public static class HierarchyIdEdmFunctions
    {
        // HierarchyId ‘Static’ Functions

        /// <summary>
        ///     Creates a <see cref="DbFunctionExpression" /> that invokes the canonical 'HierarchyIdParse' function with the
        ///     specified argument, which must have a string result type.
        ///     The result type of the expression is Edm.HierarchyId.
        /// </summary>
        /// <param name="input"> An expression that provides the canonical representation of the hierarchyid value. </param>
        /// <returns> A new DbFunctionExpression that returns a new hierarchyid value based on the specified value. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No overload of the canonical 'HierarchyIdParse' function accept an argument with the result type of
        ///     <paramref name="input" />
        ///     .
        /// </exception>
        public static DbFunctionExpression HierarchyIdParse(DbExpression input)
        {
            Check.NotNull(input, "input");
            return EdmFunctions.InvokeCanonicalFunction("HierarchyIdParse", input);
        }

        /// <summary>
        ///     Creates a <see cref="DbFunctionExpression" /> that invokes the canonical 'HierarchyIdGetRoot' function.
        ///     The result type of the expression is Edm.HierarchyId.
        /// </summary>
        /// <returns> A new DbFunctionExpression that returns a new root hierarchyid value. </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static DbFunctionExpression HierarchyIdGetRoot()
        {
            return EdmFunctions.InvokeCanonicalFunction("HierarchyIdGetRoot");
        }

        // HierarchyId ‘Instance’ Functions

        /// <summary>
        ///     Creates a <see cref="DbFunctionExpression" /> that invokes the canonical 'GetAncestor' function with the
        ///     specified argument, which must have an Int32 result type.
        ///     The result type of the expression is Edm.HierarchyId.
        /// </summary>
        /// <param name="hierarchyIdValue"> An expression that specifies the hierarchyid value. </param>
        /// <param name="n"> An expression that provides an integer value. </param>
        /// <returns> A new DbFunctionExpression that returns a hierarchyid. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="hierarchyIdValue" />
        ///     or
        ///     <paramref name="n" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No overload of the canonical 'GetAncestor' function accept an argument with the result type of
        ///     <paramref name="n" />
        ///     .
        /// </exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "n")]
        public static DbFunctionExpression GetAncestor(this DbExpression hierarchyIdValue, DbExpression n)
        {
            Check.NotNull(hierarchyIdValue, "hierarchyIdValue");
            Check.NotNull(n, "n");
            return EdmFunctions.InvokeCanonicalFunction("GetAncestor", hierarchyIdValue, n);
        }

        /// <summary>
        ///     Creates a <see cref="DbFunctionExpression" /> that invokes the canonical 'GetDescendant' function with the
        ///     specified argument, which must have a HierarchyId result type.
        ///     The result type of the expression is Edm.HierarchyId.
        /// </summary>
        /// <param name="hierarchyIdValue"> An expression that specifies the hierarchyid value. </param>
        /// <param name="child1"> An expression that provides a hierarchyid value. </param>
        /// <param name="child2"> An expression that provides a hierarchyid value. </param>
        /// <returns> A new DbFunctionExpression that returns a hierarchyid. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="hierarchyIdValue" />
        ///     or
        ///     <paramref name="child1" />
        ///     or
        ///     <paramref name="child2" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No overload of the canonical 'GetDescendant' function accept an argument with the result type of
        ///     <paramref name="child1" />
        ///     and
        ///     <paramref name="child2" />
        ///     .
        /// </exception>
        public static DbFunctionExpression GetDescendant(this DbExpression hierarchyIdValue, DbExpression child1, DbExpression child2)
        {
            Check.NotNull(hierarchyIdValue, "hierarchyIdValue");
            Check.NotNull(child1, "child1");
            Check.NotNull(child2, "child2");
            return EdmFunctions.InvokeCanonicalFunction("GetDescendant", hierarchyIdValue, child1, child2);
        }

        /// <summary>
        ///     Creates a <see cref="DbFunctionExpression" /> that invokes the canonical 'GetLevel' function.
        ///     The result type of the expression is Int32.
        /// </summary>
        /// <param name="hierarchyIdValue"> An expression that specifies the hierarchyid value. </param>
        /// <returns> A new DbFunctionExpression that returns the level of the given hierarchyid. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="hierarchyIdValue" />
        ///     is null.
        /// </exception>
        public static DbFunctionExpression GetLevel(this DbExpression hierarchyIdValue)
        {
            Check.NotNull(hierarchyIdValue, "hierarchyIdValue");
            return EdmFunctions.InvokeCanonicalFunction("GetLevel", hierarchyIdValue);
        }

        /// <summary>
        ///     Creates a <see cref="DbFunctionExpression" /> that invokes the canonical 'IsDescendantOf' function with the
        ///     specified argument, which must have a HierarchyId result type.
        ///     The result type of the expression is Int32.
        /// </summary>
        /// <param name="hierarchyIdValue"> An expression that specifies the hierarchyid value. </param>
        /// <param name="parent"> An expression that provides a hierarchyid value. </param>
        /// <returns> A new DbFunctionExpression that returns an integer value. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="hierarchyIdValue" />
        ///     or
        ///     <paramref name="parent" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No overload of the canonical 'IsDescendantOf' function accept an argument with the result type of
        ///     <paramref name="parent" />
        ///     .
        /// </exception>
        public static DbFunctionExpression IsDescendantOf(this DbExpression hierarchyIdValue, DbExpression parent)
        {
            Check.NotNull(hierarchyIdValue, "hierarchyIdValue");
            Check.NotNull(parent, "parent");
            return EdmFunctions.InvokeCanonicalFunction("IsDescendantOf", hierarchyIdValue, parent);
        }

        /// <summary>
        ///     Creates a <see cref="DbFunctionExpression" /> that invokes the canonical 'GetReparentedValue' function with the
        ///     specified arguments, which must have a HierarchyId result type.
        ///     The result type of the expression is Edm.HierarchyId.
        /// </summary>
        /// <param name="hierarchyIdValue"> An expression that specifies the hierarchyid value. </param>
        /// <param name="oldRoot"> An expression that provides a hierarchyid value. </param>
        /// <param name="newRoot"> An expression that provides a hierarchyid value. </param>
        /// <returns> A new DbFunctionExpression that returns a hierarchyid. </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="hierarchyIdValue" />
        ///     or
        ///     <paramref name="oldRoot" />
        ///     or
        ///     <paramref name="newRoot" />
        ///     is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     No overload of the canonical 'GetReparentedValue' function accept an argument with the result type of
        ///     <paramref name="oldRoot" />
        ///     and
        ///     <paramref name="newRoot" />
        ///     .
        /// </exception>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Reparented")]
        public static DbFunctionExpression GetReparentedValue(this DbExpression hierarchyIdValue, DbExpression oldRoot, DbExpression newRoot)
        {
            Check.NotNull(hierarchyIdValue, "hierarchyIdValue");
            Check.NotNull(oldRoot, "oldRoot");
            Check.NotNull(newRoot, "newRoot");
            return EdmFunctions.InvokeCanonicalFunction("GetReparentedValue", hierarchyIdValue, oldRoot, newRoot);
        }
    }
}
