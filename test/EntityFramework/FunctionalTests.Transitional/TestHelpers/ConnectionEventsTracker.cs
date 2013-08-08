// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Common;
    using Xunit;

    public class ConnectionEventsTracker
    {
        private int countOpenClose;
        private int countCloseOpen;
        private int countOtherConnectionStates;

        public ConnectionEventsTracker(DbConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentException("Cannot track events for a null connection!");
            }

            connection.StateChange += OnStateChange;
        }

        /// <summary>
        /// Verifies the no connection events were fired.
        /// </summary>
        public void VerifyNoConnectionEventsWereFired()
        {
            Assert.True(countOpenClose == 0);
            Assert.True(countCloseOpen == 0);
            Assert.True(countOtherConnectionStates == 0);
        }

        /// <summary>
        /// Verifies the connection open and close events were fired.
        /// </summary>
        public void VerifyConnectionOpenCloseEventsWereFired()
        {
            Assert.True(countCloseOpen == 1);
            Assert.True(countOpenClose == 1);
            Assert.True(countOtherConnectionStates == 0);
        }

        /// <summary>
        /// Verifies the connection opened event was fired.
        /// </summary>
        public void VerifyConnectionOpenedEventWasFired()
        {
            Assert.True(countCloseOpen == 1);
            Assert.True(countOpenClose == 0);
            Assert.True(countOtherConnectionStates == 0);
        }

        /// <summary>
        /// Verifies the connection closed event was fired.
        /// </summary>
        public void VerifyConnectionClosedEventWasFired()
        {
            Assert.True(countOpenClose == 1);
            Assert.True(countCloseOpen == 0);
            Assert.True(countOtherConnectionStates == 0);
        }

        public void ResetEventCounters()
        {
            countOpenClose = countCloseOpen = countOtherConnectionStates = 0;
        }

        private void OnStateChange(object sender, StateChangeEventArgs args)
        {
            if (args.OriginalState == ConnectionState.Closed
                && args.CurrentState == ConnectionState.Open)
            {
                countCloseOpen++;
            }
            else if (args.OriginalState == ConnectionState.Open
                     && args.CurrentState == ConnectionState.Closed)
            {
                countOpenClose++;
            }
            else
            {
                countOtherConnectionStates++;
            }
        }
    }
}
