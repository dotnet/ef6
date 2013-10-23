// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    ///     Return data for the IBranch.OnStartDrag method
    /// </summary>
    [Serializable]
    internal struct VirtualTreeStartDragData
    {
        private readonly object myData;
        private readonly DragDropEffects myAllowedEffects;

        /// <summary>
        ///     Begin a new drag operation with the given data and effects
        /// </summary>
        /// <param name="data">The data for the drag operation</param>
        /// <param name="allowedEffects">Effects allowed on the given data</param>
        public VirtualTreeStartDragData(object data, DragDropEffects allowedEffects)
        {
            myData = data;
            myAllowedEffects = allowedEffects;
        }

        /// <summary>
        ///     A standard value for an Empty drag operation. Return VirtualTreeStartDragData.Empty to stop the attempted drag operation.
        /// </summary>
        public static readonly VirtualTreeStartDragData Empty = new VirtualTreeStartDragData(null, DragDropEffects.None);

        /// <summary>
        ///     A standard value for allowing a cut or copy operation. Return VirtualTreeStartDragData.Empty in response to a
        ///     DragReason.CanCut or DragReason.CanCopy request if the indicated item can be cut or copied.  Otherwise, return
        ///     VirtualTreeStartDragData.Empty.
        /// </summary>
        public static readonly VirtualTreeStartDragData AllowCutCopy = new VirtualTreeStartDragData(new object(), DragDropEffects.None);

        /// <summary>
        ///     The data object for the drag operation
        /// </summary>
        public object Data
        {
            get { return myData; }
        }

        /// <summary>
        ///     True if the current drag data is empty, or no effects are allowed. Do not continue with the drag operation.
        /// </summary>
        public bool IsEmpty
        {
            get { return myData == null || myAllowedEffects == DragDropEffects.None; }
        }

        /// <summary>
        ///     The drag effects supported by this drag object.
        /// </summary>
        /// <value>System.Windows.Forms.DragDropEffects</value>
        public DragDropEffects AllowedEffects
        {
            get { return myAllowedEffects; }
        }

        #region Equals override and related functions

        /// <summary>
        ///     Equals override. Defers to Compare function.
        /// </summary>
        /// <param name="obj">An item to compare to this object</param>
        /// <returns>True if the items are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj is VirtualTreeStartDragData)
            {
                return Compare(this, (VirtualTreeStartDragData)obj);
            }
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
        /// <returns></returns>
        public static bool operator ==(VirtualTreeStartDragData operand1, VirtualTreeStartDragData operand2)
        {
            return Compare(operand1, operand2);
        }

        /// <summary>
        ///     Compare two VirtualTreeStartDragData structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>true if operands are equal</returns>
        public static bool Compare(VirtualTreeStartDragData operand1, VirtualTreeStartDragData operand2)
        {
            return operand1.myData == operand2.myData && operand1.myAllowedEffects == operand2.myAllowedEffects;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns></returns>
        public static bool operator !=(VirtualTreeStartDragData operand1, VirtualTreeStartDragData operand2)
        {
            return !Compare(operand1, operand2);
        }

        #endregion // Equals override and related functions
    }
}
