// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using EdmxModel = Microsoft.Data.Entity.Design.Model;

namespace Microsoft.Data.Entity.Design.VisualStudio.Data.Sql
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.VisualStudio.Data.Services.RelationalObjectModel;

    internal class DataSchemaProcedure : DataSchemaObject, IDataSchemaProcedure
    {
        private readonly IVsDataExecutableObject _executableObject;
        private IList<IDataSchemaColumn> _columns;
        private IList<IDataSchemaParameter> _parameters;

        public DataSchemaProcedure(DataSchemaServer server, IVsDataExecutableObject executableObject)
            : base(server, executableObject)
        {
            _executableObject = executableObject;
        }

        #region IDataSchemaProcedure Members

        public IList<IDataSchemaParameter> Parameters
        {
            get
            {
                if (_parameters == null)
                {
                    _parameters = new List<IDataSchemaParameter>();
                    foreach (var parameter in _executableObject.Parameters)
                    {
                        _parameters.Add(new DataSchemaParameter(Server, parameter));
                    }
                }

                return _parameters;
            }
        }

        /// <summary>
        ///     Used to determine the shape of the resultset of non-Functions
        /// </summary>
        public IList<IDataSchemaColumn> Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new List<IDataSchemaColumn>();

                    var tabularData = _executableObject as IVsDataTabularObject;
                    if (tabularData != null)
                    {
                        foreach (var column in tabularData.Columns)
                        {
                            _columns.Add(new DataSchemaColumn(Server, column));
                        }
                    }
                }

                return _columns;
            }
        }

        /// <summary>
        ///     Used to determine the return type of Functions
        /// </summary>
        public IDataSchemaParameter ReturnValue
        {
            get
            {
                IDataSchemaParameter result = null;
                var function = _executableObject as IVsDataScalarFunction;
                if (function != null)
                {
                    result = new DataSchemaParameter(Server, function.ReturnValue);
                }

                return result;
            }
        }

        public bool HasRows
        {
            get
            {
                bool hasRows;
                if (_executableObject is IVsDataStoredProcedure)
                {
                    hasRows = (Columns.Count > 0);
                }
                else
                {
                    hasRows = (_executableObject is IVsDataTabularFunction);
                }
                return hasRows;
            }
        }

        public bool IsFunction
        {
            get { return (_executableObject is IVsDataScalarFunction || _executableObject is IVsDataTabularFunction); }
        }

        public string Schema
        {
            get { return _executableObject.Schema; }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, EdmxModel.Resources.DatabaseObjectNameFormat, Schema, Name);
        }

        #endregion

        public IList<IRawDataSchemaParameter> RawParameters
        {
            get { return Parameters.OfType<IRawDataSchemaParameter>().ToList(); }
        }

        /// <summary>
        ///     Used to determine the shape of the resultset of non-Functions
        /// </summary>
        public IList<IRawDataSchemaColumn> RawColumns
        {
            get { return Columns.OfType<IRawDataSchemaColumn>().ToList(); }
        }

        /// <summary>
        ///     Used to determine the return type of Functions
        /// </summary>
        public IRawDataSchemaParameter RawReturnValue
        {
            get { return ReturnValue; }
        }
    }
}
