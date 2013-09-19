// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    // if you edit this file be sure you change GeneratorErrorSeverity
    // also, they must stay in sync

    /// <summary>
    /// Defines the different severities of errors that can occur when validating an Entity Framework model.
    /// </summary>
    public enum EdmSchemaErrorSeverity
    {
        /// <summary>
        /// A warning that does not prevent the model from being used.
        /// </summary>
        Warning = 0,

        /// <summary>
        /// An error that prevents the model from being used.
        /// </summary>
        Error = 1,
    }
}
