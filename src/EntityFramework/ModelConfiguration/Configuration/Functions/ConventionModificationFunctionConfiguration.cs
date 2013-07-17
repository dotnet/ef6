// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    /// <summary>
    ///     Creates a convention that configures stored procedures to be used to modify entities in the database.
    /// </summary>
    public abstract class ConventionModificationFunctionConfiguration
    {
        private readonly ModificationFunctionConfiguration _configuration
            = new ModificationFunctionConfiguration();

        internal ConventionModificationFunctionConfiguration()
        {
        }

        internal ModificationFunctionConfiguration Configuration
        {
            get { return _configuration; }
        }
    }
}
