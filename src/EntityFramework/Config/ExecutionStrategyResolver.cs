// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;

    internal class ExecutionStrategyResolver : IDbDependencyResolver
    {
        public object GetService(Type type, object key)
        {
            if (type == typeof(IExecutionStrategy))
            {
                var executionStrategyKey = key as ExecutionStrategyKey;
                if (executionStrategyKey == null)
                {
                    return null;
                }

                var providerServices = DbConfiguration.GetService<DbProviderServices>(
                    executionStrategyKey.InvariantProviderName);

                Debug.Assert(providerServices != null);

                return providerServices.GetExecutionStrategy();
            }

            return null;
        }
    }
}
