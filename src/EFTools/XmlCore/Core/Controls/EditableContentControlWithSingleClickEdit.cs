// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Core.Controls
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Threading;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class EditableContentControlWithSingleClickEdit : EditableContentControl
    {
        private DispatcherTimer _renameTimer;
        private bool _renameOnClick;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="sender">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="e">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender, e);

            if (FocusableAncestor != null)
            {
                FocusableAncestor.PreviewMouseLeftButtonUp += focusableAncestor_PreviewMouseLeftButtonUp;
                FocusableAncestor.PreviewMouseLeftButtonDown += focusableAncestor_PreviewMouseLeftButtonDown;
            }

            MouseDoubleClick += EditableContentControl_MouseDoubleClick;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="sender">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="e">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected override void OnUnloaded(object sender, RoutedEventArgs e)
        {
            base.OnUnloaded(sender, e);

            if (FocusableAncestor != null)
            {
                FocusableAncestor.MouseLeftButtonUp -= focusableAncestor_PreviewMouseLeftButtonUp;
                FocusableAncestor.PreviewMouseLeftButtonDown -= focusableAncestor_PreviewMouseLeftButtonDown;
            }

            MouseDoubleClick -= EditableContentControl_MouseDoubleClick;
        }

        /// <summary>
        ///     MouseLeftButtonUp Handler for focusable ancestor. If renameOnClick flag is set, starts the timer to attemp rename.
        /// </summary>
        private void focusableAncestor_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsEditModeAllowed && _renameOnClick)
            {
                StartRenameTimer();
            }
        }

        /// <summary>
        ///     MouseLeftButtonDown Handler for focusable ancestor. Checks if element sending the event has already in keyboard focus
        ///     If yes, then sets the renameOnClickFlag as true.
        /// </summary>
        private void focusableAncestor_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.Assert(FocusableAncestor != null, "Focusable Ancestor in EditibleControl should not have been null!");

            _renameOnClick = FocusableAncestor.IsKeyboardFocusWithin;
        }

        /// <summary>
        ///     Called if the rename timer times out meaning no double click happened in the meantime.
        ///     Stops the timer, puts the control in edit mode and resets the rename flag.
        /// </summary>
        private void RenameTimer_TimedOut_StartEditing(object sender, EventArgs e)
        {
            StopRenameTimer();

            Debug.Assert(FocusableAncestor != null, "Focusable Ancestor in EditibleControl should not have been null!");

            //check if it's still focused at the moment
            if (FocusableAncestor.IsKeyboardFocusWithin)
            {
                IsInEditMode = true;
            }

            _renameOnClick = false;
        }

        /// <summary>
        ///     Double click on focusable ancestor. Stops the timer if enabled, indicating that we received a double click possibly after a single selection click.
        /// </summary>
        private void EditableContentControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            StopRenameTimer();

            _renameOnClick = false;
        }

        /// <summary>
        ///     Starts the rename timer
        /// </summary>
        private void StartRenameTimer()
        {
            if (_renameTimer == null)
            {
                _renameTimer = new DispatcherTimer(
                    TimeSpan.FromMilliseconds(SystemInformation.DoubleClickTime), DispatcherPriority.Input,
                    RenameTimer_TimedOut_StartEditing, Dispatcher.CurrentDispatcher);
                _renameTimer.Start();
            }
        }

        /// <summary>
        ///     Stops the rename timer.
        /// </summary>
        private void StopRenameTimer()
        {
            if (_renameTimer != null)
            {
                _renameTimer.Stop();
                _renameTimer = null;
            }
        }
    }
}
