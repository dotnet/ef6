// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;

    public class InternalDispatcherTests
    {
        public class AddRemove : TestBase
        {
            [Fact]
            public void Interceptors_for_only_the_matching_interface_type_can_be_added_and_removed()
            {
                var mockInterceptor1 = new Mock<FakeInterceptor1>();
                var mockInterceptor2 = new Mock<FakeInterceptor2>();

                var dispatcher = new InternalDispatcher<FakeInterceptor1>();

                dispatcher.Add(mockInterceptor1.Object);
                dispatcher.Add(mockInterceptor2.Object);

                dispatcher.Dispatch(i => i.CallMe(new DbInterceptionContext()));

                mockInterceptor1.Verify(m => m.CallMe(It.IsAny<DbInterceptionContext>()), Times.Once());
                mockInterceptor2.Verify(m => m.CallMe(It.IsAny<DbInterceptionContext>()), Times.Never());

                dispatcher.Remove(mockInterceptor1.Object);
                dispatcher.Remove(mockInterceptor2.Object);

                dispatcher.Dispatch(i => i.CallMe(new DbInterceptionContext()));

                mockInterceptor1.Verify(m => m.CallMe(It.IsAny<DbInterceptionContext>()), Times.Once());
                mockInterceptor2.Verify(m => m.CallMe(It.IsAny<DbInterceptionContext>()), Times.Never());
            }

            [Fact]
            public void Removing_an_interceptor_that_is_not_registered_is_a_no_op()
            {
                new InternalDispatcher<FakeInterceptor1>().Remove(new Mock<FakeInterceptor1>().Object);
            }

            [Fact]
            public void Interceptors_can_be_added_removed_and_dispatched_to_concurrently()
            {
                var interceptors = new ConcurrentStack<InterceptorForThreads>();
                var dispatcher = new InternalDispatcher<InterceptorForThreads>();

                const int interceptorCount = 20;
                const int dispatchCount = 10;

                // Add in parallel
                ExecuteInParallel(
                    () =>
                        {
                            var interceptor = new InterceptorForThreads();
                            interceptors.Push(interceptor);
                            dispatcher.Add(interceptor);
                        }, interceptorCount);

                Assert.Equal(interceptorCount, interceptors.Count);

                // Dispatch in parallel
                var calledInterceptors = new ConcurrentStack<InterceptorForThreads>();
                ExecuteInParallel(() => dispatcher.Dispatch(calledInterceptors.Push), dispatchCount);

                Assert.Equal(dispatchCount * interceptorCount, calledInterceptors.Count);
                interceptors.Each(i => Assert.Equal(dispatchCount, calledInterceptors.Count(c => c == i)));

                var toRemove = new ConcurrentStack<InterceptorForThreads>(interceptors);

                // Add, remove, and dispatch in parallel
                ExecuteInParallel(
                    () =>
                        {
                            dispatcher.Dispatch(i => { });
                            InterceptorForThreads interceptor;
                            toRemove.TryPop(out interceptor);
                            dispatcher.Remove(interceptor);
                            dispatcher.Add(interceptor);
                        }, interceptorCount);

                // Dispatch in parallel
                calledInterceptors = new ConcurrentStack<InterceptorForThreads>();
                ExecuteInParallel(() => dispatcher.Dispatch(calledInterceptors.Push), dispatchCount);

                Assert.Equal(dispatchCount * interceptorCount, calledInterceptors.Count);
                interceptors.Each(i => Assert.Equal(dispatchCount, calledInterceptors.Count(c => c == i)));
            }

            // Can't use Moq in multi-threaded test
            public class InterceptorForThreads : IDbInterceptor
            {
            }
        }

        public class Dispatch : TestBase
        {
            [Fact]
            public void Simple_Dispatch_dispatches_to_all_registered_interceptors()
            {
                var mockInterceptors = CreateMockInterceptors();
                var dispatcher = new InternalDispatcher<FakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                dispatcher.Dispatch(i => i.CallMe(new DbInterceptionContext()));

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbInterceptionContext>()), Times.Once()));
            }

            [Fact]
            public void Result_Dispatch_dispatches_to_all_registered_interceptors_and_aggregates_result()
            {
                var mockInterceptors = CreateMockInterceptors();
                var dispatcher = new InternalDispatcher<FakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Equal("0123", dispatcher.Dispatch("0", (r, i) => r + i.CallMe(new DbInterceptionContext())));

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbInterceptionContext>()), Times.Once()));
            }

            [Fact]
            public void Result_Dispatch_returns_result_if_no_dispatchers_registered()
            {
                Assert.Equal(
                    "0", new InternalDispatcher<FakeInterceptor1>().Dispatch("0", (r, i) => r + i.CallMe(new DbInterceptionContext())));
            }

            [Fact]
            public void Operation_Dispatch_dispatches_to_all_registered_interceptors_and_aggregates_results_of_operations()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors();
                var dispatcher = new InternalDispatcher<FakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Equal(
                    "0123",
                    dispatcher.Dispatch(
                        () => "0",
                        interceptionContext,
                        i => i.CallMeFirst(interceptionContext),
                        (i, c) => c.Result = c.Result + i.CallMe(c)));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.Is<DbInterceptionContext>(c => c.Exception == null)), Times.Once()));
            }

            [Fact]
            public void Execution_of_operation_can_be_canceled_with_everything_else_still_happening()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors();
                mockInterceptors.First()
                    .Setup(m => m.CallMeFirst(It.IsAny<DbCommandInterceptionContext<string>>()))
                    .Callback<DbInterceptionContext>(c => ((DbCommandInterceptionContext<string>)c).Result = "N");

                var dispatcher = new InternalDispatcher<FakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Equal(
                    "N123",
                    dispatcher.Dispatch<DbCommandInterceptionContext<string>, string>(
                        () => { throw new Exception("Bang!"); },
                        interceptionContext,
                        i => i.CallMeFirst(interceptionContext),
                        (i, c) => c.Result = c.Result + i.CallMe(c)));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.Is<DbInterceptionContext>(c => c.Exception == null)), Times.Once()));
            }

            [Fact]
            public void Operation_Dispatch_executes_operation_and_returns_result_if_no_dispatchers_registered()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                Assert.Equal(
                    "0",
                    new InternalDispatcher<FakeInterceptor1>().Dispatch(
                        () => "0", interceptionContext, i => i.CallMeFirst(interceptionContext), (i, c) => c.Result = c.Result + i.CallMe(c)));
            }

            [Fact]
            public void Operation_Dispatch_dispatches_to_all_registered_interceptors_even_if_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors();
                var dispatcher = new InternalDispatcher<FakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var exception = Assert.Throws<Exception>(
                    () => dispatcher.Dispatch<DbCommandInterceptionContext<string>, string>(
                        () => { throw new Exception("Bang!"); },
                        interceptionContext,
                        i => i.CallMeFirst(interceptionContext),
                        (i, c) => i.CallMe(c)));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(
                    i => i.Verify(m => m.CallMe(It.Is<DbInterceptionContext>(c => c.Exception == exception)), Times.Once()));
            }

