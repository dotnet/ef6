// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     The enumeration defining the type semantics used to resolve function overloads. 
    ///     These flags are defined in the provider manifest per function definition.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
    public enum ParameterTypeSemantics
    {
        /// <summary>
        ///     Allow Implicit Conversion between given and formal argument types (default).
        /// </summary>
        AllowImplicitConversion = 0,

        /// <summary>
        ///     Allow Type Promotion between given and formal argument types.
        /// </summary>
        AllowImplicitPromotion = 1,

        /// <summary>
        ///     Use strict Equivalence only.
        /// </summary>
        ExactMatchOnly = 2
    }
}
