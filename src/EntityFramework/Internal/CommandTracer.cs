// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Config;
    using System.Linq;

    internal class CommandTracer : IDisposable
    {
        private IDbCommandInterceptor _commandInterceptor;

        public CommandTracer(IDbCommandInterceptor commandInterceptor = null)
        {
            _commandInterceptor
                = commandInterceptor
                  ?? DbConfiguration.GetService<IDbCommandInterceptor>();

            if (_commandInterceptor != null)
            {
                _commandInterceptor.IsEnabled = true;
            }
        }

        public IEnumerable<InterceptedCommand> Commands
        {
            get
            {
                return (_commandInterceptor != null)
                           ? _commandInterceptor.Commands
                           : Enumerable.Empty<InterceptedCommand>();
            }
        }

        public void Dispose()
        {
            if (_commandInterceptor != null)
            {
                _commandInterceptor.IsEnabled = false;
                _commandInterceptor = null;
            }
        }
    }
}
