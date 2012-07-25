// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents a key composed of multiple parts.
    /// </summary>
    internal class CompositeKey
    {
        #region Fields

        /// <summary>
        /// Gets components of this composite key.
        /// </summary>
        internal readonly PropagatorResult[] KeyComponents;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize a new composite key using the given constant values. Order is important.
        /// </summary>
        /// <param name="values">Key values.</param>
        internal CompositeKey(PropagatorResult[] constants)
        {
            Debug.Assert(null != constants, "key values must be given");

            KeyComponents = constants;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a key comparer operating in the context of the given translator.
        /// </summary>
        internal static IEqualityComparer<CompositeKey> CreateComparer(KeyManager keyManager)
        {
            return new CompositeKeyComparer(keyManager);
        }

        /// <summary>
        /// Creates a merged key instance where each key component contains both elements.
        /// </summary>
        /// <param name="other">Must be a non-null compatible key (same number of components).</param>
        /// <returns>Merged key.</returns>
        internal CompositeKey Merge(KeyManager keyManager, CompositeKey other)
        {
            Debug.Assert(null != other && other.KeyComponents.Length == KeyComponents.Length, "expected a compatible CompositeKey");
            var mergedKeyValues = new PropagatorResult[KeyComponents.Length];
            for (var i = 0; i < KeyComponents.Length; i++)
            {
                mergedKeyValues[i] = KeyComponents[i].Merge(keyManager, other.KeyComponents[i]);
            }
            return new CompositeKey(mergedKeyValues);
        }

        #endregion

        /// <summary>
        /// Equality and comparison implementation for composite keys.
        /// </summary>
        private class CompositeKeyComparer : IEqualityComparer<CompositeKey>
        {
            private readonly KeyManager _manager;

            internal CompositeKeyComparer(KeyManager manager)
            {
                Contract.Requires(manager != null);

                _manager = manager;
            }

            // determines equality by comparing each key component
            public bool Equals(CompositeKey left, CompositeKey right)
            {
                // Short circuit the comparison if we know the other reference is equivalent
                if (ReferenceEquals(left, right))
                {
                    return true;
                }

                // If either side is null, return false order (both can't be null because of
                // the previous check)
                if (null == left
                    || null == right)
                {
                    return false;
                }

                Debug.Assert(
                    null != left.KeyComponents && null != right.KeyComponents,
                    "(Update/JoinPropagator) CompositeKey must be initialized");

                if (left.KeyComponents.Length
                    != right.KeyComponents.Length)
                {
                    return false;
                }

                for (var i = 0; i < left.KeyComponents.Length; i++)
                {
                    var leftValue = left.KeyComponents[i];
                    var rightValue = right.KeyComponents[i];

                    // if both side are identifiers, check if they're the same or one is constrained by the
                    // other (if there is a dependent-principal relationship, they get fixed up to the same
                    // value)
                    if (leftValue.Identifier
                        != PropagatorResult.NullIdentifier)
                    {
                        if (rightValue.Identifier == PropagatorResult.NullIdentifier
                            ||
                            _manager.GetCliqueIdentifier(leftValue.Identifier) != _manager.GetCliqueIdentifier(rightValue.Identifier))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (rightValue.Identifier != PropagatorResult.NullIdentifier
                            ||
                            !ByValueEqualityComparer.Default.Equals(leftValue.GetSimpleValue(), rightValue.GetSimpleValue()))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            // creates a hash code by XORing hash codes for all key components.
            public int GetHashCode(CompositeKey key)
            {
                var result = 0;
                foreach (var keyComponent in key.KeyComponents)
                {
                    result = (result << 5) ^ GetComponentHashCode(keyComponent);
                }

                return result;
            }

            // Gets the value to use for hash code
            private int GetComponentHashCode(PropagatorResult keyComponent)
            {
                if (keyComponent.Identifier
                    == PropagatorResult.NullIdentifier)
                {
                    // no identifier exists for this key component, so use the actual key
                    // value
                    Debug.Assert(
                        null != keyComponent && null != keyComponent,
                        "key value must not be null");
                    return ByValueEqualityComparer.Default.GetHashCode(keyComponent.GetSimpleValue());
                }
                else
                {
                    // use ID for FK graph clique (this ensures that keys fixed up to the same
                    // value based on a constraint will have the same hash code)
                    return _manager.GetCliqueIdentifier(keyComponent.Identifier).GetHashCode();
                }
            }
        }
    }
}
