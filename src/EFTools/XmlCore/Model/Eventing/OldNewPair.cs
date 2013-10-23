// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Eventing
{
    /// <summary>
    ///     A class that encapsulates an old and new value within an EfiChangeGroup,
    ///     with the old value being the value before the start of the change group
    ///     transaction and the new value being the value at the end.
    /// </summary>
    internal class OldNewPair
    {
        internal OldNewPair(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        ///     Property for accessing the old value.
        /// </summary>
        internal object OldValue { get; set; }

        /// <summary>
        ///     Property for accessing the new value.
        /// </summary>
        internal object NewValue { get; set; }
    }
}
