// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Reflection;
    using Xunit;

    public static class DbDispatchersHelpers
    {
        public static void AssertNoInterceptors()
        {
            Assert.Empty(
                (List<ICancelableDbCommandInterceptor>)
                    typeof(InternalDispatcher<ICancelableDbCommandInterceptor>).GetField(
                        "_interceptors", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(DbInterception.Dispatch.CancelableCommand.InternalDispatcher));
            Assert.Empty(
                (List<ICancelableEntityConnectionInterceptor>)
                    typeof(InternalDispatcher<ICancelableEntityConnectionInterceptor>).GetField(
                        "_interceptors", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(DbInterception.Dispatch.CancelableEntityConnection.InternalDispatcher));
            Assert.Empty(
                (List<IDbCommandInterceptor>)
                    typeof(InternalDispatcher<IDbCommandInterceptor>).GetField(
                        "_interceptors", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(DbInterception.Dispatch.Command.InternalDispatcher));
            Assert.Empty(
                (List<IDbCommandTreeInterceptor>)
                    typeof(InternalDispatcher<IDbCommandTreeInterceptor>).GetField(
                        "_interceptors", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(DbInterception.Dispatch.CommandTree.InternalDispatcher));
            Assert.Empty(
                (List<IDbConnectionInterceptor>)
                    typeof(InternalDispatcher<IDbConnectionInterceptor>).GetField(
                        "_interceptors", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(DbInterception.Dispatch.Connection.InternalDispatcher));
            Assert.Empty(
                (List<IDbTransactionInterceptor>)
                    typeof(InternalDispatcher<IDbTransactionInterceptor>).GetField(
                        "_interceptors", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(DbInterception.Dispatch.Transaction.InternalDispatcher));
        }
    }
}
