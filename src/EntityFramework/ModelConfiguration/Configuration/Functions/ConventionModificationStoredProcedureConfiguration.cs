// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    /// <summary>
    /// Creates a convention that configures stored procedures to be used to modify entities in the database.
    /// </summary>
    public abstract class ConventionModificationStoredProcedureConfiguration
    {
        private readonly ModificationStoredProcedureConfiguration _configuration
            = new ModificationStoredProcedureConfiguration();

        internal ConventionModificationStoredProcedureConfiguration()
        {
        }

        internal ModificationStoredProcedureConfiguration Configuration
        {
            get { return _configuration; }
        }
    }
}
