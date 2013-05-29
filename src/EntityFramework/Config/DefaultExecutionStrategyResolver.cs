// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

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
                        Strings.DbDependencyResolver_InvalidKey(typeof(ExecutionStrategyKey).Name, "Func<IExecutionStrategy>"));
                }

                return (Func<IExecutionStrategy>)(() => new NonRetryingExecutionStrategy());
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            return this.GetServiceAsServices(type, key);
        }
    }
}
