// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    /// <summary>
    ///     WizardStage indicates whether the Wizard Extension Page occurs in the wizard before or after model generation.
    /// </summary>
    public enum WizardStage
    {
        /// <summary>
        ///     Wizard Extension Page occurs before model generation.
        /// </summary>
        PreModelGeneration,

        /// <summary>
        ///     Wizard Extension Page occurs after model generation.
        /// </summary>
        PostModelGeneration
    };
}
