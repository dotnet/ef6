// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [XunitTestCaseDiscoverer("System.Data.Entity.Migrations.MigrationsTheoryDiscoverer", "EntityFramework.FunctionalTests")]
    public class MigrationsTheoryAttribute : ExtendedFactAttribute
    {
    }

    public class MigrationsTheoryDiscoverer : ExtendedFactDiscoverer
    {
        public MigrationsTheoryDiscoverer(IMessageSink diagnosticMessageSink)
            : base(diagnosticMessageSink)
        {
        }

        public override IEnumerable<IXunitTestCase> Discover(
            ITestFrameworkDiscoveryOptions discoveryOptions,
            ITestMethod testMethod,
            IAttributeInfo factAttribute)
        {
            return ShouldRun(factAttribute, factAttribute.GetNamedArgument<TestGroup>(nameof(ExtendedFactAttribute.SlowGroup)))
                       ? from providerLanguageCombination in GetCombinations(testMethod.Method)
                         where ShouldRun(factAttribute, providerLanguageCombination.Slow ? TestGroup.MigrationsTests : TestGroup.Default)
                         select new MigrationsTestCase(
                             DiagnosticMessageSink,
                             discoveryOptions.MethodDisplayOrDefault(),
                             discoveryOptions.MethodDisplayOptionsOrDefault(),
                             testMethod,
                             providerLanguageCombination.DatabaseProvider,
                             providerLanguageCombination.ProgrammingLanguage)
                       : Enumerable.Empty<IXunitTestCase>();
        }

        private static IEnumerable<VariantAttribute> GetCombinations(IMethodInfo method)
        {
            var methodVariants
                = method
                    .GetCustomAttributes(typeof(VariantAttribute))
                    .Select(a => new VariantAttribute((DatabaseProvider)a.GetConstructorArguments().First(), (ProgrammingLanguage)a.GetConstructorArguments().Skip(1).First()) { Slow = a.GetNamedArgument<bool>(nameof(VariantAttribute.Slow)) })
                    .ToList();

            if (methodVariants.Any())
            {
                return methodVariants;
            }

            var typeVariants
                = method.Type
                    .GetCustomAttributes(typeof(VariantAttribute))
                    .Select(a => new VariantAttribute((DatabaseProvider)a.GetConstructorArguments().First(), (ProgrammingLanguage)a.GetConstructorArguments().Skip(1).First()) { Slow = a.GetNamedArgument<bool>(nameof(VariantAttribute.Slow)) })
                    .ToList();

            if (typeVariants.Any())
            {
                return typeVariants;
            }

            return new[] { new VariantAttribute(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp) };
        }
    }
}
