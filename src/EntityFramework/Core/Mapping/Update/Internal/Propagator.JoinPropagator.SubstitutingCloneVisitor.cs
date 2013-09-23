// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Data.Entity.Utilities;

    internal partial class Propagator
    {
        private partial class JoinPropagator
        {
            // <summary>
            // Describes the mode of behavior for the <see cref="PlaceholderPopulator" />.
            // </summary>
            private enum PopulateMode
            {
                // <summary>
                // Produce a null extension record (for outer joins) marked as modified
                // </summary>
                NullModified,

                // <summary>
                // Produce a null extension record (for outer joins) marked as preserve
                // </summary>
                NullPreserve,

                // <summary>
                // Produce a placeholder for a record that is known to exist but whose specific
                // values are unknown.
                // </summary>
                Unknown,
            }

            // <summary>
            // Fills in a placeholder with join key data (also performs a clone so that the
            // placeholder can be reused).
            // </summary>
            // <remarks>
            // Clones of placeholder nodes are created when either the structure of the node
            // needs to change or the record markup for the node needs to change.
            // </remarks>
            private static class PlaceholderPopulator
            {
                // <summary>
                // Construct a new placeholder with the shape of the given placeholder. Key values are
                // injected into the resulting place holder and default values are substituted with
                // either propagator constants or progagator nulls depending on the mode established
                // by the <paramref name="mode" /> flag.
                // </summary>
                // <remarks>
                // The key is essentially an array of values. The key map indicates that for a particular
                // placeholder an expression (keyMap.Keys) corresponds to some ordinal in the key array.
                // </remarks>
                // <param name="placeholder"> Placeholder to clone </param>
                // <param name="key"> Key to substitute </param>
                // <param name="placeholderKey"> Key elements in the placeholder (ordinally aligned with 'key') </param>
                // <param name="mode"> Mode of operation. </param>
                // <returns> Cloned placeholder with key values </returns>
                internal static PropagatorResult Populate(
                    PropagatorResult placeholder, CompositeKey key,
                    CompositeKey placeholderKey, PopulateMode mode)
                {
                    DebugCheck.NotNull(placeholder);
                    DebugCheck.NotNull(key);
                    DebugCheck.NotNull(placeholderKey);

                    // Figure out which flags to apply to generated elements.
                    var isNull = mode == PopulateMode.NullModified || mode == PopulateMode.NullPreserve;
                    var preserve = mode == PopulateMode.NullPreserve || mode == PopulateMode.Unknown;
                    var flags = PropagatorFlags.NoFlags;
                    if (!isNull)
                    {
                        flags |= PropagatorFlags.Unknown;
                    } // only null values are known
                    if (preserve)
                    {
                        flags |= PropagatorFlags.Preserve;
                    }

                    var result = placeholder.Replace(
                        node =>
                            {
                                // See if this is a key element
                                var keyIndex = -1;
                                for (var i = 0; i < placeholderKey.KeyComponents.Length; i++)
                                {
                                    if (placeholderKey.KeyComponents[i] == node)
                                    {
                                        keyIndex = i;
                                        break;
                                    }
                                }

                                if (keyIndex != -1)
                                {
                                    // Key value.
                                    return key.KeyComponents[keyIndex];
                                }
                                else
                                {
                                    // for simple entries, just return using the markup context for this
                                    // populator
                                    var value = isNull ? null : node.GetSimpleValue();
                                    return PropagatorResult.CreateSimpleValue(flags, value);
                                }
                            });

                    return result;
                }
            }
        }
    }
}
