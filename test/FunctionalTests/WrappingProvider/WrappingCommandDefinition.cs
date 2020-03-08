// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Data.Common;
    using System.Data.Entity.Core.Common;

    public class WrappingCommandDefinition<TBase> : DbCommandDefinition where TBase : DbProviderFactory
    {
        private readonly DbCommandDefinition _baseCommandDefinition;

        public WrappingCommandDefinition(DbCommandDefinition baseCommandDefinition)
        {
            _baseCommandDefinition = baseCommandDefinition;
        }

        public override DbCommand CreateCommand()
        {
            return new WrappingCommand<TBase>(_baseCommandDefinition.CreateCommand());
        }
    }
}
