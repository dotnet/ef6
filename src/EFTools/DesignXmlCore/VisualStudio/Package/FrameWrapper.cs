// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    internal abstract class FrameWrapper
    {
        protected IVsWindowFrame _frame;

        protected FrameWrapper(IVsWindowFrame frame)
        {
            _frame = frame;
        }

        public override bool Equals(object obj)
        {
            if (_frame == null
                && obj == null)
            {
                return true;
            }

            var frameWrapper2 = obj as FrameWrapper;
            if (frameWrapper2 != null)
            {
                return _frame == frameWrapper2._frame;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (_frame != null)
            {
                return _frame.GetHashCode();
            }
            return 0;
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal Uri Uri
        {
            get
            {
                if (_frame != null)
                {
                    object value;
                    if (_frame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out value) == NativeMethods.S_OK)
                    {
                        var filename = value as string;
                        if (filename != null)
                        {
                            try
                            {
                                return new Uri(filename);
                            }
                            catch (Exception)
                            {
                                // numerous exceptions could occur here since the moniker property of the frame doesn't have
                                // to be in the URI format. There could also be security exceptions, FNF exceptions, etc.
                                return null;
                            }
                        }
                    }
                }
                return null;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame.GetGuidProperty(System.Int32,System.Guid@)")]
        protected Guid Editor
        {
            get
            {
                var editorGuid = Guid.Empty;
                if (_frame != null)
                {
                    _frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_guidEditorType, out editorGuid);
                }
                return editorGuid;
            }
        }

        internal abstract bool ShouldShowToolWindows { get; }
        internal abstract bool IsDesignerDocInDesigner { get; }
        internal abstract bool IsDesignerDocInXmlEditor { get; }

        internal IVsTextView TextView
        {
            get
            {
                if (_frame != null)
                {
                    object value;
                    NativeMethods.ThrowOnFailure(_frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out value));
                    var codeWindow = value as IVsCodeWindow;
                    if (codeWindow != null)
                    {
                        IVsTextView textView;
                        var hr = codeWindow.GetLastActiveView(out textView);
                        if (!NativeMethods.Succeeded(hr) || textView == null)
                        {
                            textView = VsShellUtilities.GetTextView(_frame);
                        }
                        return textView;
                    }
                }
                return null;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsWindowFrame.Show")]
        internal void Show()
        {
            if (_frame != null)
            {
                _frame.Show();
            }
        }

        internal bool IsDocumentOpen(IServiceProvider sp)
        {
            if (sp != null)
            {
                var uri = Uri;
                if (uri != null)
                {
                    IVsUIHierarchy hier;
                    uint itemId;
                    IVsWindowFrame frame;
                    if (VsShellUtilities.IsDocumentOpen(sp, uri.LocalPath, Guid.Empty, out hier, out itemId, out frame))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