#if !NET40
            [Fact]
            public void Async_Dispatch_dispatches_to_all_registered_interceptors_and_aggregates_results_of_operations()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors();
                mockInterceptors.First()
                    .Setup(m => m.CallMeFirst(It.IsAny<DbCommandInterceptionContext<string>>()))
                    .Callback<DbInterceptionContext>(c => ((DbCommandInterceptionContext<string>)c).Result = "N");

                var dispatcher = new InternalDispatcher<FakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var operation = new Task<string>(() => { throw new Exception("Bang!"); });
                var modifiedContext = new DbCommandInterceptionContext<string>();

                var interceptTask = dispatcher.Dispatch(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    (i, c) => c.Result = c.Result + i.CallMe(c),
                    (c, t) => modifiedContext);

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(modifiedContext), Times.Once()));

                interceptTask.Wait();
                Assert.Equal("N123", interceptTask.Result);
            }

            [Fact]
            public void Execution_of_async_operation_can_be_canceled_with_everything_else_still_happening()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors();
                var dispatcher = new InternalDispatcher<FakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var operation = new Task<string>(() => "0");
                var modifiedContext = new DbCommandInterceptionContext<string>();

                var interceptTask = dispatcher.Dispatch(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    (i, c) => c.Result = c.Result + i.CallMe(c),
                    (c, t) => modifiedContext);

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext>()), Times.Never()));

                operation.Start();
                interceptTask.Wait();

                Assert.Equal("0", operation.Result);
                Assert.Equal("0123", interceptTask.Result);

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(modifiedContext), Times.Once()));
            }

            [Fact]
            public void Async_Dispatch_executes_operation_and_returns_result_if_no_dispatchers_registered()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var dispatcher = new InternalDispatcher<FakeInterceptor1>();
                var operation = new Task<string>(() => "0");

                var interceptTask = dispatcher.Dispatch(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    (i, c) => c.Result = c.Result + i.CallMe(c),
                    (c, t) => interceptionContext);

                operation.Start();
                interceptTask.Wait();

                Assert.Equal("0", operation.Result);
                Assert.Equal("0", interceptTask.Result);
            }

            [Fact]
            public void Async_Dispatch_dispatches_to_all_registered_interceptors_even_if_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors();
                var dispatcher = new InternalDispatcher<FakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var exception = new Exception("Bang!");
                var operation = new Task<string>(() => { throw exception; });

                var interceptTask = dispatcher.Dispatch(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    (i, c) => c.Result =c.Result + i.CallMe(c),
                    (c, t) => c);

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext>()), Times.Never()));

                operation.Start();
                Assert.Throws<AggregateException>(() => interceptTask.Wait());

                mockInterceptors.Each(
                    i => i.Verify(m => m.CallMe(It.Is<DbInterceptionContext>(c => c.Exception == exception)), Times.Once()));
            }

            [Fact]
            public void Async_Dispatch_dispatches_to_all_registered_interceptors_even_if_task_is_canceled()
            {
                var mockInterceptors = CreateMockInterceptors();
                var dispatcher = new InternalDispatcher<FakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                var operation = new Task<string>(() => "0", cancellationToken);

                var interceptionContext = new DbCommandInterceptionContext<string>();
                var modifiedContext = new DbCommandInterceptionContext<string>();

                var interceptTask = dispatcher.Dispatch(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    (i, c) => c.Result = c.Result + i.CallMe(c),
                    (c, t) => modifiedContext);

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext>()), Times.Never()));

                cancellationTokenSource.Cancel();
                Assert.Throws<AggregateException>(() => interceptTask.Wait());

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(modifiedContext), Times.Once()));
            }
#endif
        }

        private static IList<Mock<FakeInterceptor1>> CreateMockInterceptors(int count = 3)
        {
            var mockInterceptors = new List<Mock<FakeInterceptor1>>();
            for (var i = 0; i < count; i++)
            {
                var mock = new Mock<FakeInterceptor1>();
                mockInterceptors.Add(mock);
                mock.Setup(m => m.CallMe(It.IsAny<DbInterceptionContext>())).Returns((i + 1).ToString(CultureInfo.InvariantCulture));
            }
            return mockInterceptors;
        }

        public interface FakeInterceptor1 : IDbInterceptor
        {
            string CallMeFirst(DbInterceptionContext interceptionContext);
            string CallMe(DbInterceptionContext interceptionContext);
        }

        public interface FakeInterceptor2 : IDbInterceptor
        {
            string CallMe(DbInterceptionContext interceptionContext);
        }
    }
}
