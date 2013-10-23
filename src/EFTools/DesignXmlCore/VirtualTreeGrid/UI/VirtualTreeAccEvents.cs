// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VisualStudio;

    /// <summary>
    ///     Accessible WinEvent support for Diagrams.
    /// </summary>
    internal partial class VirtualTreeControl
    {
        internal sealed class VirtualTreeAccEvents
        {
            private int nextIndex;
            private const int maxEntries = 64;
            private readonly AccessibleObjectEntry[] array;
            private static VirtualTreeAccEvents instance;

            #region Singleton code

            private VirtualTreeAccEvents()
            {
                array = new AccessibleObjectEntry[maxEntries];

                for (var i = 0; i < maxEntries; ++i)
                {
                    array[i] = new AccessibleObjectEntry(i);
                }
            }

            /// <summary>
            ///     A internal object used for synchronization.
            /// </summary>
            private static object internalSyncObject;

            /// <summary>
            ///     Gets the internal object used for synchronization.
            /// </summary>
            private static object InternalSyncObject
            {
                get
                {
                    if (internalSyncObject == null)
                    {
                        var o = new object();
                        Interlocked.CompareExchange(ref internalSyncObject, o, null);
                    }
                    return internalSyncObject;
                }
            }

            private static VirtualTreeAccEvents Instance
            {
                get
                {
                    if (instance == null)
                    {
                        lock (InternalSyncObject)
                        {
                            if (instance == null)
                            {
                                instance = new VirtualTreeAccEvents();
                            }
                        }
                    }
                    return instance;
                }
            }

            #endregion // Singleton code

            #region Event constants

            /// <summary>
            ///     An object's KeyboardShortcut property has changed.
            /// </summary>
            public const int eventObjectAcceleratorChange = NativeMethods.EVENT_OBJECT_ACCELERATORCHANGE;

            /// <summary>
            ///     An object has been created.
            /// </summary>
            public const int eventObjectCreate = NativeMethods.EVENT_OBJECT_CREATE;

            /// <summary>
            ///     An object's DefaultAction property has changed.
            /// </summary>
            public const int eventObjectDefaultActionChange = NativeMethods.EVENT_OBJECT_DEFACTIONCHANGE;

            /// <summary>
            ///     An object's Description property has changed.
            /// </summary>
            public const int eventObjectDescriptionChange = NativeMethods.EVENT_OBJECT_DESCRIPTIONCHANGE;

            /// <summary>
            ///     An object has been destroyed.
            /// </summary>
            public const int eventObjectDestroy = NativeMethods.EVENT_OBJECT_DESTROY;

            /// <summary>
            ///     An object has received the keyboard focus.
            /// </summary>
            public const int eventObjectFocus = NativeMethods.EVENT_OBJECT_FOCUS;

            /// <summary>
            ///     An object's Help property has changed.
            /// </summary>
            public const int eventObjectHelpChange = NativeMethods.EVENT_OBJECT_HELPCHANGE;

            /// <summary>
            ///     An object is hidden.
            /// </summary>
            public const int eventObjectHide = NativeMethods.EVENT_OBJECT_HIDE;

            /// <summary>
            ///     A DiagramItem's location and/or size has changed.
            /// </summary>
            public const int eventObjectLocationChange = NativeMethods.EVENT_OBJECT_LOCATIONCHANGE;

            /// <summary>
            ///     An object's Name property has changed.
            /// </summary>
            public const int eventObjectNameChange = NativeMethods.EVENT_OBJECT_NAMECHANGE;

            /// <summary>
            ///     An object has a new parent object.
            /// </summary>
            public const int eventObjectParentChange = NativeMethods.EVENT_OBJECT_PARENTCHANGE;

            /// <summary>
            ///     A container object has added, removed, or reordered its children.
            /// </summary>
            public const int eventObjectReorder = NativeMethods.EVENT_OBJECT_REORDER;

            /// <summary>
            ///     The selection within a container object has changed.
            /// </summary>
            public const int eventObjectSelection = NativeMethods.EVENT_OBJECT_SELECTION;

            /// <summary>
            ///     An item within a container object has been added to the selection.
            /// </summary>
            public const int eventObjectSelectionAdd = NativeMethods.EVENT_OBJECT_SELECTIONADD;

            /// <summary>
            ///     An item within a container object has been removed from the selection.
            /// </summary>
            public const int eventObjectSelectionRemove = NativeMethods.EVENT_OBJECT_SELECTIONREMOVE;

            /// <summary>
            ///     Numerous selection changes have occurred within a container object.
            /// </summary>
            public const int eventObjectSelectionWithin = NativeMethods.EVENT_OBJECT_SELECTIONWITHIN;

            /// <summary>
            ///     A hidden object is shown.
            /// </summary>
            public const int eventObjectShow = NativeMethods.EVENT_OBJECT_SHOW;

            /// <summary>
            ///     An object's state has changed.
            /// </summary>
            public const int eventObjectStateChange = NativeMethods.EVENT_OBJECT_STATECHANGE;

            /// <summary>
            ///     An object's Value property has changed.
            /// </summary>
            public const int eventObjectValueChange = NativeMethods.EVENT_OBJECT_VALUECHANGE;

            #endregion // Event constants

            #region Accessibility Event Methods

            /// <summary>
            ///     Call to test if the Notify method needs to be called. ShouldNotify should
            ///     always be called before Notify.
            /// </summary>
            /// <param name="accessibilityEvent">One of the predefined EVENT_OBJECT_* or EVENT_SYSTEM_* constants.</param>
            /// <param name="treeControl">The tree control that generated the event</param>
            /// <returns>true if Notify should be called</returns>
            public static bool ShouldNotify(int accessibilityEvent, VirtualTreeControl treeControl)
            {
                if (treeControl.GetStateFlag(VTCStateFlags.ReturnedAccessibilityObject))
                {
                    // IsWinEventHookInstalled not available on W2k
                    return NativeMethods.WinXPOrHigher ? (0 != NativeMethods.IsWinEventHookInstalled(accessibilityEvent)) : true;
                }
                return false;
            }

            /// <summary>
            ///     Signals the system that an accessibility event occurred.
            ///     The notification will be sent for the specified view only.
            /// </summary>
            /// <param name="accessibilityEvent">One of the predefined EVENT_OBJECT_* or EVENT_SYSTEM_* constants.</param>
            /// <param name="row">row index for item.</param>
            /// <param name="column">column index for item.</param>
            /// <param name="treeControl">The VirtualTreeControl that contains the object that generated the event.</param>
            public static void Notify(int accessibilityEvent, int row, int column, VirtualTreeControl treeControl)
            {
                Debug.Assert(ShouldNotify(accessibilityEvent, treeControl));
                if ((treeControl == null)
                    || (row < 0)
                    || (column < 0))
                {
                    return;
                }

                // Add the accessibleObject to the circular array and generate an ID for it.
                var accessibleObjectId = Instance.InternalAddObject(treeControl, row, column);

                // Create a HandleRef object for the hwnd.
                // (A HandleRef structure wraps a managed object holding a 
                // handle to a resource that is passed to unmanaged code.
                // Wrapping a handle with HandleRef guarantees that the managed 
                // object is not garbage collected until the platform invoke call completes.)
                var viewHandleRef = new HandleRef(treeControl, treeControl.Handle);

                // Notify the system of the event.
                NativeMethods.NotifyWinEvent(
                    accessibilityEvent, // Specifies the event that occurred
                    viewHandleRef, // Handle to the window that contains the object that generated the event.
                    accessibleObjectId, // Generated object ID that uniquely identifies the accessibleObject (for use by WM_GETOBJECT).
                    NativeMethods.CHILDID_SELF); // CHILDID_SELF means the event was generated by the object itself, not a child object.
            }

            /// <summary>
            ///     Retrieves the AccessibleObject corresponding to the specified id.
            ///     This method is intended to implement WM_GETOBJECT, where the
            ///     object identifier is provided by the LPARAM.
            /// </summary>
            /// <param name="accessibleObjectId">
            ///     The id that uniquely identifies the accessible object.
            ///     This is the LPARAM value of the WM_GETOBJECT message.
            /// </param>
            /// <returns>
            ///     An AccessibleObject corresponding to the id, or
            ///     null if the accessible object no longer exists.
            ///     (Existence of the accessible object does not
            ///     reflect the existence of the object that it represents.
            /// </returns>
            internal static AccessibleObject GetObject(int accessibleObjectId)
            {
                if (accessibleObjectId >= 0)
                {
                    return Instance.InternalGetObject(accessibleObjectId);
                }
                else
                {
                    return null;
                }
            }

            private int InternalAddObject(VirtualTreeControl treeControl, int row, int column)
            {
                array[nextIndex].SetObjectData(treeControl, row, column);

                var id = array[nextIndex].Id;
                if (nextIndex >= array.GetUpperBound(0))
                {
                    nextIndex = array.GetLowerBound(0);
                }
                else
                {
                    nextIndex++;
                }
                return id;
            }

            private AccessibleObject InternalGetObject(int accessibleObjectId)
            {
                // (The index is in the higher-order word (two bytes)).
                var index = accessibleObjectId >> 16;
                if (index >= array.GetLowerBound(0)
                    && index <= array.GetUpperBound(0))
                {
                    if (array[index].Id == accessibleObjectId)
                    {
                        // todo: create the accessible object based on row, column data.
                        var entry = array[index];
                        if (entry.TreeControl != null)
                        {
                            return entry.TreeControl.GetAccessibleObject(entry.Row, entry.Column, false);
                        }
                    }
                }
                return null;
            }

            #endregion // Accessibility Event Methods

            #region AccessibleObjectEntry struct

            private struct AccessibleObjectEntry
            {
                private readonly int _index;
                private int _entryReuseCounter;
                private VirtualTreeControl _treeControl;
                private int _row;
                private int _column;

                public AccessibleObjectEntry(int index)
                {
                    // empty entry
                    _index = index;
                    _entryReuseCounter = 0;
                    _treeControl = null;
                    _row = 0;
                    _column = 0;
                }

                public int Id
                {
                    get { return (_index << 16) + _entryReuseCounter; }
                }

                public void SetObjectData(VirtualTreeControl treeControl, int row, int column)
                {
                    _treeControl = treeControl;
                    _row = row;
                    _column = column;

                    if (_entryReuseCounter >= Int16.MaxValue)
                    {
                        _entryReuseCounter = 0;
                    }
                    else
                    {
                        _entryReuseCounter++;
                    }
                }

                public int Row
                {
                    get { return _row; }
                }

                public int Column
                {
                    get { return _column; }
                }

                public VirtualTreeControl TreeControl
                {
                    get { return _treeControl; }
                }
            }

            #endregion
        }
    }
}
