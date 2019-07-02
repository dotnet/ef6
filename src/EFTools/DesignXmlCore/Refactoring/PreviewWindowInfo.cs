// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     This class contains all preview data for an RefactorOperation.
    ///     Preview dialog will use all information in this class to populate the dialog.
    /// </summary>
    internal sealed class PreviewWindowInfo
    {
        #region Private Members

        #endregion

        /// <summary>
        ///     Apply button text
        /// </summary>
        public string ConfirmButtonText { get; set; }

        /// <summary>
        ///     Description of this RefactorOperation.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Help context for this RefactorOperation.
        /// </summary>
        public string HelpContext { get; set; }

        /// <summary>
        ///     The text view description, that appears on the header of the
        ///     text view in preview dialog.
        /// </summary>
        public string TextViewDescription { get; set; }

        /// <summary>
        ///     Preview dialog title for this RefactorOperation
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        ///     Warning text in the preview dialog for this RefactorOperation.
        /// </summary>
        public string Warning { get; set; }

        /// <summary>
        ///     Warning level for this RefactorOperation.
        /// </summary>
        public __PREVIEWCHANGESWARNINGLEVEL WarningLevel { get; set; }
    }
}
