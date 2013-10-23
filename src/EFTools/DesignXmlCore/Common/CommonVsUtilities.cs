// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Common
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    internal static class CommonVsUtilities
    {
        // Used for InvokeRequired
        private static Control _marshalingControl;

        /// <summary>
        ///     This is the delegate used to make sure we're calling
        ///     on the UI thread.
        /// </summary>
        /// <returns>
        ///     Dialog result.
        /// </returns>
        private delegate DialogResult SafeShowMessageBox(
            string title,
            string text,
            MessageBoxButtons buttons,
            MessageBoxDefaultButton defaultButton,
            MessageBoxIcon icon);

        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public static DialogResult ShowMessageBoxEx(
            string title,
            string text,
            MessageBoxButtons buttons,
            MessageBoxDefaultButton defaultButton,
            MessageBoxIcon icon)
        {
            if (_marshalingControl == null)
            {
                _marshalingControl = new Control();
            }

            if (_marshalingControl.InvokeRequired)
            {
                return (DialogResult)_marshalingControl.Invoke(
                    new SafeShowMessageBox(ShowMessageBoxEx),
                    title,
                    text,
                    defaultButton,
                    buttons,
                    icon);
            }

            var uiShell = Package.GetGlobalService(typeof(SVsUIShell)) as IVsUIShell;

            Debug.Assert(uiShell != null);
            var result = (int)(DialogResult.OK);

            if (uiShell != null)
            {
                var clsid = Guid.Empty;
                var vsButtons = (OLEMSGBUTTON)buttons;
                var vsIcon = OLEMSGICON.OLEMSGICON_INFO;

                // need to translate from Winform icon enum to VS enum.
                switch (icon)
                {
                    case MessageBoxIcon.Error:
                        vsIcon = OLEMSGICON.OLEMSGICON_CRITICAL;
                        break;
                    case MessageBoxIcon.Information:
                        vsIcon = OLEMSGICON.OLEMSGICON_INFO;
                        break;
                    case MessageBoxIcon.None:
                        vsIcon = OLEMSGICON.OLEMSGICON_NOICON;
                        break;
                    case MessageBoxIcon.Question:
                        vsIcon = OLEMSGICON.OLEMSGICON_QUERY;
                        break;
                    case MessageBoxIcon.Warning:
                        vsIcon = OLEMSGICON.OLEMSGICON_WARNING;
                        break;
                }

                var oleDefButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
                if (defaultButton == MessageBoxDefaultButton.Button2)
                {
                    oleDefButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND;
                }
                else if (defaultButton == MessageBoxDefaultButton.Button3)
                {
                    oleDefButton = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_THIRD;
                }

                NativeMethods.ThrowOnFailure(
                    uiShell.ShowMessageBox(
                        0,
                        ref clsid,
                        title,
                        (string.IsNullOrEmpty(text) ? null : text),
                        string.Empty,
                        0,
                        vsButtons,
                        oleDefButton,
                        vsIcon,
                        0, // false
                        out result));
            }

            return (DialogResult)result;
        }

        /// <summary>
        ///     The flag to indicate the type of documents returned by EnumerateOpenedDocuments.
        /// </summary>
        internal enum EnumerateDocumentsFlag
        {
            DirtyDocuments, // return all documents that are dirty
            DirtyOrPrimary, // return all documents that are dirty plus the primary document, even if it is not dirty
            DirtyExceptPrimary, // return all documents that are dirty except the primary document, even if it is dirty
            AllDocuments // return all documents associated with the editor, regardless of dirty state.
        };

        internal static EnumerateDocumentsFlag GetDesignerDocumentFlagFromSaveOption(__VSRDTSAVEOPTIONS saveOption)
        {
            // if ForceSave, make sure the primary document is in the list even if it is not dirty
            return ((saveOption & __VSRDTSAVEOPTIONS.RDTSAVEOPT_ForceSave) != 0)
                       ? EnumerateDocumentsFlag.DirtyOrPrimary
                       : EnumerateDocumentsFlag.DirtyDocuments;
        }

        internal static bool IsDirty(object docData)
        {
            var persistDocData = docData as IVsPersistDocData;
            if (persistDocData != null)
            {
                int dirty;
                NativeMethods.ThrowOnFailure(persistDocData.IsDocDataDirty(out dirty));
                return (dirty != 0);
            }

            return false;
        }

        /// <summary>
        ///     Returns the docdata for this cookie on the rdt
        /// </summary>
        internal static bool TryGetDocDataFromCookie(uint cookie, out object docData)
        {
            docData = null;
            var success = false;

            var rdt = Package.GetGlobalService(typeof(IVsRunningDocumentTable)) as IVsRunningDocumentTable;

            Debug.Assert(rdt != null);
            if (rdt != null)
            {
                IVsHierarchy hierarchy;
                uint rdtFlags;
                uint readLocks;
                uint editLocks;
                string itemName;
                uint itemId;
                var unknownDocData = IntPtr.Zero;

                try
                {
                    var hr = rdt.GetDocumentInfo(
                        cookie,
                        out rdtFlags,
                        out readLocks,
                        out editLocks,
                        out itemName,
                        out hierarchy,
                        out itemId,
                        out unknownDocData);

                    if (NativeMethods.Succeeded(hr))
                    {
                        docData = Marshal.GetObjectForIUnknown(unknownDocData);
                        success = true;
                    }
                }
                finally
                {
                    if (unknownDocData != IntPtr.Zero)
                    {
                        Marshal.Release(unknownDocData);
                    }
                }
            }

            return success;
        }
    }
}
