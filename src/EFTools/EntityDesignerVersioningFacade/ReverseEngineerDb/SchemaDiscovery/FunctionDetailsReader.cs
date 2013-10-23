// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Diagnostics;

    internal class FunctionDetailsReader : IDisposable
    {
        private readonly DbDataReader _reader;
        private readonly EntityCommand _command;
        private readonly Func<object[], FunctionDetailsRowView> _rowViewFactoryMethod;
        private FunctionDetailsRowView _currentRow;

        public FunctionDetailsReader(EntityCommand command, Version storeSchemaModelVersion)
        {
            Debug.Assert(command != null, "command != null");
            Debug.Assert(storeSchemaModelVersion != null, "storeSchemaModelVersion != null");

            _rowViewFactoryMethod =
                storeSchemaModelVersion < EntityFrameworkVersion.Version3
                    ? (values) => new FunctionDetailsV1RowView(values)
                    : (Func<object[], FunctionDetailsRowView>)((values) => new FunctionDetailsV3RowView(values));

            _command = command;
            _reader = _command.ExecuteReader(CommandBehavior.SequentialAccess);
        }

        public bool Read()
        {
            var haveRow = _reader.Read();
            if (haveRow)
            {
                var values = new object[_reader.FieldCount];
                _reader.GetValues(values);
                _currentRow = _rowViewFactoryMethod(values);
            }
            else
            {
                _currentRow = null;
            }
            return haveRow;
        }

        public FunctionDetailsRowView CurrentRow
        {
            get { return _currentRow; }
        }

        public void Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);
            Debug.Assert(_reader != null, "_reader != null");
            _reader.Dispose();
        }
    }
}
