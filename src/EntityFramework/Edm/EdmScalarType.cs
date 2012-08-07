// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     The base for all all Entity Data Model (EDM) types that represent a scalar type from the EDM type system.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal abstract class EdmScalarType
        : EdmDataModelType
    {
    }
}
