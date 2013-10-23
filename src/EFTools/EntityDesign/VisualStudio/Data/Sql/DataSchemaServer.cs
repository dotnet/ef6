// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Data.Sql
{
    using System;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.VisualStudio.Data.Services;
    using Microsoft.VisualStudio.Data.Services.RelationalObjectModel;
    using Microsoft.VisualStudio.Data.Services.SupportEntities;

    internal class DataSchemaServer : IDataSchemaServer
    {
        private readonly IVsDataConnection _connection;
        private readonly IVsDataMappedObjectSelector _selector;

        public DataSchemaServer(IVsDataConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            _connection = connection;
            _selector = (connection).GetService(typeof(IVsDataMappedObjectSelector)) as IVsDataMappedObjectSelector;
            if (_selector == null)
            {
                throw new ArgumentException("IVsDataMappedObjectSelector");
            }
        }

        #region IDataProviderSchemaServer Members

        public IVsDataConnection Connection
        {
            get { return _connection; }
        }

        public IDataSchemaProcedure GetProcedureOrFunction(string schemaName, string objectName)
        {
            IDataSchemaProcedure procedureOrFunction = null;
            if (!string.IsNullOrEmpty(schemaName)
                && !string.IsNullOrEmpty(objectName))
            {
                var procedureList =
                    _selector.SelectMappedObjects<IVsDataStoredProcedure>(new object[] { DefaultCatalog, schemaName, objectName }, true);
                if (procedureList.Count > 0)
                {
                    procedureOrFunction = new DataSchemaProcedure(this, procedureList[0]);
                }
                else
                {
                    var functionList =
                        _selector.SelectMappedObjects<IVsDataScalarFunction>(new object[] { DefaultCatalog, schemaName, objectName }, true);
                    if (functionList.Count > 0)
                    {
                        procedureOrFunction = new DataSchemaProcedure(this, functionList[0]);
                    }
                    else
                    {
                        var tvfList =
                            _selector.SelectMappedObjects<IVsDataTabularFunction>(
                                new object[] { DefaultCatalog, schemaName, objectName }, true);
                        if (tvfList.Count > 0)
                        {
                            procedureOrFunction = new DataSchemaProcedure(this, tvfList[0]);
                        }
                    }
                }
            }

            return procedureOrFunction;
        }

        public IVsDataMappedObjectSelector Selector
        {
            get { return _selector; }
        }

        #endregion

        #region IDataSchemaServer Members

        public string DefaultCatalog
        {
            get
            {
                string catalog = null;

                var dsi = _connection.GetService(typeof(IVsDataSourceInformation)) as IVsDataSourceInformation;
                if (dsi != null)
                {
                    catalog = dsi["DefaultCatalog"] as string;
                }
                return catalog;
            }
        }

        #endregion
    }
}
