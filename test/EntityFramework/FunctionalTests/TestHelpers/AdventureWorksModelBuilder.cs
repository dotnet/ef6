// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Reflection;

    public sealed class AdventureWorksModelBuilder : DbModelBuilder
    {
        internal DbDatabaseMapping BuildAndValidate(DbProviderInfo providerInfo, params Type[] unignoredTypes)
        {
            return BuildAndValidate(providerInfo, false, unignoredTypes);
        }

        internal void IgnoreAll(params Type[] unignoredTypes)
        {
            Ignore(
                Assembly.GetExecutingAssembly().GetTypes()
                    .Where(
                        t => !string.IsNullOrWhiteSpace(t.Namespace)
                             && t.Namespace.Contains("Model")).Except(
                                 Configurations.GetConfiguredTypes().Union(unignoredTypes)));
        }

        internal DbDatabaseMapping BuildAndValidate(DbProviderInfo providerInfo, bool throwOnError, params Type[] unignoredTypes)
        {
            IgnoreAll(unignoredTypes);

            // Build and clone multiple times to check for idempotency issues.

            Build(providerInfo);

            var cloned = Clone();

            var databaseMapping = cloned.Build(providerInfo).DatabaseMapping;

            databaseMapping.AssertValid(throwOnError);

            return databaseMapping;
        }
    }
}
