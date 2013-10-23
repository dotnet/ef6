// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Data.Sql
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.VisualStudio.Data.Services.SupportEntities;
    using IVsDataParameter = Microsoft.VisualStudio.Data.Services.RelationalObjectModel.IVsDataParameter;

    internal class DataSchemaParameter : DataSchemaObject, IDataSchemaParameter
    {
        private readonly IVsDataParameter _parameter;
        private const int InvalidDirection = -1;

        private static readonly int[] DirectionMappingTable = new[]
            {
                (int)DataParameterDirection.In, (int)ParameterDirection.Input,
                (int)DataParameterDirection.InOut, (int)ParameterDirection.InputOutput,
                (int)DataParameterDirection.Out, (int)ParameterDirection.Output,
                (int)DataParameterDirection.ReturnValue, (int)ParameterDirection.ReturnValue
            };

        public DataSchemaParameter(DataSchemaServer server, IVsDataParameter parameter)
            : base(server, parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException("parameter");
            }

            _parameter = parameter;
        }

        #region IDataSchemaParameter Members

        public Type UrtType
        {
            get { return _parameter.FrameworkDataType; }
        }

        public DbType DbType
        {
            get { return (DbType)_parameter.AdoDotNetDbType; }
        }

        public ParameterDirection Direction
        {
            get
            {
                var result = ParameterDirection.Input;

                var retval = Map(
                    DirectionMappingTable,
                    (int)_parameter.Direction,
                    InvalidDirection);

                Debug.Assert(
                    retval != InvalidDirection, "Unknown ParameterDirection value: " +
                                                Enum.Format(typeof(DataParameterDirection), _parameter.Direction, "g"));

                if (retval != InvalidDirection)
                {
                    result = (ParameterDirection)retval;
                }

                return result;
            }
        }

        public int Size
        {
            get { return _parameter.Length; }
        }

        public int Precision
        {
            get { return _parameter.Precision; }
        }

        public int Scale
        {
            get { return _parameter.Scale; }
        }

        public int ProviderDataType
        {
            get { return _parameter.AdoDotNetDataType; }
        }

        public string NativeDataType
        {
            get { return _parameter.NativeDataType; }
        }

        #endregion

        #region Private Members

        private static int Map(int[] mapTable, int from, int defaultTarget)
        {
            Debug.Assert((mapTable.Length % 2) == 0, "mapTable.Length (" + mapTable.Length + ") should be even");

            var mappedValue = defaultTarget;
            for (var index = 0; index < mapTable.Length; index = index + 2)
            {
                if (mapTable[index] == from)
                {
                    mappedValue = mapTable[index + 1];
                    break;
                }
            }

            return mappedValue;
        }

        #endregion
    }
}
