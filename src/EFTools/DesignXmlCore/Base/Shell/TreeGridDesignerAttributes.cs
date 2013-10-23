// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Base.Shell
{
    using System;

    /// <summary>
    ///     Base class for TreeGrid designer attributes.  Implements IComparable functionality
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal abstract class TreeGridDesignerBaseAttribute : Attribute, IComparable
    {
        private int _order;

        #region IComparable implementation

        /// <summary>
        ///     IComparable interface implementation.
        /// </summary>
        public int /* IComparable */ CompareTo(object obj)
        {
            return _order - ((TreeGridDesignerBaseAttribute)obj)._order;
        }

        /// <summary>
        ///     Determines equality based on the Order property.
        /// </summary>
        public override bool Equals(object obj)
        {
            var attr = obj as TreeGridDesignerBaseAttribute;
            if (attr == null)
            {
                return false;
            }

            return _order == attr._order;
        }

        /// <summary>
        ///     Returns a unique hashcode for this object.
        /// </summary>
        public override int GetHashCode()
        {
            return _order.GetHashCode();
        }

        /// <summary>
        ///     Comparison based on the Order property.
        /// </summary>
        public static bool operator ==(TreeGridDesignerBaseAttribute attr1, TreeGridDesignerBaseAttribute attr2)
        {
            return attr1._order == attr2._order;
        }

        /// <summary>
        ///     Comparison based on the Order property.
        /// </summary>
        public static bool operator !=(TreeGridDesignerBaseAttribute attr1, TreeGridDesignerBaseAttribute attr2)
        {
            return attr1._order != attr2._order;
        }

        /// <summary>
        ///     Comparison based on the Order property.
        /// </summary>
        public static bool operator <=(TreeGridDesignerBaseAttribute attr1, TreeGridDesignerBaseAttribute attr2)
        {
            return attr1._order <= attr2._order;
        }

        /// <summary>
        ///     Comparison based on the Order property.
        /// </summary>
        public static bool operator >=(TreeGridDesignerBaseAttribute attr1, TreeGridDesignerBaseAttribute attr2)
        {
            return attr1._order >= attr2._order;
        }

        /// <summary>
        ///     Comparison based on the Order property.
        /// </summary>
        public static bool operator <(TreeGridDesignerBaseAttribute attr1, TreeGridDesignerBaseAttribute attr2)
        {
            return attr1._order < attr2._order;
        }

        /// <summary>
        ///     Comparison based on the Order property.
        /// </summary>
        public static bool operator >(TreeGridDesignerBaseAttribute attr1, TreeGridDesignerBaseAttribute attr2)
        {
            return attr1._order > attr2._order;
        }

        #endregion

        /// <summary>
        ///     Determines the order of rows or columns to be displayed.  Lower values are
        ///     displayed above or to the left of higher values.
        /// </summary>
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }
    }

    /// <summary>
    ///     Attribute which may be placed on a selectable object to specify an
    ///     TreeGridDesignerBranch that should be displayed in the TreeGrid Designer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class TreeGridDesignerRootBranchAttribute : TreeGridDesignerBaseAttribute
    {
        private readonly Type _branchType;

        /// <summary>
        ///     Construct an empty OperationDesignerRootBranchAttribute
        /// </summary>
        internal TreeGridDesignerRootBranchAttribute()
        {
        }

        /// <summary>
        ///     Construct an OperationDesignerRootBranchAttribute with the given branch type.
        /// </summary>
        /// <param name="branchType"></param>
        internal TreeGridDesignerRootBranchAttribute(Type branchType)
        {
            _branchType = branchType;
        }

        /// <summary>
        ///     Type of branch to be created.
        /// </summary>
        internal Type BranchType
        {
            get { return _branchType; }
        }
    }

    /// <summary>
    ///     Attribute which may be placed on a selectable object to specify a
    ///     column that should be displayed in the TreeGridDesigner
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class TreeGridDesignerColumnAttribute : TreeGridDesignerBaseAttribute
    {
        private readonly Type _columnType;

        /// <summary>
        ///     Construct an empty OperationDesignerColumnsAttribute
        /// </summary>
        internal TreeGridDesignerColumnAttribute()
            : this(null)
        {
        }

        /// <summary>
        ///     Construct an OperationDesignerColumnAttribute with the given column type.
        /// </summary>
        /// <param name="columnType">Type of column to display</param>
        internal TreeGridDesignerColumnAttribute(Type columnType)
        {
            _columnType = columnType;
            InitialPercentage = TreeGridDesignerColumnDescriptor.CalculatePercentage;
        }

        /// <summary>
        ///     Type of column to be created.
        /// </summary>
        internal Type ColumnType
        {
            get { return _columnType; }
        }

        /// <summary>
        ///     Initial width of this column, as a percentage of the total width.  A value of
        ///     The default value of ColumnDescriptor.CalculatePercentage indicates that the percentage should be calculated by the tree control.
        ///     Otherwise, the value should be in the range (0, 1).
        /// </summary>
        public float InitialPercentage { get; set; }
    }
}
