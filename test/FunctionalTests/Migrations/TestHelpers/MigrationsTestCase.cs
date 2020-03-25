// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    [Serializable]
    internal class MigrationsTestCase : XunitTestCase
    {
        private DatabaseProvider _provider;
        private ProgrammingLanguage _language;

        public MigrationsTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, TestMethodDisplayOptions defaultMethodDisplayOptions, ITestMethod testMethod, DatabaseProvider provider, ProgrammingLanguage language)
            : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
        {
            _provider = provider;
            _language = language;
            DisplayName = string.Format(
                    "{0} - DatabaseProvider: {1}, ProgrammingLanguage: {2}", DisplayName, _provider, _language);
        }

        public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
            => new MigrationsTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, TestMethodArguments, messageBus, aggregator, cancellationTokenSource, _provider, _language).RunAsync();

        public override void Serialize(IXunitSerializationInfo data)
        {
            base.Serialize(data);

            data.AddValue("DatabaseProvider", _provider);
            data.AddValue("ProgrammingLanguage", _language);
        }

        public override void Deserialize(IXunitSerializationInfo data)
        {
            base.Deserialize(data);

            _provider = data.GetValue<DatabaseProvider>("DatabaseProvider");
            _language = data.GetValue<ProgrammingLanguage>("ProgrammingLanguage");
        }
    }

    public class MigrationsTestCaseRunner : XunitTestCaseRunner
    {
        private readonly DatabaseProvider _provider;
        private readonly ProgrammingLanguage _language;

        public MigrationsTestCaseRunner(IXunitTestCase testCase, string displayName, string skipReason, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, DatabaseProvider provider, ProgrammingLanguage language)
            : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource)
        {
            _provider = provider;
            _language = language;
        }

        protected override Task<RunSummary> RunTestAsync()
            => new MigrationsTestRunner(new XunitTest(TestCase, DisplayName), MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason, BeforeAfterAttributes, new ExceptionAggregator(Aggregator), CancellationTokenSource, _provider, _language).RunAsync();
    }

    public class MigrationsTestRunner : XunitTestRunner
    {
        private readonly DatabaseProvider _provider;
        private readonly ProgrammingLanguage _language;

        public MigrationsTestRunner(XunitTest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator exceptionAggregator, CancellationTokenSource cancellationTokenSource, DatabaseProvider provider, ProgrammingLanguage language)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, beforeAfterAttributes, exceptionAggregator, cancellationTokenSource)
        {
            _provider = provider;
            _language = language;
        }

        protected override Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
            => new MigrationsTestInvoker(Test, MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments, BeforeAfterAttributes, aggregator, CancellationTokenSource, _provider, _language).RunAsync();
    }

    public class MigrationsTestInvoker : XunitTestInvoker
    {
        private readonly DatabaseProvider _provider;
        private readonly ProgrammingLanguage _language;

        public MigrationsTestInvoker(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource, DatabaseProvider provider, ProgrammingLanguage language)
            : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterAttributes, aggregator, cancellationTokenSource)
        {
            _provider = provider;
            _language = language;
        }

        protected override object CreateTestClass()
        {
            var testClass = base.CreateTestClass();
            var dbTestClass = testClass as DbTestCase;

            if (dbTestClass == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Expected {0} to be derived from {1}", testClass.GetType().FullName, typeof(DbTestCase).FullName));
            }

            dbTestClass.Init(_provider, _language);

            return dbTestClass;
        }
    }
}
