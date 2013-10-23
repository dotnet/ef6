// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    /// <summary>An enumeration that provides information about which wizard started an .edmx file generation or update process.</summary>
    public enum WizardKind
    {
        /// <summary>Indicates that no wizard started an .edmx file modification process.</summary>
        None = 0,

        /// <summary>Indicates that the Entity Data Model Wizard started an .edmx file generation process.</summary>
        Generate = 1,

        /// <summary>Indicates that the Update Model Wizard started an .edmx file update process.</summary>
        UpdateModel = 2,
    }
}
