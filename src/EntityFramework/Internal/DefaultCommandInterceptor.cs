// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class DefaultCommandInterceptor : IDbCommandInterceptor
    {
        private List<InterceptedCommand> _commands;
        private bool _isEnabled;

        public IEnumerable<InterceptedCommand> Commands
        {
            get
            {
                return (_commands != null)
                           ? _commands.ToArray()
                           : Enumerable.Empty<InterceptedCommand>();
            }
        }

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (!_isEnabled && value)
                {
                    _commands = new List<InterceptedCommand>();
                }

                _isEnabled = value;
            }
        }

        public bool Intercept(DbCommand command)
        {
            DebugCheck.NotNull(command);

            if (!IsEnabled)
            {
                return true;
            }

            _commands.Add(new InterceptedCommand(command));

            // don't actually execute
            return false;
        }
    }
}
