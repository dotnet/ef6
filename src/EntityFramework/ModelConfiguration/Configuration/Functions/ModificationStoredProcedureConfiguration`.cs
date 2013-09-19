// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    /// <summary>
    /// Performs configuration of a stored procedure uses to modify an entity in the database.
    /// </summary>
    public abstract class ModificationStoredProcedureConfigurationBase
    {
        private readonly ModificationStoredProcedureConfiguration _configuration
            = new ModificationStoredProcedureConfiguration();

        internal ModificationStoredProcedureConfigurationBase()
        {
        }

        internal ModificationStoredProcedureConfiguration Configuration
        {
            get { return _configuration; }
        }
    }
}
