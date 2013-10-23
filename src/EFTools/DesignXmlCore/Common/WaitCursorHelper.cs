// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Common
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    ///     Useful class that creates a disposable item controlling the VS wait cursor.
    /// </summary>
    internal sealed class WaitCursorHelper
    {
        /// <summary>
        ///     Create a wait cursor object.
        /// </summary>
        /// <returns></returns>
        public static IDisposable NewWaitCursor()
        {
            return new WaitCursor();
        }

        /// <summary>
        ///     Changes the cursor to the wait icon and restores the cursor when disposed.
        /// </summary>
        private class WaitCursor : IDisposable
        {
            /// <summary>
            ///     Cache or save off current cursor and set cursor to the wait cursor.
            /// </summary>
            public WaitCursor()
            {
                _currentCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
            }

            /// <summary>
            ///     Finializer.
            /// </summary>
            ~WaitCursor()
            {
                DisposeInternal();
            }

            /// <summary>
            ///     Dispose.
            /// </summary>
            public void Dispose()
            {
                DisposeInternal();
                GC.SuppressFinalize(this);
            }

            /// <summary>
            ///     Restore cursor to what it was before we switched to the wait cursor.
            /// </summary>
            private void DisposeInternal()
            {
                if (_currentCursor != null)
                {
                    lock (this)
                    {
                        if (_currentCursor != null)
                        {
                            Cursor.Current = _currentCursor;
                            _currentCursor = null;
                        }
                    }
                }
            }

            // Original cursor.
            private Cursor _currentCursor;
        }
    }
}
