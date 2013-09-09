// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Data.Entity.Functionals.Utilities;
    using System.Linq;
    using System.Reflection;
    using Xunit.Sdk;

    public class MigrationsTheoryAttribute : ExtendedFactAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            return ShouldRun(SlowGroup)
                       ? from providerLanguageCombination in GetCombinations(method.MethodInfo)
                         where ShouldRun(providerLanguageCombination.Slow ? TestGroup.MigrationsTests : TestGroup.Default)
                         select new MigrationsTheoryCommand(
                             method,
                             providerLanguageCombination.DatabaseProvider,
                             providerLanguageCombination.ProgrammingLanguage)
                       : Enumerable.Empty<ITestCommand>();
        }

        private static IEnumerable<VariantAttribute> GetCombinations(MethodInfo method)
        {
            var methodVariants
                = method
                    .GetCustomAttributes<VariantAttribute>(inherit: true)
                    .ToList();

            if (methodVariants.Any())
            {
                return methodVariants;
            }

            var typeVariants
                = method.DeclaringType
                    .GetCustomAttributes<VariantAttribute>(inherit: true)
                    .ToList();

            if (typeVariants.Any())
            {
                return typeVariants;
            }

            return new[] { new VariantAttribute(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp) };
        }
    }
}
