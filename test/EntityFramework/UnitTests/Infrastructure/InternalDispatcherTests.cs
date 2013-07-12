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
                var mockInterceptor1 = new Mock<IFakeInterceptor1>();
                var mockInterceptor2 = new Mock<IFakeInterceptor2>();

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();

                dispatcher.Add(mockInterceptor1.Object);
                dispatcher.Add(mockInterceptor2.Object);

                dispatcher.Dispatch(i => i.CallMe(new DbCommandInterceptionContext<string>()));

                mockInterceptor1.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Once());
                mockInterceptor2.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Never());

                dispatcher.Remove(mockInterceptor1.Object);
                dispatcher.Remove(mockInterceptor2.Object);

                dispatcher.Dispatch(i => i.CallMe(new DbCommandInterceptionContext<string>()));

                mockInterceptor1.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Once());
                mockInterceptor2.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Never());
            }

            [Fact]
            public void Removing_an_interceptor_that_is_not_registered_is_a_no_op()
            {
                new InternalDispatcher<IFakeInterceptor1>().Remove(new Mock<IFakeInterceptor1>().Object);
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
                var mockInterceptors = CreateMockInterceptors(c => { }, c => { });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                dispatcher.Dispatch(i => i.CallMe(new DbCommandInterceptionContext<string>()));

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Once()));
            }

            [Fact]
            public void Result_Dispatch_dispatches_to_all_registered_interceptors_and_aggregates_result()
            {
                var mockInterceptors = CreateMockInterceptors(c => { }, c => { });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Equal("0123", dispatcher.Dispatch("0", (r, i) => r + i.CallMe(new DbCommandInterceptionContext<string>())));

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Once()));
            }

            [Fact]
            public void Result_Dispatch_returns_result_if_no_dispatchers_registered()
            {
                Assert.Equal(
                    "0",
                    new InternalDispatcher<IFakeInterceptor1>().Dispatch(
                        "0", (r, i) => r + i.CallMe(new DbCommandInterceptionContext<string>())));
            }

            [Fact]
            public void Operation_Dispatch_dispatches_to_all_registered_interceptors_and_aggregates_results_of_operations()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.NotEmpty(c.Result);
                            Assert.Equal("0", c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Equal(
                    "0123",
                    dispatcher.Dispatch(
                        () => "0",
                        interceptionContext,
                        i => i.CallMeFirst(interceptionContext),
                        i => interceptionContext.Result = interceptionContext.Result + i.CallMe(interceptionContext)));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Execution_of_operation_can_be_suppressed_by_setting_result_with_everything_else_still_happening()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            if (c.Result == null)
                            {
                                c.Result = "N";
                            }
                            Assert.True(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.NotEmpty(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.True(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Equal(
                    "N123",
                    dispatcher.Dispatch(
                        (Func<string>)(() => { throw new Exception("Bang!"); }),
                        interceptionContext,
                        i => i.CallMeFirst(interceptionContext),
                        i => interceptionContext.Result = interceptionContext.Result + i.CallMe(interceptionContext)));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Execution_of_operation_can_be_suppressed_by_setting_exception_with_everything_else_still_happening()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.OriginalException);
                            if (c.Exception == null)
                            {
                                c.Exception = new Exception("Bing!");
                            }
                            Assert.True(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Equal("Bing!", c.Exception.Message);
                            Assert.Null(c.OriginalException);
                            Assert.True(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var exception = Assert.Throws<Exception>(
                    () => dispatcher.Dispatch(
                        (Func<string>)(() => { throw new Exception("Bang!"); }),
                        interceptionContext,
                        i => i.CallMeFirst(interceptionContext),
                        i => i.CallMe(interceptionContext)));

                Assert.Equal("Bing!", exception.Message);

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Execution_of_operation_can_be_suppressed_explicitly_with_everything_else_still_happening()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.OriginalException);
                            Assert.Null(c.OriginalException);
                            c.SuppressExecution();
                            Assert.True(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.OriginalException);
                            Assert.Null(c.OriginalException);
                            Assert.True(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Null(
                    dispatcher.Dispatch(
                        (Func<string>)(() => { throw new Exception("Bang!"); }),
                        interceptionContext,
                        i => i.CallMeFirst(interceptionContext),
                        i => i.CallMe(interceptionContext)));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Operation_Dispatch_executes_operation_and_returns_result_if_no_dispatchers_registered()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                Assert.Equal(
                    "0",
                    new InternalDispatcher<IFakeInterceptor1>().Dispatch(
                        () => "0", 
                        interceptionContext, 
                        i => i.CallMeFirst(interceptionContext),
                        i => interceptionContext.Result = interceptionContext.Result + i.CallMe(interceptionContext)));
            }

            [Fact]
            public void Operation_Dispatch_dispatches_to_all_registered_interceptors_even_if_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Equal("Bang!", c.Exception.Message);
                            Assert.Equal("Bang!", c.OriginalException.Message);
                            Assert.False(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var exception = Assert.Throws<Exception>(
                    () => dispatcher.Dispatch(
                        (Func<string>)(() => { throw new Exception("Bang!"); }),
                        interceptionContext,
                        i => i.CallMeFirst(interceptionContext),
                        i => i.CallMe(interceptionContext)));

                Assert.Equal("Bang!", exception.Message);

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Operation_Dispatch_can_change_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.NotEmpty(c.Exception.Message);
                            Assert.Equal("Bang!", c.OriginalException.Message);
                            c.Exception = new Exception("Bing!");
                            Assert.False(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var exception = Assert.Throws<Exception>(
                    () => dispatcher.Dispatch(
                        (Func<string>)(() => { throw new Exception("Bang!"); }),
                        interceptionContext,
                        i => i.CallMeFirst(interceptionContext),
                        i => i.CallMe(interceptionContext)));

                Assert.Equal("Bing!", exception.Message);

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Operation_Dispatch_can_prevent_exception_from_being_thrown_and_return_result_instead()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.False(c.IsAsync);
                            Assert.Null(c.OriginalResult);
                            Assert.Equal("Bang!", c.OriginalException.Message);
                            c.Exception = null;
                            c.Result = "N";
                            Assert.False(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Equal(
                    "N",
                    dispatcher.Dispatch(
                        (Func<string>)(() => { throw new Exception("Bang!"); }),
                        interceptionContext,
                        i => i.CallMeFirst(interceptionContext),
                        i => i.CallMe(interceptionContext)));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Operation_Dispatch_is_aborted_if_Executing_interceptor_throws_exception()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors(
                    c => { throw new Exception("Ba-da-bing!"); },
                    c => { });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Equal(
                    "Ba-da-bing!",
                    Assert.Throws<Exception>(
                        () => dispatcher.Dispatch(
                            () => "N",
                            interceptionContext,
                            i => i.CallMeFirst(interceptionContext),
                            i => i.CallMe(interceptionContext))).Message);

                mockInterceptors.First().Verify(m => m.CallMeFirst(interceptionContext), Times.Once());
                mockInterceptors.Skip(1).Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Never()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Never()));
            }

            [Fact]
            public void Operation_Dispatch_is_aborted_if_Executed_interceptor_throws_exception()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors(
                    c => { },
                    c => { throw new Exception("Ba-da-bing!"); });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Equal(
                    "Ba-da-bing!",
                    Assert.Throws<Exception>(
                        () => dispatcher.Dispatch(
                            () => "N",
                            interceptionContext,
                            i => i.CallMeFirst(interceptionContext),
                            i => i.CallMe(interceptionContext))).Message);

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.First().Verify(m => m.CallMe(interceptionContext), Times.Once());
                mockInterceptors.Skip(1).Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Never()));
            }

#if !NET40
            [Fact]
            public void Async_Dispatch_dispatches_to_all_registered_interceptors_and_aggregates_results_of_operations()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>().AsAsync();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.NotEmpty(c.Result);
                            Assert.Equal("0", c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var operation = new Task<string>(() => "0");

                var interceptTask = dispatcher.DispatchAsync(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    i => interceptionContext.Result = interceptionContext.Result + i.CallMe(interceptionContext));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Never()));

                operation.Start();
                interceptTask.Wait();

                Assert.Equal("0123", interceptTask.Result);

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Execution_of_async_operation_can_be_suppressed_by_setting_result_with_everything_else_still_happening()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>().AsAsync();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            if (c.Result == null)
                            {
                                c.Result = "N";
                            }
                            Assert.True(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.NotEmpty(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.True(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var operation = new Task<string>(() => "0");

                var interceptTask = dispatcher.DispatchAsync(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    i => interceptionContext.Result = interceptionContext.Result + i.CallMe(interceptionContext));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));

                interceptTask.Wait();

                Assert.Equal("N123", interceptTask.Result);
            }

            [Fact]
            public void Execution_of_async_operation_can_be_suppressed_by_setting_exception_with_everything_else_still_happening()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>().AsAsync();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.OriginalException);
                            if (c.Exception == null)
                            {
                                c.Exception = new Exception("Bing!");
                            }
                            Assert.True(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Equal("Bing!", c.Exception.Message);
                            Assert.Null(c.OriginalException);
                            Assert.True(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var operation = new Task<string>(() => "0");

                var interceptTask = dispatcher.DispatchAsync(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    i => i.CallMe(interceptionContext));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));

                Assert.Equal(
                    "Bing!",
                    Assert.Throws<AggregateException>(() => interceptTask.Wait()).InnerException.Message);
            }

            [Fact]
            public void Execution_of_async_operation_can_be_suppressed_explicitly_with_everything_else_still_happening()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>().AsAsync();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.OriginalException);
                            Assert.Null(c.OriginalException);
                            c.SuppressExecution();
                            Assert.True(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.OriginalException);
                            Assert.Null(c.OriginalException);
                            Assert.True(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var operation = new Task<string>(() => "0");

                var interceptTask = dispatcher.DispatchAsync(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    i => i.CallMe(interceptionContext));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));

                interceptTask.Wait();

                Assert.Null(interceptTask.Result);
            }

            [Fact]
            public void Async_Dispatch_executes_operation_and_returns_result_if_no_dispatchers_registered()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>().AsAsync();
                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                var operation = new Task<string>(() => "0");

                var interceptTask = dispatcher.DispatchAsync(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    i => interceptionContext.Result = interceptionContext.Result + i.CallMe(interceptionContext));

                operation.Start();
                interceptTask.Wait();

                Assert.Equal("0", operation.Result);
                Assert.Equal("0", interceptTask.Result);
            }

            [Fact]
            public void Async_Dispatch_dispatches_to_all_registered_interceptors_even_if_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>().AsAsync();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Equal("Bang!", c.Exception.Message);
                            Assert.Equal("Bang!", c.OriginalException.Message);
                            Assert.False(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var operation = new Task<string>(() => { throw new Exception("Bang!"); });

                var interceptTask = dispatcher.DispatchAsync(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    i => i.CallMe(interceptionContext));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Never()));

                operation.Start();
                Assert.Equal(
                    "Bang!",
                    Assert.Throws<AggregateException>(() => interceptTask.Wait()).InnerException.Message);

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Async_Dispatch_can_change_exception_thrown()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>().AsAsync();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.NotEmpty(c.Exception.Message);
                            Assert.Equal("Bang!", c.OriginalException.Message);
                            c.Exception = new Exception("Bing!");
                            Assert.False(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var operation = new Task<string>(() => { throw new Exception("Bang!"); });

                var interceptTask = dispatcher.DispatchAsync(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    i => i.CallMe(interceptionContext));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Never()));

                operation.Start();
                Assert.Equal(
                    "Bing!",
                    Assert.Throws<AggregateException>(() => interceptTask.Wait()).InnerException.Message);

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Async_Dispatch_can_prevent_exception_from_being_thrown_and_return_result_instead()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>().AsAsync();
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.OriginalResult);
                            Assert.Equal("Bang!", c.OriginalException.Message);
                            c.Exception = null;
                            c.Result = "N";
                            Assert.False(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var operation = new Task<string>(() => { throw new Exception("Bang!"); });

                var interceptTask = dispatcher.DispatchAsync(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    i => i.CallMe(interceptionContext));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Never()));

                operation.Start();
                interceptTask.Wait();

                Assert.Equal("N", interceptTask.Result);

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Async_Dispatch_dispatches_to_all_registered_interceptors_even_if_task_is_canceled()
            {
                var mockInterceptors = CreateMockInterceptors(
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        },
                    c =>
                        {
                            Assert.True(c.IsAsync);
                            Assert.Null(c.Result);
                            Assert.Null(c.OriginalResult);
                            Assert.Null(c.Exception);
                            Assert.Null(c.OriginalException);
                            Assert.False(c.IsSuppressed);
                        });

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var cancellationTokenSource = new CancellationTokenSource();
                var cancellationToken = cancellationTokenSource.Token;
                var operation = new Task<string>(() => "0", cancellationToken);

                var interceptionContext = new DbCommandInterceptionContext<string>().AsAsync();

                var interceptTask = dispatcher.DispatchAsync(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    i => i.CallMe(interceptionContext));

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()), Times.Never()));

                cancellationTokenSource.Cancel();
                Assert.Throws<AggregateException>(() => interceptTask.Wait());

                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Once()));
            }

            [Fact]
            public void Async_Dispatch_is_aborted_if_Executing_interceptor_throws_exception()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>().AsAsync();
                var mockInterceptors = CreateMockInterceptors(
                    c => { throw new Exception("Ba-da-bing!"); },
                    c => { });

                var operation = new Task<string>(() => "0");

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                Assert.Equal(
                    "Ba-da-bing!",
                    Assert.Throws<Exception>(
                        () => dispatcher.DispatchAsync(
                            () => operation,
                            interceptionContext,
                            i => i.CallMeFirst(interceptionContext),
                            i => i.CallMe(interceptionContext))).Message);

                mockInterceptors.First().Verify(m => m.CallMeFirst(interceptionContext), Times.Once());
                mockInterceptors.Skip(1).Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Never()));
                mockInterceptors.Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Never()));
            }

            [Fact]
            public void Async_Dispatch_creates_faulted_task_if_Executed_interceptor_throws_exception()
            {
                var interceptionContext = new DbCommandInterceptionContext<string>();
                var mockInterceptors = CreateMockInterceptors(
                    c => { },
                    c => { throw new Exception("Ba-da-bing!"); });

                var operation = new Task<string>(() => "0");

                var dispatcher = new InternalDispatcher<IFakeInterceptor1>();
                mockInterceptors.Each(i => dispatcher.Add(i.Object));

                var interceptTask = dispatcher.DispatchAsync(
                    () => operation,
                    interceptionContext,
                    i => i.CallMeFirst(interceptionContext),
                    i => i.CallMe(interceptionContext));

                operation.Start();

                Assert.Equal(
                    "Ba-da-bing!",
                    Assert.Throws<AggregateException>(() => interceptTask.Wait()).InnerException.Message);

                mockInterceptors.Each(i => i.Verify(m => m.CallMeFirst(interceptionContext), Times.Once()));
                mockInterceptors.First().Verify(m => m.CallMe(interceptionContext), Times.Once());
                mockInterceptors.Skip(1).Each(i => i.Verify(m => m.CallMe(interceptionContext), Times.Never()));
            }
