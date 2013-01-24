// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Linq;

    /// <summary>
    ///     A retry policy with exponentially increasing delay between retries.
    /// </summary>
    /// <remarks>
    ///     The following formula is used to calculate the delay after <c>retryCount</c> number of attempts:
    ///     <code>min(minDelay + coefficient * random(1, maxRandomFactor) * (exponentialBase ^ retryCount - 1), maxDelay)</code>
    ///     The <c>retryCount</c> starts at 0.
    ///     The <c>random</c> factor distributes uniformly the retry attempts from multiple parallel actions failing simultaneously.
    ///     The <c>coefficient</c> determines the scale at wich the delay is increased while the <c>exponentialBase</c>
    ///     sets the speed of the delay increase.
    /// </remarks>
    public class ExponentialRetryDelayStrategy : IRetryDelayStrategy
    {
        private readonly int _maxRetryCount;
        private readonly TimeSpan _minDelay;
        private readonly TimeSpan _maxDelay;
        private readonly double _maxRandomFactor;
        private readonly double _exponentialBase;
        private readonly TimeSpan _coefficient;

        /// <summary>
        ///     The default number of retry attempts.
        /// </summary>
        public static readonly int DefaultMaxRetryCount = 5;

        /// <summary>
        ///     The default maximum random factor.
        /// </summary>
        public static readonly double DefaultRandomFactor = 1.1;

        /// <summary>
        ///     The default base for the exponential function used to compute the delay between retries.
        /// </summary>
        public static readonly double DefaultExponentialBase = 2;

        /// <summary>
        ///     The default coefficient for the exponential function used to compute the delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultCoefficient = TimeSpan.FromSeconds(1);

        /// <summary>
        ///     The default maximum time delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultMaxDelay = TimeSpan.FromSeconds(30);

        /// <summary>
        ///     The default minimum time delay between retries.
        /// </summary>
        public static readonly TimeSpan DefaultMinDelay = TimeSpan.FromSeconds(0);

        private readonly List<Exception> _exceptionsEncountered = new List<Exception>();
        private readonly Random _random = new Random();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExponentialRetryDelayStrategy"/> class. 
        /// </summary>
        public ExponentialRetryDelayStrategy()
            : this(DefaultMaxRetryCount, DefaultMinDelay, DefaultMaxDelay, DefaultRandomFactor, DefaultExponentialBase, DefaultCoefficient)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExponentialRetryDelayStrategy"/> class. 
        /// </summary>
        /// <param name="maxRetryCount"> The maximum number of retry attempts. </param>
        /// <param name="minDelay"> The minimum delay in milliseconds between retries, must be nonnegative. </param>
        /// <param name="maxDelay"> The maximum delay in milliseconds between retries, must be equal or greater than <paramref name="minDelay"/>. </param>
        /// <param name="maxRandomFactor"> The maximum random factor, must not be lesser than 1. </param>
        /// <param name="exponentialBase"> The base for the exponential function used to compute the delay between retries, must be positive. </param>
        /// <param name="coefficient"> The coefficient for the exponential function used to compute the delay between retries, must be nonnegative. </param>
        public ExponentialRetryDelayStrategy(
            int maxRetryCount, TimeSpan minDelay, TimeSpan maxDelay, double maxRandomFactor, double exponentialBase, TimeSpan coefficient)
        {
            if (maxRetryCount < 0.0)
            {
                throw new ArgumentOutOfRangeException("maxRetryCount");
            }
            if (minDelay.TotalMilliseconds < 0.0)
            {
                throw new ArgumentOutOfRangeException("minDelay");
            }
            if (minDelay.TotalMilliseconds > maxDelay.TotalMilliseconds)
            {
                throw new ArgumentOutOfRangeException("maxDelay", Strings.ExecutionStrategy_MinimumMustBeLessThanMaximum);
            }
            if (maxRandomFactor < 1.0)
            {
                throw new ArgumentOutOfRangeException("maxRandomFactor");
            }
            if (exponentialBase <= 0.0)
            {
                throw new ArgumentOutOfRangeException("exponentialBase");
            }
            if (coefficient.TotalMilliseconds < 0.0)
            {
                throw new ArgumentOutOfRangeException("coefficient");
            }

            _maxRetryCount = maxRetryCount;
            _minDelay = minDelay;
            _maxDelay = maxDelay;
            _maxRandomFactor = maxRandomFactor;
            _exponentialBase = exponentialBase;
            _coefficient = coefficient;
        }

        /// <inheritdoc/>
        public TimeSpan? GetNextDelay(Exception lastException)
        {
            _exceptionsEncountered.Add(lastException);

            var currentRetryCount = _exceptionsEncountered.Count() - 1;
            if (currentRetryCount < _maxRetryCount)
            {
                var delta = (Math.Pow(_exponentialBase, currentRetryCount) - 1.0)
                            * (1.0 + _random.NextDouble() * (_maxRandomFactor - 1.0));

                var delay = Math.Min(
                    _minDelay.TotalMilliseconds + _coefficient.TotalMilliseconds * delta,
                    _maxDelay.TotalMilliseconds);

                return TimeSpan.FromMilliseconds(delay);
            }

            return null;
        }
    }
}
