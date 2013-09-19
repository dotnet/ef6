// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using Xunit.Sdk;

    internal class MigrationsTheoryCommand : FactCommand
    {
        private readonly DatabaseProvider _provider;
        private readonly ProgrammingLanguage _language;

        public MigrationsTheoryCommand(IMethodInfo method, DatabaseProvider provider, ProgrammingLanguage language)
            :base(method)
        {
            _provider = provider;
            _language = language;
            DisplayName = string.Format(
                    "{0} - DatabaseProvider: {1}, ProgrammingLanguage: {2}", DisplayName, _provider, _language);
        }

        public override MethodResult Execute(object testClass)
        {
            var dbTestClass = testClass as DbTestCase;

            if (dbTestClass == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Expected {0} to be derived from {1}", testClass.GetType().FullName, typeof(DbTestCase).FullName));
            }

            dbTestClass.Init(_provider, _language);

            return base.Execute(testClass);
        }
    }
}
