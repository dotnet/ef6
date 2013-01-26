// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Xml;
    using Xunit.Sdk;

    internal class MigrationsTheoryCommand : ITestCommand
    {
        private readonly ITestCommand _innerCommand;
        private readonly DatabaseProvider _provider;
        private readonly ProgrammingLanguage _language;

        public MigrationsTheoryCommand(ITestCommand innerCommand, DatabaseProvider provider, ProgrammingLanguage language)
        {
            _innerCommand = innerCommand;
            _provider = provider;
            _language = language;
        }

        public MethodResult Execute(object testClass)
        {
            var dbTestClass = testClass as DbTestCase;

            if (dbTestClass == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Expected {0} to be derived from {1}", testClass.GetType().FullName, typeof(DbTestCase).FullName));
            }

            dbTestClass.Init(_provider, _language);

            return _innerCommand.Execute(testClass);
        }

        public string DisplayName
        {
            get
            {
                return string.Format(
                    "{0} - DatabaseProvider: {1}, ProgrammingLanguage: {2}", _innerCommand.DisplayName, _provider, _language);
            }
        }

        public bool ShouldCreateInstance
        {
            get { return _innerCommand.ShouldCreateInstance; }
        }

        public int Timeout
        {
            get { return _innerCommand.Timeout; }
        }

        public XmlNode ToStartXml()
        {
            return _innerCommand.ToStartXml();
        }
    }
}
