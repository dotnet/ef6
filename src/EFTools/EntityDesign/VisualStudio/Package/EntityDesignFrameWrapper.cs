// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System.IO;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.VisualStudio.Shell.Interop;

    internal class EntityDesignFrameWrapper : FrameWrapper
    {
        private enum EditorTypes
        {
            NonEscher,
            XmlEditor,
            Escher
        }

        internal EntityDesignFrameWrapper(IVsWindowFrame frame)
            : base(frame)
        {
        }

        private EditorTypes EditorType
        {
            get
            {
                if (_frame != null)
                {
                    var editor = Editor;
                    if (editor == CommonPackageConstants.xmlEditorGuid
                        || editor == CommonPackageConstants.xmlEditorGuid2)
                    {
                        return EditorTypes.XmlEditor;
                    }
                    if (editor == PackageConstants.guidEscherEditorFactory)
                    {
                        return EditorTypes.Escher;
                    }
                }
                return EditorTypes.NonEscher;
            }
        }

        internal bool IsEscherDocument
        {
            get
            {
                if (_frame != null)
                {
                    var uri = Uri;
                    if (uri != null)
                    {
                        return VSArtifact.GetVSArtifactFileExtensions().Contains(Path.GetExtension(uri.LocalPath));
                    }
                }
                return false;
            }
        }

        internal override bool ShouldShowToolWindows
        {
            get
            {
                if (_frame != null
                    && IsEscherDocument
                    && EditorType == EditorTypes.Escher)
                {
                    var artifact = PackageManager.Package.ModelManager.GetArtifact(Uri) as EntityDesignArtifact;

                    // artifact may be null during shutdown
                    if (artifact != null)
                    {
                        if (artifact.IsDesignerSafeAndEditSafe())
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        internal override bool IsDesignerDocInDesigner
        {
            get { return IsEscherDocInEntityDesigner; }
        }

        internal bool IsEscherDocInEntityDesigner
        {
            get { return _frame != null && IsEscherDocument && EditorType == EditorTypes.Escher; }
        }

        internal override bool IsDesignerDocInXmlEditor
        {
            get { return IsEscherDocInXmlEditor; }
        }

        internal bool IsEscherDocInXmlEditor
        {
            get { return _frame != null && IsEscherDocument && EditorType == EditorTypes.XmlEditor; }
        }
    }
}
