// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics.Contracts;
    using System.Linq;

    public sealed class InterceptedCommand
    {
        private readonly string _commandText;
        private readonly DbParameter[] _parameters;

        public InterceptedCommand(DbCommand command)
        {
            Contract.Requires(command != null);

            _commandText = command.CommandText;
            _parameters = command.Parameters.Cast<DbParameter>().ToArray();
        }

        public string CommandText
        {
            get { return _commandText; }
        }

        public IEnumerable<DbParameter> Parameters
        {
            get { return _parameters; }
        }
    }
}
