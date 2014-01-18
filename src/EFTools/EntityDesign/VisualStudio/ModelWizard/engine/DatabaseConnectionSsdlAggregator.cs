// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;

    // <summary>
    //     Aggregate the tables/views/sprocs for display in the wizard by connecting to
    //     a database server
    // </summary>
    internal class DatabaseConnectionSsdlAggregator
    {
        private readonly ModelBuilderSettings _settings;

        internal DatabaseConnectionSsdlAggregator(ModelBuilderSettings settings)
        {
            _settings = settings;
        }

        public ICollection<EntityStoreSchemaFilterEntry> GetTableFilterEntries(DoWorkEventArgs args)
        {
            return DatabaseMetadataQueryTool.GetTablesFilterEntries(_settings, args);
        }

        public ICollection<EntityStoreSchemaFilterEntry> GetViewFilterEntries(DoWorkEventArgs args)
        {
            return DatabaseMetadataQueryTool.GetViewFilterEntries(_settings, args);
        }

        public ICollection<EntityStoreSchemaFilterEntry> GetFunctionFilterEntries(DoWorkEventArgs args)
        {
            return DatabaseMetadataQueryTool.GetFunctionsFilterEntries(_settings, args);
        }
    }
}
