// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Utilities
{
    using System.Data.Common;
    using System.Data.Entity.Config;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    internal static class DbProviderFactoryExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static string GetProviderInvariantName(this DbProviderFactory factory)
        {
            Contract.Requires(factory != null);

            const int invariantNameIndex = 2;

            var row = new ProviderRowFinder().FindRow(
                factory.GetType(),
                r => DbProviderFactories.GetFactory(r).GetType() == factory.GetType());

            if (row == null)
            {
                throw new NotSupportedException(Strings.ProviderNameNotFound(factory));
            }

            return (string)row[invariantNameIndex];
        }

        internal static DbProviderServices GetProviderServices(this DbProviderFactory factory)
        {
            Contract.Requires(factory != null);

            // The EntityClient provider invariant name is not normally registered so we can't use
            // the normal method for looking up this factory.
            if (factory is EntityProviderFactory)
            {
                return EntityProviderServices.Instance;
            }

            var invariantName = factory.GetProviderInvariantName();
            Contract.Assert(invariantName != null);

            return DbConfiguration.GetService<DbProviderServices>(invariantName);
        }
    }
}
