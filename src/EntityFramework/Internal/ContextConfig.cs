// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class ContextConfig
    {
        private readonly EntityFrameworkSection _entityFrameworkSettings;

        private readonly ConcurrentDictionary<Type, int?> _commandTimeouts = new ConcurrentDictionary<Type, int?>();

        public ContextConfig()
        {
        }

        public ContextConfig(EntityFrameworkSection entityFrameworkSettings)
        {
            DebugCheck.NotNull(entityFrameworkSettings);

            _entityFrameworkSettings = entityFrameworkSettings;
        }

        public virtual int? TryGetCommandTimeout(Type contextType)
        {
            DebugCheck.NotNull(contextType);

            return _commandTimeouts.GetOrAdd(
                contextType,
                (requiredContextType) => _entityFrameworkSettings.Contexts
                    .OfType<ContextElement>()
                    .Where(e => e.CommandTimeout.HasValue)
                    .Select(e => TryGetCommandTimeout(contextType, e.ContextTypeName, e.CommandTimeout.Value))
                    .FirstOrDefault(i => i.HasValue));
        }

        private static int? TryGetCommandTimeout(
            Type requiredContextType,
            string contextTypeName,
            int commandTimeout)
        {
            DebugCheck.NotNull(requiredContextType);
            DebugCheck.NotNull(contextTypeName);

            try
            {
                if (Type.GetType(contextTypeName, throwOnError: true) == requiredContextType)
                {
                    return commandTimeout;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(Strings.Database_InitializationException, ex);
            }

            return null;
        }
    }
}
