// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    internal class DefaultExecutionStrategyResolver : IDbDependencyResolver
    {
        public object GetService(Type type, object key)
        {
            if (type == typeof(Func<IExecutionStrategy>))
            {
                Check.NotNull(key, "key");

                var executionStrategyKey = key as ExecutionStrategyKey;
                if (executionStrategyKey == null)
                {
                    throw new ArgumentException(
                        Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, typeof(IExecutionStrategy)));
                }

                var providerServices = DbConfiguration.GetService<DbProviderServices>(
                    executionStrategyKey.ProviderInvariantName);

                Debug.Assert(providerServices != null);

                return providerServices.GetExecutionStrategyFactory();
            }

            return null;
        }
    }
}
