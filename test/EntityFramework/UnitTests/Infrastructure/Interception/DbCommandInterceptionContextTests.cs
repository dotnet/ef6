// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Interception
{
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Internal;
    using System.Data.Entity.Resources;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class DbCommandInterceptionContextTests : TestBase
    {
        [Fact]
        public void New_base_interception_context_has_no_state()
        {
            var interceptionContext = new DbCommandInterceptionContext();

            Assert.Empty(interceptionContext.ObjectContexts);
            Assert.Empty(interceptionContext.DbContexts);
            Assert.False(interceptionContext.IsAsync);
            Assert.Equal(CommandBehavior.Default, interceptionContext.CommandBehavior);
        }

        [Fact]
        public void Cloning_the_base_interception_context_preserves_contextual_information()
        {
            var objectContext = new ObjectContext();
            var dbContext = DbContextMockHelper.CreateDbContext(objectContext);

            var interceptionContext = new DbCommandInterceptionContext()
                .WithDbContext(dbContext)
                .WithObjectContext(objectContext)
                .WithCommandBehavior(CommandBehavior.SchemaOnly)
                .AsAsync();

            Assert.Equal(new[] { objectContext }, interceptionContext.ObjectContexts);
            Assert.Equal(new[] { dbContext }, interceptionContext.DbContexts);
            Assert.True(interceptionContext.IsAsync);
            Assert.Equal(CommandBehavior.SchemaOnly, interceptionContext.CommandBehavior);
        }

        [Fact]
        public void Base_interception_context_constructor_throws_on_null()
        {
            Assert.Equal(
                "copyFrom",
                Assert.Throws<ArgumentNullException>(() => new DbCommandInterceptionContext(null)).ParamName);
        }

        [Fact]
        public void Association_the_base_with_a_null_ObjectContext_or_DbContext_throws()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbCommandInterceptionContext().WithObjectContext(null)).ParamName);

            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbCommandInterceptionContext().WithDbContext(null)).ParamName);
        }

        [Fact]
        public void New_interception_context_has_no_state()
        {
            var interceptionContext = new DbCommandInterceptionContext<int>();

            Assert.Empty(interceptionContext.ObjectContexts);
            Assert.Empty(interceptionContext.DbContexts);
            Assert.Null(interceptionContext.Exception);
            Assert.Null(interceptionContext.OriginalException);
            Assert.False(interceptionContext.IsAsync);
            Assert.Equal((TaskStatus)0, interceptionContext.TaskStatus);
            Assert.Equal(CommandBehavior.Default, interceptionContext.CommandBehavior);
            Assert.Equal(0, interceptionContext.Result);
            Assert.Equal(0, interceptionContext.OriginalResult);
            Assert.False(interceptionContext.IsExecutionSuppressed);
        }

        [Fact]
        public void Cloning_the_interception_context_preserves_contextual_information_but_not_mutable_state()
        {
            var objectContext = new ObjectContext();
            var dbContext = DbContextMockHelper.CreateDbContext(objectContext);

            var interceptionContext = new DbCommandInterceptionContext<string>();
            
            var mutableData = ((IDbMutableInterceptionContext<string>)interceptionContext).MutableData;
            mutableData.SetExecuted("Wensleydale");
            mutableData.SetExceptionThrown(new Exception("Cheez Whiz"));
            mutableData.UserState = new object();

            interceptionContext = interceptionContext
                .WithDbContext(dbContext)
                .WithObjectContext(objectContext)
                .AsAsync()
                .WithCommandBehavior(CommandBehavior.SchemaOnly);

            Assert.Equal(new[] { objectContext }, interceptionContext.ObjectContexts);
            Assert.Equal(new[] { dbContext }, interceptionContext.DbContexts);
            Assert.True(interceptionContext.IsAsync);
            Assert.Equal(CommandBehavior.SchemaOnly, interceptionContext.CommandBehavior);

            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.OriginalResult);
            Assert.Null(interceptionContext.Exception);
            Assert.Null(interceptionContext.OriginalException);
            Assert.Null(interceptionContext.UserState);
            Assert.False(interceptionContext.IsExecutionSuppressed);
        }

        [Fact]
        public void Interception_context_constructor_throws_on_null()
        {
            Assert.Equal(
                "copyFrom",
                Assert.Throws<ArgumentNullException>(() => new DbCommandInterceptionContext<int>(null)).ParamName);
        }

        [Fact]
        public void Result_can_be_mutated()
        {
            var interceptionContext = new DbCommandInterceptionContext<string>();

            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.OriginalResult);
            Assert.Null(interceptionContext.Exception);
            Assert.Null(interceptionContext.OriginalException);
            Assert.Null(interceptionContext.UserState);
            Assert.False(interceptionContext.IsExecutionSuppressed);

            ((IDbMutableInterceptionContext<string>)interceptionContext).MutableData.SetExecuted("Wensleydale");

            Assert.Equal("Wensleydale", interceptionContext.Result);
            Assert.Equal("Wensleydale", interceptionContext.OriginalResult);
            Assert.Null(interceptionContext.Exception);
            Assert.Null(interceptionContext.OriginalException);
            Assert.Null(interceptionContext.UserState);
            Assert.False(interceptionContext.IsExecutionSuppressed);

            interceptionContext.Result = "Double Gloucester";
            interceptionContext.UserState = "Cheddar";
            Assert.Equal("Double Gloucester", interceptionContext.Result);
            Assert.Equal("Wensleydale", interceptionContext.OriginalResult);
            Assert.Equal("Cheddar", interceptionContext.UserState);
            Assert.False(interceptionContext.IsExecutionSuppressed);

            interceptionContext.Result = null;
            interceptionContext.UserState = null;
            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.UserState);
            Assert.Equal("Wensleydale", interceptionContext.OriginalResult);
            Assert.False(interceptionContext.IsExecutionSuppressed);
        }

        [Fact]
        public void Exception_can_be_mutated()
        {
            var interceptionContext = new DbCommandInterceptionContext<string>();

            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.OriginalResult);
            Assert.Null(interceptionContext.Exception);
            Assert.Null(interceptionContext.OriginalException);
            Assert.False(interceptionContext.IsExecutionSuppressed);

            interceptionContext.MutableData.SetExceptionThrown(new Exception("Cheez Whiz"));

            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.OriginalResult);
            Assert.Equal("Cheez Whiz", interceptionContext.Exception.Message);
            Assert.Equal("Cheez Whiz", interceptionContext.OriginalException.Message);
            Assert.False(interceptionContext.IsExecutionSuppressed);

            interceptionContext.Exception = new Exception("Velveeta");
            Assert.Equal("Velveeta", interceptionContext.Exception.Message);
            Assert.Equal("Cheez Whiz", interceptionContext.OriginalException.Message);
            Assert.False(interceptionContext.IsExecutionSuppressed);

            interceptionContext.Exception = null;
            Assert.Null(interceptionContext.Exception);
            Assert.Equal("Cheez Whiz", interceptionContext.OriginalException.Message);
            Assert.False(interceptionContext.IsExecutionSuppressed);
        }

        [Fact]
        public void Suppression_can_be_flagged_by_setting_Result_before_execution()
        {
            var interceptionContext = new DbCommandInterceptionContext<string>();

            interceptionContext.Result = "Double Gloucester";

            Assert.True(interceptionContext.IsExecutionSuppressed);
            Assert.Equal("Double Gloucester", interceptionContext.Result);
        }

        [Fact]
        public void Suppression_can_be_flagged_by_setting_Exception_before_execution()
        {
            var interceptionContext = new DbCommandInterceptionContext<string>();

            interceptionContext.Exception = new Exception("Velveeta");

            Assert.True(interceptionContext.IsExecutionSuppressed);
            Assert.Equal("Velveeta", interceptionContext.Exception.Message);
        }

        [Fact]
        public void Suppression_can_be_flagged_by_calling_SuppressExecution()
        {
            var interceptionContext = new DbCommandInterceptionContext<string>();

            interceptionContext.SuppressExecution();

            Assert.True(interceptionContext.IsExecutionSuppressed);
            Assert.Null(interceptionContext.Result);
            Assert.Null(interceptionContext.Exception);
        }


        [Fact]
        public void Calling_SuppressExecution_after_execution_throws()
        {
            var interceptionContext = new DbCommandInterceptionContext<string>();

            ((IDbMutableInterceptionContext<string>)interceptionContext).MutableData.SetExecuted("Wensleydale");

            Assert.Equal(
                Strings.SuppressionAfterExecution,
                Assert.Throws<InvalidOperationException>(() => interceptionContext.SuppressExecution()).Message);
        }

        [Fact]
        public void Association_with_a_null_ObjectContext_or_DbContext_throws()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbCommandInterceptionContext<int>().WithObjectContext(null)).ParamName);

            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => new DbCommandInterceptionContext<int>().WithDbContext(null)).ParamName);
        }
    }
}
