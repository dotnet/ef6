// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Utilities;

    internal class QueryCacheConfig
    {
        private const int DefaultSize = 1000;
        private const int DefaultCleaningIntervalInSeconds = 60 * 1000;

        private readonly EntityFrameworkSection _entityFrameworkSection;

        public QueryCacheConfig(EntityFrameworkSection entityFrameworkSection)
        {
            DebugCheck.NotNull(entityFrameworkSection);

            _entityFrameworkSection = entityFrameworkSection;
        }

        public int GetQueryCacheSize()
        {
            var size = _entityFrameworkSection.QueryCache
                .Size;

            return (size != default(Int32)) ? size : DefaultSize;
        }

        public int GetCleaningIntervalInSeconds()
        {
            var cleaningIntervalInSeconds = _entityFrameworkSection.QueryCache
                .CleaningIntervalInSeconds;

            return (cleaningIntervalInSeconds != default(Int32)) ? cleaningIntervalInSeconds : DefaultCleaningIntervalInSeconds;
        }
    }
}
