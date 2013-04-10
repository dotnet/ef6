// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Internal.ConfigFile;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class ProviderConfig
    {
        private readonly EntityFrameworkSection _entityFrameworkSettings;

        public ProviderConfig()
        {
        }

        public ProviderConfig(EntityFrameworkSection entityFrameworkSettings)
        {
            DebugCheck.NotNull(entityFrameworkSettings);

            _entityFrameworkSettings = entityFrameworkSettings;
        }

        public virtual IEnumerable<ProviderElement> GetAllDbProviderServices()
        {
            return _entityFrameworkSettings.Providers.OfType<ProviderElement>();
        }
    }
}
