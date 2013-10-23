// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Data returned from an IBranch.BeginLabelEdit method call.
    /// </summary>
    internal struct VirtualTreeLabelEditData
    {
        private string myAlternateText;
        private int myMaxTextLength;
        private object myCustomInPlaceEdit;
        private readonly bool myIsValid;
        private readonly bool myDeferActivation;
        private CommitLabelEditCallback myCustomCallback;

        private VirtualTreeLabelEditData(bool isValid, bool deferActivation)
        {
            myIsValid = isValid;
            myDeferActivation = deferActivation;
            myAlternateText = null;
            myCustomInPlaceEdit = null;
            myCustomCallback = null;
            myMaxTextLength = 0;
        }

        /// <summary>
        ///     No data is available, return from IBranch.BeginLabelEdit to cancel the label edit.
        /// </summary>
        public static readonly VirtualTreeLabelEditData Invalid = new VirtualTreeLabelEditData(false, false);

        /// <summary>
        ///     No data is available for immediate activation, but the control should be activated
        ///     when the standard edit timer fires.
        /// </summary>
        public static readonly VirtualTreeLabelEditData DeferActivation = new VirtualTreeLabelEditData(true, true);

        /// <summary>
        ///     Return from IBranch.BeginLabelEdit to use the default editing settings.
        /// </summary>
        public static readonly VirtualTreeLabelEditData Default = new VirtualTreeLabelEditData(true, false);

        /// <summary>
        ///     Create new data for beginning a label edit.
        /// </summary>
        public VirtualTreeLabelEditData(object customInPlaceEdit)
            : this(customInPlaceEdit, null, null, 0)
        {
        }

        /// <summary>
        ///     Create new data for beginning a label edit.
        /// </summary>
        /// <param name="customInPlaceEdit">An alternate control instance or type for editing. Specify null for a standard edit control.</param>
        /// <param name="customCommit">An function to call on label commit. Specify null to use IBranch.CommitLabelEdit</param>
        public VirtualTreeLabelEditData(object customInPlaceEdit, CommitLabelEditCallback customCommit)
            : this(customInPlaceEdit, customCommit, null, 0)
        {
        }

        /// <summary>
        ///     Create new data for beginning a label edit.
        /// </summary>
        /// <param name="alternateText">An alternate text. Specify null to use IBranch.GetText</param>
        public VirtualTreeLabelEditData(string alternateText)
            : this(null, null, alternateText, 0)
        {
        }

        /// <summary>
        ///     Create new data for beginning a label edit.
        /// </summary>
        /// <param name="alternateText">An alternate text. Specify null to use IBranch.GetText</param>
        /// <param name="maxTextLength">A maximum text length, or 0 for unbounded.</param>
        public VirtualTreeLabelEditData(string alternateText, int maxTextLength)
            : this(null, null, alternateText, maxTextLength)
        {
        }

        /// <summary>
        ///     Create new data for beginning a label edit.
        /// </summary>
        /// <param name="customInPlaceEdit">An alternate control instance or type for editing. Specify null for a standard edit control.</param>
        /// <param name="customCommit">An function to call on label commit. Specify null to use IBranch.CommitLabelEdit</param>
        /// <param name="alternateText">An alternate text. Specify null to use IBranch.GetText</param>
        /// <param name="maxTextLength">A maximum text length, or 0 for unbounded.</param>
        public VirtualTreeLabelEditData(
            object customInPlaceEdit, CommitLabelEditCallback customCommit, string alternateText, int maxTextLength)
        {
            myIsValid = true;
            myDeferActivation = false;
            myCustomInPlaceEdit = customInPlaceEdit;
            myCustomCallback = customCommit;
            myAlternateText = alternateText;
            myMaxTextLength = maxTextLength;
        }

        /// <summary>
        ///     Return true if this is a valid item. Use VirtualTreeLabelEditData.Invalid to stop label activation.
        /// </summary>
        public bool IsValid
        {
            get { return myIsValid; }
        }

        /// <summary>
        ///     Set to true for the structure returned by VirtualTreeLabelEditData.DeferredActivation. Return
        ///     this value from IBranch.BeginLabelEdit when immediateActivationRequest is true to defer label
        ///     editing to the normal edit timing.
        /// </summary>
        public bool ActivationDeferred
        {
            get { return myDeferActivation; }
        }

        /// <summary>
        ///     Set to String.Empty to display an empty string. Set to null to use the default text.
        /// </summary>
        public string AlternateText
        {
            get { return myAlternateText; }
            set { myAlternateText = value; }
        }

        /// <summary>
        ///     Set to the maximum number of characters to allow in the edit window, or 0 for unbounded.
        /// </summary>
        public int MaxTextLength
        {
            get { return myMaxTextLength; }
            set { myMaxTextLength = value; }
        }

        /// <summary>
        ///     Return null to use the default editor, a System.Type that implements IVirtualTreeInPlaceControl, or
        ///     a pre-instantiated instance of an appropriate type.
        /// </summary>
        public object CustomInPlaceEdit
        {
            get { return myCustomInPlaceEdit; }
            set { myCustomInPlaceEdit = value; }
        }

        /// <summary>
        ///     Provide an alternative callback location to IBranch.CommitLabelEdit.
        /// </summary>
        public CommitLabelEditCallback CustomCommit
        {
            get { return myCustomCallback; }
            set { myCustomCallback = value; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     GetHashCode override
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            // We're forced to override this with the Equals override.
            return base.GetHashCode();
        }

        /// <summary>
        ///     Equals operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns false, there is no need to compare VirtualTreeLabelEditData structures.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator ==(VirtualTreeLabelEditData operand1, VirtualTreeLabelEditData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Compare two VirtualTreeLabelEditData structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns false, there is no need to compare VirtualTreeLabelEditData structures.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool Compare(VirtualTreeLabelEditData operand1, VirtualTreeLabelEditData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns true, there is no need to compare VirtualTreeLabelEditData structures.</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator !=(VirtualTreeLabelEditData operand1, VirtualTreeLabelEditData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return true;
        }

        #endregion Equals override and related functions
    }
}
