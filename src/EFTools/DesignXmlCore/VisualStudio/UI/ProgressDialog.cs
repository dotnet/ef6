// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.UI
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;

    /// <summary>
    ///     This dialog shows a description, a progress bar, and a labelled status area. Through a BackgroundWorker
    ///     it runs the ProgressDialogWork delegate passed in.
    ///     While the backgroundjob is running the closeInterruptButton is in Interrupt mode: if the user presses
    ///     the button in this mode we will cancel the background job and show the user the final state.
    ///     Once the background job has been cancelled or if there was an exception running it the status will be
    ///     updated to show the final status and the closeInterruptButton will be put in Close mode. Clicking on it
    ///     in Close mode will close the dialog.
    ///     If the user never clicks on the closeInterruptButton the dialog will close automatically once the
    ///     background job finishes successfully.
    ///     The background delegate can send status updates to the BackgroundWorker through its ReportProgress API.
    ///     It expects to receive a ProgressDialogUserState object as the argument to this (see below)
    /// </summary>
    internal partial class ProgressDialog : Form
    {
        #region TESTS

        // For test purposes only!!!
        private static event EventHandler BackgroundWorkCompletedEventStorage;

        // This should be set only in the test code!!!
        internal static event EventHandler BackgroundWorkCompletedEvent
        {
            add { BackgroundWorkCompletedEventStorage += value; }
            remove { BackgroundWorkCompletedEventStorage -= value; }
        }

        // tests reset this value to simulate user pressing the Stop button after this number of
        // sprocs have been processed
        // (Note: 1st sproc will be labelled iteration 1 i.e. iterations are 1-based not 0-based)
        internal static int SimulateStopAfter = int.MaxValue;

        #endregion

        private ProgressDialogUserState _currentUserState;
        private readonly ProgressDialogWork _progressDialogWork;
        private readonly object _progressDialogWorkArgument;

        internal ProgressDialog(
            string title, string description, string initialStatus, ProgressDialogWork progressDialogWork, object progressDialogWorkArgument)
        {
            InitializeComponent();

            Text = title;
            descriptionTextBox.Text = description;
            statusTextBox.Text = initialStatus;
            _progressDialogWork = progressDialogWork;
            _progressDialogWorkArgument = progressDialogWorkArgument;
        }

        internal string DialogTitle
        {
            get { return Text; }
            set { Text = value; }
        }

        internal string Description
        {
            get { return descriptionTextBox.Text; }
            set { descriptionTextBox.Text = value; }
        }

        internal string Status
        {
            get { return statusTextBox.Text; }
            set { statusTextBox.Text = value; }
        }

        private void ProgressDialog_Load(object sender, EventArgs e)
        {
            // start background worker
            Debug.Assert(
                backgroundWorker.IsBusy == false, "Should not attempt to start the background job when the backgroundWorker is already busy");
            if (false == backgroundWorker.IsBusy)
            {
                closeInterruptButton.Text = Resources.ProgressDialogCloseInterruptButtonInterruptText; // set button to interrupt mode
                closeInterruptButton.Enabled = true;
                backgroundWorker.RunWorkerAsync(_progressDialogWorkArgument);
            }
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker that raised this event.
            var worker = sender as BackgroundWorker;
            if (null != worker)
            {
                e.Result = _progressDialogWork(worker, e);
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            _currentUserState = (ProgressDialogUserState)e.UserState;
            Status = _currentUserState.CurrentStatusMessage;

            #region TESTS

            // For test purposes only!!!
            // simulate a user clicking the Stop button
            if (_currentUserState.CurrentIteration >= SimulateStopAfter)
            {
                closeInterruptButton_Click(this, EventArgs.Empty);
            }

            #endregion
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // some error happened while executing DoWork
                DialogResult = DialogResult.Abort; // set result to Abort so that caller knows what happened
                var sb = new StringBuilder();
                sb.AppendLine(
                    string.Format(
                        CultureInfo.CurrentCulture, Resources.ProgressDialogBackgroundJobErrorMessage, _currentUserState.CurrentIteration,
                        _currentUserState.NumberIterations, e.GetType().FullName, e.Error.Message, e.Error.StackTrace));
                var inner = e.Error.InnerException;
                var indent = "  ";
                while (inner != null)
                {
                    var message = string.Format(
                        CultureInfo.CurrentCulture, Resources.ProgressDialogBackgroundJobInnerExceptionErrorMessage,
                        inner.GetType().FullName, inner.Message, inner.StackTrace);
                    sb.AppendLine(indent + message);
                    inner = inner.InnerException;
                    indent += "  ";
                }
                Status = sb.ToString();
                closeInterruptButton.Text = Resources.ProgressDialogCloseInterruptButtonCloseText;
                closeInterruptButton.Enabled = true;
                if (BackgroundWorkCompletedEventStorage != null)
                {
                    BackgroundWorkCompletedEventStorage(this, EventArgs.Empty);
                }
            }
            else if (e.Cancelled)
            {
                // background job was cancelled by user - show status and set stopInterruptButton to Close mode
                DialogResult = DialogResult.Cancel; // set result to Cancel so that caller knows what happened
                var statusText = string.Format(
                    CultureInfo.CurrentCulture, Resources.ProgressDialogBackgroundJobCancellationMessage, _currentUserState.CurrentIteration,
                    _currentUserState.NumberIterations, _currentUserState.CurrentStatusMessage);
                Status = statusText;
                closeInterruptButton.Text = Resources.ProgressDialogCloseInterruptButtonCloseText;
                closeInterruptButton.Enabled = true;
                if (BackgroundWorkCompletedEventStorage != null)
                {
                    BackgroundWorkCompletedEventStorage(this, EventArgs.Empty);
                }
            }
            else
            {
                // background job completed successfully - close dialog
                DialogResult = DialogResult.OK; // set result to OK so that caller knows what happened
                closeInterruptButton.Enabled = false;
                if (BackgroundWorkCompletedEventStorage != null)
                {
                    BackgroundWorkCompletedEventStorage(this, EventArgs.Empty);
                }
                Close();
            }
        }

        private void closeInterruptButton_Click(object sender, EventArgs e)
        {
            if (closeInterruptButton.Text.Equals(
                Resources.ProgressDialogCloseInterruptButtonInterruptText, StringComparison.CurrentCulture))
            {
                // user clicked on closeInterruptButton in Interrupt mode - so cancel background job
                closeInterruptButton.Enabled = false; // when cancellation is received button will be re-enabled in Close mode
                backgroundWorker.CancelAsync();
            }
            else
            {
                // user clicked on closeInterruptButton in Close mode - so just close the dialog
                Close();
            }
        }
    }

    internal delegate object ProgressDialogWork(BackgroundWorker worker, DoWorkEventArgs e);

    /// <summary>
    ///     Keeps track of state of current iteration being processed, the total number of iterations to be processed
    ///     and the current status message to be displayed.
    /// </summary>
    internal struct ProgressDialogUserState
    {
        internal int NumberIterations;
        internal int CurrentIteration;
        internal string CurrentStatusMessage;
        internal bool IsError;
    }

    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    [Serializable]
    internal class ProgressDialogException : Exception
    {
        internal ProgressDialogException(string message)
            : base(message)
        {
        }

        internal ProgressDialogException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ProgressDialogException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
