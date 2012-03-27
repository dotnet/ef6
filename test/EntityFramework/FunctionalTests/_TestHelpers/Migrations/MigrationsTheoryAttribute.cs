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
            IEnumerable<VariantAttribute> _providerLanguageCominations = GetCombinations(method.MethodInfo);

            IEnumerable<ITestCommand> testCommands;
            if (method.MethodInfo.GetParameters().Length == 0)
            {
                testCommands = new[] { new FactCommand(method) };
            }
            else
            {
                testCommands = base.EnumerateTestCommands(method);
            }

            foreach (var providerLanguageComination in _providerLanguageCominations)
            {
                foreach (var testCommand in testCommands)
                {
                    yield return new MigrationsTheoryCommand(testCommand, providerLanguageComination.DatabaseProvider, providerLanguageComination.ProgrammingLanguage);
                }
            }
        }

        static IEnumerable<VariantAttribute> GetCombinations(MethodInfo method)
        {
            var variants = method.GetCustomAttributes(typeof(VariantAttribute), true).Cast<VariantAttribute>();
            if (variants.Count() > 0)
            {
                return variants;
            }

            variants = method.DeclaringType.GetCustomAttributes(typeof(VariantAttribute), true).Cast<VariantAttribute>();
            if (variants.Count() > 0)
            {
                return variants;
            }

            return new[] { new VariantAttribute(DatabaseProvider.SqlClient, ProgrammingLanguage.CSharp) };
        }
    }
}