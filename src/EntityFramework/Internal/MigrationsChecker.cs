// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    internal class MigrationsChecker
    {
        private readonly Func<InternalContext, bool> _finder;

        public MigrationsChecker(Func<InternalContext, bool> finder = null)
        {
            _finder = finder ?? (c => c.MigrationsConfigurationDiscovered);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual bool IsMigrationsConfigured(InternalContext internalContext, Func<bool> databaseExists)
        {
            DebugCheck.NotNull(internalContext);

            try
            {
                if (!_finder(internalContext))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.Fail("Exception ignored while attempting to create migration configuration: " + ex);
                return false;
            }

            if (databaseExists())
            {
                return true;
            }

            throw new InvalidOperationException(
                Strings.DatabaseInitializationStrategy_MigrationsEnabled(internalContext.Owner.GetType().Name));
        }
    }
}
