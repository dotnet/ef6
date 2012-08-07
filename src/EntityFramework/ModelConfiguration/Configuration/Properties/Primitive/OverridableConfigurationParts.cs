// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    /// <summary>
    ///     Indicates what parts of a configuration are overridable.
    /// </summary>
    [Flags]
    internal enum OverridableConfigurationParts
    {
        /// <summary>
        ///     Nothing in the configuration is overridable.
        /// </summary>
        None = 0x0,

        /// <summary>
        ///     The configuration values related to C-Space are overridable.
        /// </summary>
        OverridableInCSpace = 0x1,

        /// <summary>
        ///     The configuration values only related to S-Space are overridable.
        /// </summary>
        OverridableInSSpace = 0x2
    }
}
