// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if !NET40
namespace System.Data.Entity.Utilities
{
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class TaskExtensionsTests
    {
        [Fact]
        public void NonGeneric_WithCurrentCulture_preserves_culture()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            var expectedCulture = new CultureInfo("de-DE");
            Thread.CurrentThread.CurrentCulture = expectedCulture;

            try
            {
                var culture = GetCurrentCultureAsync().Result;

                Assert.Equal(expectedCulture, culture);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        [Fact]
        public void Generic_WithCurrentCulture_preserves_culture()
        {
            var originalCulture = Thread.CurrentThread.CurrentCulture;
            var expectedCulture = new CultureInfo("de-DE");
            Thread.CurrentThread.CurrentCulture = expectedCulture;

            try
            {
                var culture = GetCurrentCultureAsync<object>().Result;

                Assert.Equal(expectedCulture, culture);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
            }
        }

        private async Task<CultureInfo> GetCurrentCultureAsync()
        {
            await ConfigurableYield().WithCurrentCulture();
            return Thread.CurrentThread.CurrentCulture;
        }

        private async Task<CultureInfo> GetCurrentCultureAsync<T>()
        {
            await ConfigurableYield<T>().WithCurrentCulture();
            return Thread.CurrentThread.CurrentCulture;
        }

        private async Task ConfigurableYield()
        {
            await Task.Yield();
        }

        private async Task<T> ConfigurableYield<T>()
        {
            await Task.Yield();
            return default(T);
        }
    }
}
#endif