// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Identifies conventions that can be added to or removed from a <see cref="DbModelBuilder" /> instance.
    /// </summary>
    /// <remarks>
    /// Note that implementations of this interface must be immutable.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IConvention
    {
    }
}
