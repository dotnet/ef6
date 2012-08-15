// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(IDbCommandInterceptorContracts))]
    internal interface IDbCommandInterceptor
    {
        bool IsEnabled { get; set; }
        bool Intercept(DbCommand command);

        IEnumerable<InterceptedCommand> Commands { get; }
    }

    #region Interface Member Contracts

    [ContractClassFor(typeof(IDbCommandInterceptor))]
    internal abstract class IDbCommandInterceptorContracts : IDbCommandInterceptor
    {
        public bool IsEnabled { get; set; }

        public bool Intercept(DbCommand command)
        {
            Contract.Requires(command != null);

            return false;
        }

        public IEnumerable<InterceptedCommand> Commands { get; private set; }
    }

    #endregion
}
