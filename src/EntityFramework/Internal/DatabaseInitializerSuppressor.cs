// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Concurrent;
    using System.Data.Entity.Utilities;

    internal class DatabaseInitializerSuppressor
    {
        public static readonly DatabaseInitializerSuppressor Instance = new DatabaseInitializerSuppressor();

        // Using a Dictionary here since there is no concurrent set.
        private readonly ConcurrentDictionary<Type, bool> _suppressedInitializers = new ConcurrentDictionary<Type, bool>();

        public void Suppress(Type contextType)
        {
            DebugCheck.NotNull(contextType);

            _suppressedInitializers.TryAdd(contextType, true);
        }

        public void Unsuppress(Type contextType)
        {
            DebugCheck.NotNull(contextType);

            bool _;
            _suppressedInitializers.TryRemove(contextType, out _);
        }

        public bool IsSuppressed(Type contextType)
        {
            DebugCheck.NotNull(contextType);

            bool _;
            return _suppressedInitializers.TryGetValue(contextType, out _);
        }
    }
}
