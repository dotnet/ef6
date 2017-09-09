// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Hierarchy
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Represents hierarchical data.
    /// </summary>
    [DataContract]
    [Serializable]
    public class HierarchyId : IComparable
    {
        private readonly string _hierarchyId;
        private readonly int[][] _nodes;

        /// <summary>
        /// The Path separator character
        /// </summary>
        public const string PathSeparator = "/";

        private const string InvalidHierarchyIdExceptionMessage =
            "The input string '{0}' is not a valid string representation of a HierarchyId node.";

        private const string GetReparentedValueOldRootExceptionMessage =
            "HierarchyId.GetReparentedValue failed because 'oldRoot' was not an ancestor node of 'this'.  'oldRoot' was '{0}', and 'this' was '{1}'.";

        private const string GetDescendantMostBeChildExceptionMessage =
            "HierarchyId.GetDescendant failed because '{0}' must be a child of 'this'.  '{0}' was '{1}' and 'this' was '{2}'.";

        private const string GetDescendantChild1MustLessThanChild2ExceptionMessage =
            "HierarchyId.GetDescendant failed because 'child1' must be less than 'child2'.  'child1' was '{0}' and 'child2' was '{1}'.";

        /// <summary>
        ///     Constructs an HierarchyId.
        /// </summary>
        public HierarchyId()
        {
        }

        /// <summary>
        ///     Constructs an HierarchyId with the given canonical string representation value.
        /// </summary>
        /// <returns>Hierarchyid value.</returns>
        /// <param name="hierarchyId">Canonical string representation</param>
        public HierarchyId(string hierarchyId)
        {
            _hierarchyId = hierarchyId;
            if (hierarchyId != null)
            {
                var nodesStr = hierarchyId.Split('/');
                if (!string.IsNullOrEmpty(nodesStr[0])
                    || !string.IsNullOrEmpty(nodesStr[nodesStr.Length - 1]))
                {
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture, InvalidHierarchyIdExceptionMessage, hierarchyId), "hierarchyId");
                }
                int nodesCount = nodesStr.Length - 2;
                var nodes = new int[nodesCount][];
                for (int i = 0; i < nodesCount; i++)
                {
                    string node = nodesStr[i + 1];
                    var intsStr = node.Split('.');
                    var ints = new int[intsStr.Length];
                    for (int j = 0; j < intsStr.Length; j++)
                    {
                        int num;
                        if (!int.TryParse(intsStr[j], out num))
                        {
                            throw new ArgumentException(
                                string.Format(CultureInfo.InvariantCulture, InvalidHierarchyIdExceptionMessage, hierarchyId), "hierarchyId");
                        }
                        ints[j] = num;
                    }
                    nodes[i] = ints;
                }
                _nodes = nodes;
            }
        }

        /// <summary>
        ///     Returns a hierarchyid representing the nth ancestor of this.
        /// </summary>
        /// <returns>A hierarchyid representing the nth ancestor of this.</returns>
        /// <param name="n">n</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "n")]
        public HierarchyId GetAncestor(int n)
        {
            if (_nodes == null
                || GetLevel() < n)
            {
                return new HierarchyId(null);
            }
            string hierarchyStr = PathSeparator +
                                  string.Join(PathSeparator, _nodes.Take(GetLevel() - n).Select(IntArrayToStirng))
                                  + PathSeparator;
            return new HierarchyId(hierarchyStr);
        }

        /// <summary>
        ///     Returns a child node of the parent.
        /// </summary>
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
        public HierarchyId GetDescendant(HierarchyId child1, HierarchyId child2)
        {
            if (_nodes == null)
            {
                return new HierarchyId(null);
            }
            if (child1 != null
                && (child1.GetLevel() != GetLevel() + 1 || !child1.IsDescendantOf(this)))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, GetDescendantMostBeChildExceptionMessage, "child1", child1, ToString()),
                    "child1");
            }
            if (child2 != null
                && (child2.GetLevel() != GetLevel() + 1 || !child2.IsDescendantOf(this)))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, GetDescendantMostBeChildExceptionMessage, "child2", child1, ToString()),
                    "child2");
            }
            if (child1 == null
                && child2 == null)
            {
                return new HierarchyId(ToString() + 1 + PathSeparator);
            }
            string hierarchyStr;
            if (child1 == null)
            {
                var result = new HierarchyId(child2.ToString());
                var lastNode = result._nodes.Last();
                //decrease the last part of the last node of the 1nd child
                lastNode[lastNode.Length - 1]--;
                hierarchyStr = PathSeparator +
                               string.Join(PathSeparator, result._nodes.Select(IntArrayToStirng))
                               + PathSeparator;
                return new HierarchyId(hierarchyStr);
            }
            if (child2 == null)
            {
                var result = new HierarchyId(child1.ToString());
                var lastNode = result._nodes.Last();
                //increase the last part of the last node of the 2nd child
                lastNode[lastNode.Length - 1]++;
                hierarchyStr = PathSeparator +
                               string.Join(PathSeparator, result._nodes.Select(IntArrayToStirng))
                               + PathSeparator;
                return new HierarchyId(hierarchyStr);
            }
            var child1LastNode = child1._nodes.Last();
            var child2LastNode = child2._nodes.Last();
            var cmp = CompareIntArrays(child1LastNode, child2LastNode);
            if (cmp >= 0)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, GetDescendantChild1MustLessThanChild2ExceptionMessage, child1, child2),
                    "child1");
            }
            int firstDiffrenceIdx = 0;
            for (; firstDiffrenceIdx < child1LastNode.Length; firstDiffrenceIdx++)
            {
                if (child1LastNode[firstDiffrenceIdx] < child2LastNode[firstDiffrenceIdx])
                {
                    break;
                }
            }
            child1LastNode = child1LastNode.Take(firstDiffrenceIdx + 1).ToArray();
            if (child1LastNode[firstDiffrenceIdx] + 1 < child2LastNode[firstDiffrenceIdx])
            {
                child1LastNode[firstDiffrenceIdx]++;
            }
            else
            {
                child1LastNode = child1LastNode.Concat(new[] { 1 }).ToArray();
            }
            hierarchyStr = PathSeparator +
                           string.Join(PathSeparator, _nodes.Select(IntArrayToStirng))
                           + PathSeparator
                           + IntArrayToStirng(child1LastNode)
                           + PathSeparator;
            return new HierarchyId(hierarchyStr);
        }

        /// <summary>
        ///     Returns an integer that represents the depth of the node this in the tree.
        /// </summary>
        /// <returns>An integer that represents the depth of the node this in the tree.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public short GetLevel()
        {
            if (_nodes == null)
            {
                return 0;
            }
            return (short)_nodes.Length;
        }

        /// <summary>
        ///     Returns the root of the hierarchy tree.
        /// </summary>
        /// <returns>The root of the hierarchy tree.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static HierarchyId GetRoot()
        {
            return DbHierarchyServices.GetRoot();
        }

        /// <summary>
        ///     Returns true if this is a descendant of parent.
        /// </summary>
        /// <returns>True if this is a descendant of parent.</returns>
        /// <param name="parent">parent</param>
        public bool IsDescendantOf(HierarchyId parent)
        {
            if (parent == null)
            {
                return true;
            }
            if (_nodes == null
                || parent.GetLevel() > GetLevel())
            {
                return false;
            }
            for (int i = 0; i < parent.GetLevel(); i++)
            {
                int cmp = CompareIntArrays(_nodes[i], parent._nodes[i]);
                if (cmp != 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Returns a node whose path from the root is the path to newRoot, followed by the path from oldRoot to this.
        /// </summary>
        /// <returns>Hierarchyid value.</returns>
        /// <param name="oldRoot">oldRoot</param>
        /// <param name="newRoot">newRoot</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Reparented")]
        public HierarchyId GetReparentedValue(HierarchyId oldRoot, HierarchyId newRoot)
        {
            if (oldRoot == null
                || newRoot == null)
            {
                return new HierarchyId(null);
            }
            if (!IsDescendantOf(oldRoot))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, GetReparentedValueOldRootExceptionMessage, oldRoot, ToString()), "oldRoot");
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(PathSeparator);
            foreach (var node in newRoot._nodes)
            {
                sb.Append(IntArrayToStirng(node));
                sb.Append(PathSeparator);
            }
            foreach (var node in _nodes.Skip(oldRoot.GetLevel()))
            {
                sb.Append(IntArrayToStirng(node));
                sb.Append(PathSeparator);
            }
            return new HierarchyId(sb.ToString());
        }

        /// <summary>
        ///     Converts the canonical string representation of a hierarchyid to a hierarchyid value.
        /// </summary>
        /// <returns>Hierarchyid value.</returns>
        /// <param name="input">input</param>
        public static HierarchyId Parse(string input)
        {
            return new HierarchyId(input);
        }

        private static string IntArrayToStirng(IEnumerable<int> array)
        {
            return string.Join(".", array);
        }

        private static int CompareIntArrays(int[] array1, int[] array2)
        {
            int count = Math.Min(array1.Length, array2.Length);
            for (int i = 0; i < count; i++)
            {
                int item1 = array1[i];
                int item2 = array2[i];
                if (item1 < item2)
                {
                    return -1;
                }
                if (item1 > item2)
                {
                    return 1;
                }
            }
            if (array1.Length > count)
            {
                return 1;
            }
            if (array2.Length > count)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        ///     Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> 
        ///     A 32-bit signed integer that indicates the lexical relationship between the two comparands.
        ///     Value Condition Less than zero: hid1 is less than hid2. 
        ///     Zero: hid1 equals hid2. 
        ///     Greater than zero: hid1 is greater than hid2. 
        /// </returns>
        public static int Compare(HierarchyId hid1, HierarchyId hid2)
        {
            var nodes1 = (object)hid1 == null ? null : hid1._nodes;
            var nodes2 = (object)hid2 == null ? null : hid2._nodes;
            if (nodes1 == null
                && nodes2 == null)
            {
                return 0;
            }
            if (nodes1 == null)
            {
                return -1;
            }
            if (nodes2 == null)
            {
                return 1;
            }
            int count = Math.Min(nodes1.Length, nodes2.Length);
            for (int i = 0; i < count; i++)
            {
                var node1 = nodes1[i];
                var node2 = nodes2[i];
                int cmp = CompareIntArrays(node1, node2);
                if (cmp != 0)
                {
                    return cmp;
                }
            }
            if (hid1._nodes.Length > count)
            {
                return 1;
            }
            if (hid2._nodes.Length > count)
            {
                return -1;
            }
            return 0;
        }

        /// <summary>
        ///     Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> 
        ///     true if the the first parameter is less than the second parameter, false otherwise 
        /// </returns>
        public static bool operator <(HierarchyId hid1, HierarchyId hid2)
        {
            int cmp = Compare(hid1, hid2);
            return cmp < 0;
        }

        /// <summary>
        ///     Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> 
        ///     true if the the first parameter is greater than the second parameter, false otherwise 
        /// </returns>
        public static bool operator >(HierarchyId hid1, HierarchyId hid2)
        {
            return hid2 < hid1;
        }

        /// <summary>
        ///     Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> 
        ///     true if the the first parameter is less or equal than the second parameter, false otherwise 
        /// </returns>
        public static bool operator <=(HierarchyId hid1, HierarchyId hid2)
        {
            int cmp = Compare(hid1, hid2);
            return cmp <= 0;
        }

        /// <summary>
        ///     Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> 
        ///     true if the the first parameter is greater or equal than the second parameter, false otherwise 
        /// </returns>
        public static bool operator >=(HierarchyId hid1, HierarchyId hid2)
        {
            return hid2 <= hid1;
        }

        /// <summary>
        ///     Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> true if the two HierarchyIds are equal, false otherwise </returns>
        public static bool operator ==(HierarchyId hid1, HierarchyId hid2)
        {
            int cmp = Compare(hid1, hid2);
            return cmp == 0;
        }

        /// <summary>
        ///     Compares two HierarchyIds by their values.
        /// </summary>
        /// <param name="hid1"> a HierarchyId to compare </param>
        /// <param name="hid2"> a HierarchyId to compare </param>
        /// <returns> true if the two HierarchyIds are not equal, false otherwise </returns>
        public static bool operator !=(HierarchyId hid1, HierarchyId hid2)
        {
            return !(hid1 == hid2);
        }

        /// <summary>
        ///     Compares this instance to a given HierarchyId by their values.
        /// </summary>
        /// <param name="other"> the HierarchyId to compare against this instance </param>
        /// <returns> true if this instance is equal to the given HierarchyId, and false otherwise </returns>
        protected bool Equals(HierarchyId other)
        {
            return this == other;
        }

        /// <summary>
        ///     Returns a value-based hash code, to allow HierarchyId to be used in hash tables.
        /// </summary>
        /// <returns> the hash value of this HierarchyId </returns>
        public override int GetHashCode()
        {
            return (_hierarchyId != null ? _hierarchyId.GetHashCode() : 0);
        }

        /// <summary>
        ///     Compares this instance to a given HierarchyId by their values.
        /// </summary>
        /// <param name="obj"> the HierarchyId to compare against this instance </param>
        /// <returns> true if this instance is equal to the given HierarchyId, and false otherwise </returns>
        public override bool Equals(object obj)
        {
            return Equals((HierarchyId)obj);
        }

        /// <summary>
        ///     Returns a string representation of the hierarchyid value.
        /// </summary>
        /// <returns>A string representation of the hierarchyid value.</returns>
        public override string ToString()
        {
            return _hierarchyId;
        }

        /// <summary>
        /// Implementation of IComparable.CompareTo()
        /// </summary>
        /// <param name="obj"> The object to compare to </param>
        /// <returns> 0 if the HierarchyIds are "equal" (i.e., have the same _hierarchyId value) </returns>
        public int CompareTo(object obj)
        {
            var loader = obj as HierarchyId;
            if (loader != null)
            {
                return Compare(this, loader);
            }

            Debug.Assert(false, "object is not a HierarchyId");
            return -1;
        }
    }
}
