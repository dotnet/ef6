// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Common
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell.Interop;

    internal sealed class WaitDialog
    {
        private readonly IVsThreadedWaitDialog2 _waitDialog;
        private readonly string _caption;
        private readonly bool _cancelable;
        private readonly bool _supportsPercentage;
        private bool _dialogStarted;

        internal WaitDialog(IServiceProvider site, string caption, bool cancelable, bool supportsPercentage = false)
        {
            var factory = site.GetService(typeof(SVsThreadedWaitDialogFactory)) as IVsThreadedWaitDialogFactory;
            Debug.Assert(factory != null);

            ThrowOnFailure(factory.CreateInstance(out _waitDialog));
            Debug.Assert(_waitDialog != null);

            _caption = caption;
            _cancelable = cancelable;
            _supportsPercentage = supportsPercentage;
        }

        internal void Start(string message, string progress, int delayToStart)
        {
            Debug.Assert(_dialogStarted == false, "attempting to start running dialog");
            if (!_dialogStarted)
            {
                if (_supportsPercentage)
                {
                    ThrowOnFailure(
                        _waitDialog.StartWaitDialogWithPercentageProgress(
                            _caption, message, progress, null, null, _cancelable, delayToStart, 0, 0));
                }
                else
                {
                    ThrowOnFailure(_waitDialog.StartWaitDialog(_caption, message, progress, null, null, delayToStart, _cancelable, true));
                }
                _dialogStarted = true;
            }
        }

        internal bool End()
        {
            var hasCanceled = 0;

            Debug.Assert(_dialogStarted, "attempting to end a non-running dialog");
            if (_dialogStarted)
            {
                ThrowOnFailure(_waitDialog.EndWaitDialog(out hasCanceled));
                _dialogStarted = false;
            }

            return Convert.ToBoolean(hasCanceled);
        }

        private bool UpdateProgressInternal(string message, bool disableCancelable, int currentStep, int totalSteps)
        {
            var hasCanceled = false;

            if (_dialogStarted)
            {
                ThrowOnFailure(
                    _waitDialog.UpdateProgress(
                        null,
                        message,
                        null,
                        currentStep,
                        totalSteps,
                        disableCancelable,
                        out hasCanceled));
            }

            return hasCanceled;
        }

        internal bool UpdateProgressWithPercentage(string message, bool disableCancelable, int currentStep, int totalSteps)
        {
            if (!_supportsPercentage)
            {
                throw new ArgumentException("If you called this method ensure the WaitDialog _supportsPercentage");
            }

            return UpdateProgressInternal(message, disableCancelable, currentStep, totalSteps);
        }

        internal bool UpdateProgress(string message, bool disableCancelable)
        {
            if (_supportsPercentage)
            {
                throw new ArgumentException("If you called this method ensure the WaitDialog does not _supportsPercentage");
            }

            return UpdateProgressInternal(message, disableCancelable, 0, 0);
        }

        internal bool HasCanceled()
        {
            var hasCanceled = false;

            if (_dialogStarted)
            {
                ThrowOnFailure(_waitDialog.HasCanceled(out hasCanceled));
            }

            return hasCanceled;
        }

        private static int ThrowOnFailure(int hr)
        {
            if (hr < 0)
            {
                Marshal.ThrowExceptionForHR(hr);
            }

            return hr;
        }
    }
}
