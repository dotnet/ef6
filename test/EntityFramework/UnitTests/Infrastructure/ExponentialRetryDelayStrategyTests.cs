// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using Xunit;

    public class ExponentialRetryDelayStrategyTests
    {
        [Fact]
        public void Constructor_throws_on_invalid_parameters()
        {
            Assert.Equal(
                "maxRetryCount",
                Assert.Throws<ArgumentOutOfRangeException>(
                    () =>
                    new ExponentialRetryDelayStrategy(
                        maxRetryCount: -1, minDelay: TimeSpan.FromTicks(0), maxDelay: TimeSpan.FromTicks(0), maxRandomFactor: 1,
                        exponentialBase: 1, coefficient: TimeSpan.FromTicks(0))).ParamName);
            Assert.Equal(
                "minDelay",
                Assert.Throws<ArgumentOutOfRangeException>(
                    () =>
                    new ExponentialRetryDelayStrategy(
                        maxRetryCount: 0, minDelay: TimeSpan.FromTicks(-1), maxDelay: TimeSpan.FromTicks(0), maxRandomFactor: 1,
                        exponentialBase: 1, coefficient: TimeSpan.FromTicks(0))).ParamName);
            var maxDelayException = Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                new ExponentialRetryDelayStrategy(
                    maxRetryCount: 0, minDelay: TimeSpan.FromTicks(0), maxDelay: TimeSpan.FromTicks(-1), maxRandomFactor: 1,
                    exponentialBase: 1, coefficient: TimeSpan.FromTicks(0)));
            Assert.Equal(
                new ArgumentOutOfRangeException("maxDelay", Strings.ExecutionStrategy_MinimumMustBeLessThanMaximum).Message,
                maxDelayException.Message);
            Assert.Equal(
                "maxRandomFactor",
                Assert.Throws<ArgumentOutOfRangeException>(
                    () =>
                    new ExponentialRetryDelayStrategy(
                        maxRetryCount: 0, minDelay: TimeSpan.FromTicks(0), maxDelay: TimeSpan.FromTicks(0), maxRandomFactor: 0,
                        exponentialBase: 1, coefficient: TimeSpan.FromTicks(0))).ParamName);
            Assert.Equal(
                "exponentialBase",
                Assert.Throws<ArgumentOutOfRangeException>(
                    () =>
                    new ExponentialRetryDelayStrategy(
                        maxRetryCount: 0, minDelay: TimeSpan.FromTicks(0), maxDelay: TimeSpan.FromTicks(0), maxRandomFactor: 1,
                        exponentialBase: 0, coefficient: TimeSpan.FromTicks(0))).ParamName);
            Assert.Equal(
                "coefficient",
                Assert.Throws<ArgumentOutOfRangeException>(
                    () =>
                    new ExponentialRetryDelayStrategy(
                        maxRetryCount: 0, minDelay: TimeSpan.FromTicks(0), maxDelay: TimeSpan.FromTicks(0), maxRandomFactor: 1,
                        exponentialBase: 1, coefficient: TimeSpan.FromTicks(-1))).ParamName);
        }

        [Fact]
        public void GetNextDelay_returns_the_expected_default_sequence()
        {
            var strategy = new ExponentialRetryDelayStrategy();
            var delays = new List<TimeSpan>();
            TimeSpan? nextDelay;
            while ((nextDelay = strategy.GetNextDelay(null)) != null)
            {
                delays.Add(nextDelay.Value);
            }

            var expectedDelays = new List<TimeSpan>
                                     {
                                         TimeSpan.FromSeconds(0),
                                         TimeSpan.FromSeconds(1),
                                         TimeSpan.FromSeconds(3),
                                         TimeSpan.FromSeconds(7),
                                         TimeSpan.FromSeconds(15)
                                     };

            Assert.Equal(expectedDelays.Count, delays.Count);
            for (var i = 0; i < expectedDelays.Count; i++)
            {
                Assert.True(
                    (delays[i] - expectedDelays[i]).TotalMilliseconds <=
                    expectedDelays[i].TotalMilliseconds * (ExponentialRetryDelayStrategy.DefaultRandomFactor - 1.0) + 1,
                    string.Format("Expected: {0}; Actual: {1}", expectedDelays[i], delays[i]));
            }
        }
    }
}
