// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;

    public class FakeProviderServicesResolver : IDbDependencyResolver
    {
        public object GetService(Type type, object key)
        {
            if (type == typeof(DbProviderServices))
            {
                var name = key as string;
                if (name.StartsWith("My.Generic.Provider.", StringComparison.Ordinal))
                {
                    return GenericProviderServices.Instance;
                }
            }

            return null;
        }
    }
}
