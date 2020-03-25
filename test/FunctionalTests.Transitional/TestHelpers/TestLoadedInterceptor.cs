// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Globalization;

    public class TestLoadedInterceptor : IDbConfigurationInterceptor
    {
        public static readonly ConcurrentStack<string> HooksRun = new ConcurrentStack<string>();

        private readonly string _tag;

        public TestLoadedInterceptor()
        {
            _tag = "Hook1()";
        }

        public TestLoadedInterceptor(int p1, string p2)
        {
            _tag = string.Format(CultureInfo.InvariantCulture, "Hook1({0}, '{1}')", p1, p2);
        }

        public void Loaded(
            DbConfigurationLoadedEventArgs loadedEventArgs,
            DbConfigurationInterceptionContext interceptionContext)
        {
            HooksRun.Push(_tag);
        }
    }
}
