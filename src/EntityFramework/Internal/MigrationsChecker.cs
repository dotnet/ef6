// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Migrations.Utilities;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    internal class MigrationsChecker
    {
        private readonly Func<Type, MigrationsConfigurationFinder> _finder;

        public MigrationsChecker(Func<Type, MigrationsConfigurationFinder> finder = null)
        {
            _finder = finder ?? (t => new MigrationsConfigurationFinder(new TypeFinder(t.Assembly)));
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public virtual bool IsMigrationsConfigured(Type contextType, Func<bool> databaseExists)
        {
            DebugCheck.NotNull(contextType);

            try
            {
                if (_finder(contextType).FindMigrationsConfiguration(contextType, null) == null)
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
                Strings.DatabaseInitializationStrategy_MigrationsEnabled(contextType.Name));
        }
    }
}
