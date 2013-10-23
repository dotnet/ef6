// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;

    #region ContextMenuEvent delegate and argument definitions

    /// <summary>
    ///     The function prototype for handling a context menu click event
    /// </summary>
    /// <param name="sender">The control to show the context menu in</param>
    /// <param name="e">Arguments describing the event</param>
    internal delegate void ContextMenuEventHandler(object sender, ContextMenuEventArgs e);

    /// <summary>
    ///     Event arguments generated for a context menu event. Users can get the item
    ///     clicked on by using ScreenToClient to convert to client coordinates, followed
    ///     by the VirtualTreeControl.HitInfo method to get the item that was clicked on,
    ///     followed by VirtualTreeControl.Tree.GetItemInfo method with the Row and NativeColumn
    ///     properties of the HitInfo structure.
    /// </summary>
    internal class ContextMenuEventArgs : EventArgs
    {
        private readonly int myX;
        private readonly int myY;

        /// <summary>
        ///     Create new context menu arguments at the given screen coordinates
        /// </summary>
        /// <param name="x">The x (horizontal) position in screen coordinates</param>
        /// <param name="y">The y (vertical) position in screen coordinates</param>
        public ContextMenuEventArgs(int x, int y)
        {
            myX = x;
            myY = y;
        }

        /// <summary>
        ///     The x (horizontal) position in screen coordinates
        /// </summary>
        public int X
        {
            get { return myX; }
        }

        /// <summary>
        ///     The y (vertical) position in screen coordinates
        /// </summary>
        public int Y
        {
            get { return myY; }
        }
    }

    #endregion

    #region DoubleClickEvent delegate and argument definitions

    /// <summary>
    ///     The function prototype for handling a double click event
    /// </summary>
    /// <param name="sender">The control that was clicked on</param>
    /// <param name="e">Arguments describing the event</param>
    internal delegate void DoubleClickEventHandler(object sender, DoubleClickEventArgs e);

    /// <summary>
    ///     Event arguments generated for a double click event
    /// </summary>
    internal class DoubleClickEventArgs : EventArgs
    {
        private VirtualTreeHitInfo myHitInfo;
        private VirtualTreeItemInfo myItemInfo;
        private readonly int myX;
        private readonly int myY;
        private readonly MouseButtons myButton;
        private bool myHaveItemInfo;
        private readonly VirtualTreeControl myParent;

        /// <summary>
        ///     Create a new DoubleClickEventArgs in response to a double click event in the VirtualTreeControl
        /// </summary>
        /// <param name="parent">The control that was double clicked</param>
        /// <param name="button">The mouse button that was clicked</param>
        /// <param name="x">The x (horizontal) coordinate of the click</param>
        /// <param name="y">The y (vertical) coordinate of the click</param>
        public DoubleClickEventArgs(VirtualTreeControl parent, MouseButtons button, int x, int y)
        {
            myX = x;
            myY = y;
            myParent = parent;
            myButton = button;
        }

        /// <summary>
        ///     Property indicating if the double click event was handled. Set this property to true
        ///     from and event handler to stop the control from taking default action on the doudble click.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        ///     The X (horizontal) coordinate (in client coordinates) of the click event
        /// </summary>
        public int X
        {
            get { return myX; }
        }

        /// <summary>
        ///     The Y (vertical) coordinate (in client coordinates) of the click event
        /// </summary>
        public int Y
        {
            get { return myY; }
        }

        /// <summary>
        ///     Retrieve the mouse button used to double click.
        /// </summary>
        public MouseButtons Button
        {
            get { return myButton; }
        }

        /// <summary>
        ///     Retrieve the VirtualTreeHitInfo of the item that was clicked on. This tells you
        ///     what part of the item was clicked on, while the ItemInfo retrieves information
        ///     about the contents of the item.
        /// </summary>
        public VirtualTreeHitInfo HitInfo
        {
            get
            {
                if (myHitInfo.HitTarget == VirtualTreeHitTargets.Uninitialized)
                {
                    myHitInfo = myParent.HitInfo(myX, myY);
                }
                return myHitInfo;
            }
        }

        /// <summary>
        ///     Retrieve the VirtualTreeItemInfo corresponding to the item that was double clicked. This
        ///     item corresponds to the anchor item if the user clicked in a blank expansion. To get information
        ///     about where an item was clicked, use the HitInfo property.
        /// </summary>
        public VirtualTreeItemInfo ItemInfo
        {
            get
            {
                if (!myHaveItemInfo)
                {
                    myHaveItemInfo = true;
                    if (myHitInfo.HitTarget == VirtualTreeHitTargets.Uninitialized)
                    {
                        myHitInfo = myParent.HitInfo(myX, myY);
                        Debug.Assert(myHitInfo.HitTarget != VirtualTreeHitTargets.Uninitialized);
                    }
                    var row = -1;
                    var column = 0;
                    if (0 != (myHitInfo.HitTarget & VirtualTreeHitTargets.OnItemRegion))
                    {
                        // Anywhere in the item is fine. The default routine (toggle expansion)
                        // will ignore a double click on the button region.
                        row = myHitInfo.Row;
                        column = myHitInfo.NativeColumn;
                    }
                    if (row != -1)
                    {
                        // Flags are required for the default toggle expansion routine,
                        // go ahead and grab them for all cases.
                        myItemInfo = myParent.Tree.GetItemInfo(row, column, true);
                    }
                    else
                    {
                        myItemInfo = new VirtualTreeItemInfo(null, -1, 0, -1);
                    }
                }
                return myItemInfo;
            }
        }
    }

    #endregion
}
