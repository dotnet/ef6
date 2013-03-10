// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Utilities
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;

    internal class MigrationsConfigurationFinder
    {
        private readonly TypeFinder _typeFinder;

        public MigrationsConfigurationFinder(TypeFinder typeFinder)
        {
            DebugCheck.NotNull(typeFinder);

            _typeFinder = typeFinder;
        }

        public DbMigrationsConfiguration FindMigrationsConfiguration(
            Type contextType,
            string configurationTypeName,
            Func<string, Exception> noType = null,
            Func<string, IEnumerable<Type>, Exception> multipleTypes = null,
            Func<string, string, Exception> noTypeWithName = null,
            Func<string, string, Exception> multipleTypesWithName = null)
        {
            var configurationType = _typeFinder.FindType(
                contextType == null ? typeof(DbMigrationsConfiguration) : typeof(DbMigrationsConfiguration<>).MakeGenericType(contextType),
                configurationTypeName,
                types => types
                             .Where(
                                 t => t.GetConstructor(Type.EmptyTypes) != null
                                      && !t.IsAbstract
                                      && !t.IsGenericType)
                             .ToList(),
                noType,
                multipleTypes,
                noTypeWithName,
                multipleTypesWithName);

            return configurationType == null
                       ? null
                       : configurationType.CreateInstance<DbMigrationsConfiguration>(
                           Strings.CreateInstance_BadMigrationsConfigurationType,
                           s => new MigrationsException(s));
        }
    }
}
