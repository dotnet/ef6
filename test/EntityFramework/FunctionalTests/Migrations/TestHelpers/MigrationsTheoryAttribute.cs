// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit.Extensions;
    using Xunit.Sdk;

    public class MigrationsTheoryAttribute : TheoryAttribute
    {
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            var _providerLanguageCominations = GetCombinations(method.MethodInfo);

            var testCommands
                = method.MethodInfo.GetParameters().Length == 0
                      ? new[] { new FactCommand(method) }
                      : base.EnumerateTestCommands(method);

            return (from providerLanguageCombination in _providerLanguageCominations
                    from testCommand in testCommands
                    select new MigrationsTheoryCommand(
                        testCommand,
                        providerLanguageCombination.DatabaseProvider,
                        providerLanguageCombination.ProgrammingLanguage));
        }

        private static IEnumerable<VariantAttribute> GetCombinations(MethodInfo method)
        {
            var methodVariants
                = method
                    .GetCustomAttributes(typeof(VariantAttribute), true)
                    .Cast<VariantAttribute>()
                    .ToList();

            if (methodVariants.Any())
            {
                return methodVariants;
            }

            var typeVariants
                = method.DeclaringType
                    .GetCustomAttributes(typeof(VariantAttribute), true)
                    .Cast<VariantAttribute>()
                    .ToList();

            if (typeVariants.Any())
            {
                return typeVariants;
            }

            return new[] { new VariantAttribute(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp) };
        }
    }
}
