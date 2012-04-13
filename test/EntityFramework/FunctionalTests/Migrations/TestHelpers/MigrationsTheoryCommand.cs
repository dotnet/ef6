namespace System.Data.Entity.Migrations
{
    using Xunit.Sdk;

    class MigrationsTheoryCommand : ITestCommand
    {
        ITestCommand _innerCommand;
        DatabaseProvider _provider;
        ProgrammingLanguage _language;

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
                throw new InvalidOperationException(string.Format(
                    "Expected {0} to be derived from {1}", testClass.GetType().FullName, typeof(DbTestCase).FullName));
            }

            dbTestClass.Init(_provider, _language);

            return _innerCommand.Execute(testClass);
        }

        public string DisplayName
        {
            get { return string.Format("{0} - DatabaseProvider: {1}, ProgrammingLanguage: {2}", _innerCommand.DisplayName, _provider, _language); }
        }

        public bool ShouldCreateInstance
        {
            get { return _innerCommand.ShouldCreateInstance; }
        }

        public int Timeout
        {
            get { return _innerCommand.Timeout; }
        }

        public Xml.XmlNode ToStartXml()
        {
            return _innerCommand.ToStartXml();
        }
    }
}