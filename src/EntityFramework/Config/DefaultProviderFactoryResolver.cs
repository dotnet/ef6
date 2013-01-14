// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Data.Common;
    using System.Data.Entity.Resources;

    internal class DefaultProviderFactoryResolver : IDbDependencyResolver
    {
        public virtual object GetService(Type type, object key)
        {
            if (type == typeof(DbProviderFactory))
            {
                var name = key as string;

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(Strings.ProviderInvariantNotPassedToResolver);
                }

                try
                {
                    return DbProviderFactories.GetFactory(name);
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException(Strings.EntityClient_InvalidStoreProvider, e);
                }
            }

            return null;
        }
    }
}
