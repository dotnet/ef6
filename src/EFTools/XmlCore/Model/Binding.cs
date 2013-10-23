// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    internal abstract class Binding
    {
        /// <summary>
        ///     Indicates the status of the reference based on whether the parsing
        ///     is complete or not and how many, if any, objects of the sought type
        ///     and with the given name in the reference have been found.
        /// </summary>
        internal abstract BindingStatus Status { get; }
    }
}
