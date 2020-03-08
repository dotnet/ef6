// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CollationAttribute : Attribute
    {
        public const string AnnotationName = "Collation";

        private readonly string _collationName;

        public CollationAttribute(string collationName)
        {
            _collationName = collationName;
        }

        public string CollationName
        {
            get { return _collationName; }
        }

        private bool Equals(CollationAttribute other)
        {
            return base.Equals(other)
                   && string.Equals(_collationName, other._collationName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is CollationAttribute && Equals((CollationAttribute)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ _collationName.GetHashCode();
            }
        }

        public override string ToString()
        {
            return "Collation: " + _collationName;
        }
    }
}
