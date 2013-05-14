// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     This Class is never expected to be used except for by the FacetValues class.
    ///     The purpose of this class is to allow strong type checking by the compiler while setting facet values which
    ///     are typically stored as Object because they can either on of these things
    ///     1. null
    ///     2. scalar type (bool, int, byte)
    ///     3. Unbounded object
    ///     without this class it would be very easy to accidentally set precision to an int when it really is supposed to be
    ///     a byte value.  Also you would be able to set the facet value to any Object derived class (ANYTHING!!!) when really only
    ///     null and Unbounded are allowed besides an actual scalar value.  The magic of the class happens in the implicit constructors with
    ///     allow patterns like
    ///     new FacetValues( MaxLength = EdmConstants.UnboundedValue, Nullable = true};
    ///     and these are type checked at compile time
    /// </summary>
    internal struct FacetValueContainer<T>
    {
        private T _value;
        private bool _hasValue;
        private bool _isUnbounded;

        internal T Value
        {
            set
            {
                _isUnbounded = false;
                _hasValue = true;
                _value = value;
            }
        }

        private void SetUnbounded()
        {
            _isUnbounded = true;
            _hasValue = true;
        }

        // don't add an implicit conversion from object because it will kill the compile time type checking.
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "unbounded")]
        public static implicit operator FacetValueContainer<T>(EdmConstants.Unbounded unbounded)
        {
            Debug.Assert(
                ReferenceEquals(unbounded, EdmConstants.UnboundedValue),
                "you must pass the unbounded value.  If you are trying to set null, use the T parameter overload");
            var container = new FacetValueContainer<T>();
            container.SetUnbounded();
            return container;
        }

        public static implicit operator FacetValueContainer<T>(T value)
        {
            var container = new FacetValueContainer<T>();
            container.Value = value;
            return container;
        }

        internal object GetValueAsObject()
        {
            Debug.Assert(_hasValue, "Don't get the value if it has not been set");
            if (_isUnbounded)
            {
                return EdmConstants.UnboundedValue;
            }
            else
            {
                return _value;
            }
        }

        internal bool HasValue
        {
            get { return _hasValue; }
        }
    }
}
