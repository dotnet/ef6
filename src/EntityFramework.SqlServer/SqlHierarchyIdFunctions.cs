// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.Hierarchy;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Contains function stubs that expose SqlServer methods in Linq to Entities.
    /// </summary>
    public static class SqlHierarchyIdFunctions
    {
        /// <summary>Returns a hierarchyid representing the nth ancestor of this.</summary>
        /// <returns>A hierarchyid representing the nth ancestor of this.</returns>
        /// <param name="hierarchyIdValue">The hierarchyid value.</param>
        /// <param name="n">n</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hierarchyIdValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "n")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "n")]
        [DbFunction("SqlServer", "GetAncestor")]
        public static HierarchyId GetAncestor(HierarchyId hierarchyIdValue, int n)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a child node of the parent.</summary>
        /// <param name="hierarchyIdValue">The hierarchyid value.</param>
        /// <param name="child1"> null or the hierarchyid of a child of the current node. </param>
        /// <param name="child2"> null or the hierarchyid of a child of the current node. </param>
        /// <returns>
        /// Returns one child node that is a descendant of the parent.
        /// If parent is null, returns null.
        /// If parent is not null, and both child1 and child2 are null, returns a child of parent.
        /// If parent and child1 are not null, and child2 is null, returns a child of parent greater than child1.
        /// If parent and child2 are not null and child1 is null, returns a child of parent less than child2.
        /// If parent, child1, and child2 are not null, returns a child of parent greater than child1 and less than child2.
        /// If child1 is not null and not a child of parent, an exception is raised.
        /// If child2 is not null and not a child of parent, an exception is raised.
        /// If child1 >= child2, an exception is raised.
        /// </returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hierarchyIdValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "child1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "child2")]
        [DbFunction("SqlServer", "GetDescendant")]
        public static HierarchyId GetDescendant(HierarchyId hierarchyIdValue, HierarchyId child1, HierarchyId child2)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns an integer that represents the depth of the node this in the tree.</summary>
        /// <returns>An integer that represents the depth of the node this in the tree.</returns>
        /// <param name="hierarchyIdValue">The hierarchyid value.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hierarchyIdValue")]
        [DbFunction("SqlServer", "GetLevel")]
        public static short GetLevel(HierarchyId hierarchyIdValue)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns the root of the hierarchy tree.</summary>
        /// <returns>The root of the hierarchy tree.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [DbFunction("SqlServer", "GetRoot")]
        public static HierarchyId GetRoot()
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns true if this is a descendant of parent.</summary>
        /// <returns>True if this is a descendant of parent.</returns>
        /// <param name="hierarchyIdValue">The hierarchyid value.</param>
        /// <param name="parent">parent</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hierarchyIdValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "parent")]
        [DbFunction("SqlServer", "IsDescendantOf")]
        public static bool IsDescendantOf(HierarchyId hierarchyIdValue, HierarchyId parent)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Returns a node whose path from the root is the path to newRoot, followed by the path from oldRoot to this.</summary>
        /// <returns>Hierarchyid value.</returns>
        /// <param name="hierarchyIdValue">The hierarchyid value.</param>
        /// <param name="oldRoot">oldRoot</param>
        /// <param name="newRoot">newRoot</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "hierarchyIdValue")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "oldRoot")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "newRoot")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Reparented")]
        [DbFunction("SqlServer", "GetReparentedValue")]
        public static HierarchyId GetReparentedValue(HierarchyId hierarchyIdValue, HierarchyId oldRoot, HierarchyId newRoot)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }

        /// <summary>Converts the canonical string representation of a hierarchyid to a hierarchyid value.</summary>
        /// <returns>Hierarchyid value.</returns>
        /// <param name="input">input</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "input")]
        [DbFunction("SqlServer", "Parse")]
        public static HierarchyId Parse(string input)
        {
            throw new NotSupportedException(Strings.ELinq_DbFunctionDirectCall);
        }
    }
}
