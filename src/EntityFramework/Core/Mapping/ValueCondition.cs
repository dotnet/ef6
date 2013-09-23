// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    // <summary>
    // Represents a simple value condition of the form (value IS NULL), (value IS NOT NULL)
    // or (value EQ X). Supports IEquatable(Of ValueCondition) so that equivalent conditions
    // can be identified.
    // </summary>
    internal class ValueCondition : IEquatable<ValueCondition>
    {
        internal readonly string Description;
        internal readonly bool IsSentinel;

        internal const string IsNullDescription = "NULL";
        internal const string IsNotNullDescription = "NOT NULL";
        internal const string IsOtherDescription = "OTHER";

        internal static readonly ValueCondition IsNull = new ValueCondition(IsNullDescription, true);
        internal static readonly ValueCondition IsNotNull = new ValueCondition(IsNotNullDescription, true);
        internal static readonly ValueCondition IsOther = new ValueCondition(IsOtherDescription, true);

        private ValueCondition(string description, bool isSentinel)
        {
            Description = description;
            IsSentinel = isSentinel;
        }

        internal ValueCondition(string description)
            : this(description, false)
        {
        }

        internal bool IsNotNullCondition
        {
            get { return ReferenceEquals(this, IsNotNull); }
        }

        public bool Equals(ValueCondition other)
        {
            return other.IsSentinel == IsSentinel &&
                   other.Description == Description;
        }

        public override int GetHashCode()
        {
            return Description.GetHashCode();
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
