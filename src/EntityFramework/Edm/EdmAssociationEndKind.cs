// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm
{
    /// <summary>
    ///     Indicates the multiplicity of an <see cref="EdmAssociationEnd" /> and whether or not it is required.
    /// </summary>
    internal enum EdmAssociationEndKind
    {
        Optional = 0,
        Required = 1,
        Many = 2,
    }
}