#endif
        }

        private static IList<Mock<IFakeInterceptor1>> CreateMockInterceptors(
            Action<DbCommandInterceptionContext<string>> callMeFirstAction,
            Action<DbCommandInterceptionContext<string>> callMeAction,
            int count = 3)
        {
            var mockInterceptors = new List<Mock<IFakeInterceptor1>>();
            for (var i = 0; i < count; i++)
            {
                var mock = new Mock<IFakeInterceptor1>();
                mockInterceptors.Add(mock);

                mock.Setup(m => m.CallMeFirst(It.IsAny<DbCommandInterceptionContext<string>>()))
                    .Callback(callMeFirstAction);

                mock.Setup(m => m.CallMe(It.IsAny<DbCommandInterceptionContext<string>>()))
                    .Callback(callMeAction)
                    .Returns((i + 1).ToString(CultureInfo.InvariantCulture));
            }
            return mockInterceptors;
        }

        public interface IFakeInterceptor1 : IDbInterceptor
        {
            string CallMeFirst(DbCommandInterceptionContext<string> interceptionContext);
            string CallMe(DbCommandInterceptionContext<string> interceptionContext);
        }

        public interface IFakeInterceptor2 : IDbInterceptor
        {
            string CallMe(DbInterceptionContext interceptionContext);
        }
    }
}
