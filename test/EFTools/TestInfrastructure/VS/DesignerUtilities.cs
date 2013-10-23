// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure.VS
{
    using System;
    using System.Collections.Generic;
    using EnvDTE;

    public enum OperationType
    {
        UI,
        Model,
        MenuCommand,
        Keyboard,
        Custom
    }

    public class DesignerUtilities
    {
        #region Copy/cut/paste

        /// <summary>
        ///     Copy currently selected shapes onto the clipboard
        ///     Need to select the right shapes before calling this method.
        ///     The Copy menu item has to be available for this operation to succeed.
        /// </summary>
        /// <param name="dte">The DTE object</param>
        public static void Copy(DTE dte)
        {
            MenuAction.Invoke(dte, MenuAction.Copy);
        }

        /// <summary>
        ///     Paste shapes from the clipboard onto the currently selected shapes
        ///     Need to select the right shapes before calling this method.
        ///     The Paste menu item has to be available for this operation to succeed.
        /// </summary>
        /// <param name="dte">The DTE object</param>
        public static void Paste(DTE dte)
        {
            MenuAction.Invoke(dte, MenuAction.Paste);
        }

        #endregion

        /// <summary>
        /// Menu items and related operations
        /// </summary>
        public class MenuAction
        {
            #region MenuItem class

            /// <summary>
            ///     Represents a menu item
            /// </summary>
            public class MenuItem
            {
            }

            #endregion

            #region Menu items

            /// <summary>
            ///     All common menu items used in tests
            /// </summary>
            // Edit
            public static MenuItem Undo = new MenuItem();

            public static MenuItem Redo = new MenuItem();
            public static MenuItem Cut = new MenuItem();
            public static MenuItem Copy = new MenuItem();
            public static MenuItem Paste = new MenuItem();
            public static MenuItem PasteByReference = new MenuItem();
            public static MenuItem Delete = new MenuItem();

            //Sequence Designer
            public static MenuItem Movetodiagram = new MenuItem();
            public static MenuItem LayoutDiagram = new MenuItem();
            public static MenuItem ExportDiagramasImage = new MenuItem();
            public static MenuItem Reroute = new MenuItem();
            public static MenuItem ElementProperties = new MenuItem();

            // DSL
            public static MenuItem Validate = new MenuItem();
            public static MenuItem ValidateAll = new MenuItem();

            #endregion

            /// <summary>
            ///     A dictionary to keep the mapping from MenuItem to the key stroke sequence
            /// </summary>
            public static Dictionary<MenuItem, string> KeyStrokes = new Dictionary<MenuItem, string>
                {
                    { Copy, "+{F10}Y{Enter}" },
                    { Cut, "+{F10}T{Enter}" },
                    { Delete, "+{F10}D{Enter}" },
                    { Paste, "+{F10}P{Enter}" },
                    { Validate, "+{F10}V{Enter}" },
                    { ValidateAll, "+{F10}VV{Enter}" }
                };

            /// <summary>
            ///     A dictionary to keep the mapping from MenuItem to menu item string
            /// </summary>
            public static Dictionary<MenuItem, string> CommandStrings = new Dictionary<MenuItem, string>
                {
                    // Edit
                    { Undo, "Edit.Undo" },
                    { Redo, "Edit.Redo" },
                    { Cut, "Edit.Cut" },
                    { Copy, "Edit.Copy" },
                    { Paste, "Edit.Paste" },
                    { PasteByReference, "ArchitectureDesigner.PasteReference" },
                    { Delete, "Edit.Delete" },
                    //Sequence Designer
                    { Movetodiagram, "ArchitectureDesigner.Sequence.MovetoDiagram" },
                    { LayoutDiagram, "ArchitectureDesigner.RearrangeLayout" },
                    { ExportDiagramasImage, "ArchitectureDesigner.Sequence.ExportDiagramasImage" },
                    { Reroute, "DSLTools.Reroute" },
                    { ElementProperties, "Diagram.Properties" },
                    // DSL
                    { Validate, "DSLTools.Validate" },
                    { ValidateAll, "DSLTools.ValidateAll" }
                };

            /// <summary>
            ///     Check if the given menu item is currently available
            /// </summary>
            /// <param name="dte">The DTE object</param>
            /// <param name="menuItem">The menu item to check</param>
            /// <returns>true if the menu item is currently available, false otherwise</returns>
            public static bool IsMenuItemAvailable(DTE dte, MenuItem menuItem)
            {
                var menuItemName = CommandStrings[menuItem];
                foreach (Command command in dte.Commands)
                {
                    if (string.Equals(menuItemName, command.Name, StringComparison.Ordinal))
                    {
                        return command.IsAvailable;
                    }
                }
                return false;
            }

            /// <summary>
            ///     Invoke menu action for the given menu item
            /// </summary>
            /// <param name="dte">The DTE object</param>
            /// <param name="menuItem">The menu item</param>
            public static void Invoke(DTE dte, MenuItem menuItem)
            {
                Invoke(dte, menuItem, OperationType.MenuCommand);
            }

            /// <summary>
            ///     Invoke menu action for the given menu item using specified operation type
            /// </summary>
            /// <param name="dte">The DTE object</param>
            /// <param name="menuItem">The menu item</param>
            /// <param name="operationType">The type of the operation, i.e., via menu command or keyboard</param>
            public static void Invoke(DTE dte, MenuItem menuItem, OperationType operationType)
            {
                switch (operationType)
                {
                    case OperationType.MenuCommand:
                        dte.ExecuteCommand(CommandStrings[menuItem], string.Empty);
                        break;
                    case OperationType.Keyboard:
                        // TODO: This is not stable right now and might cause rolling build failure.
                        // So it is just a placeholder for now.
                        // SendKeys.SendWait(KeyStrokes[menuItem]);
                        // TODO: throwing an exception if not used should be removed
                        // otherwise fix verify and remove the other TODO
                        throw new NotSupportedException("OperationType");
                    default:
                        throw new NotSupportedException("OperationType");
                }
            }
        }
    }
}
