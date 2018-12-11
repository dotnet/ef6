// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System.Collections;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Forms;

    /// <summary>
    ///     An enum describing different pieces of information that the
    ///     tree control can automatically add to your name and description fields
    ///     for accessibility readers. Some of this information is available from
    ///     the branch itself, while other pieces (such as the current position in
    ///     the tree) are not available to the provider, but the provide can specify
    ///     how they want this information to be merged into their accessibility strings
    ///     using the VirtualTreeAccessibilityData structure.
    /// </summary>
    internal enum AccessibilityReplacementField
    {
        /// <summary>
        ///     A replacement field is not specified
        /// </summary>
        None,

        /// <summary>
        ///     Insert the replacement text from the header for the current column
        /// </summary>
        ColumnHeader,

        /// <summary>
        ///     Insert the base 0 number of the global row for this item
        /// </summary>
        GlobalRow0,

        /// <summary>
        ///     Insert the base 1 number of the global row for this item
        /// </summary>
        GlobalRow1,

        /// <summary>
        ///     Insert the base 0 'row n' text of the global row for this item
        /// </summary>
        GlobalRowText0,

        /// <summary>
        ///     Insert the base 1 'row n' text of the global row for this item
        /// </summary>
        GlobalRowText1,

        /// <summary>
        ///     Insert the base 0 number of the global column for this item
        /// </summary>
        GlobalColumn0,

        /// <summary>
        ///     Insert the base 1 number of the global column for this item
        /// </summary>
        GlobalColumn1,

        /// <summary>
        ///     Insert the base 0 'column n' text of the global column for this item
        /// </summary>
        GlobalColumnText0,

        /// <summary>
        ///     Insert the base 1 'column n' text of the global column for this item
        /// </summary>
        GlobalColumnText1,

        /// <summary>
        ///     Insert the base 0 'row n column m' text for this item
        /// </summary>
        GlobalRowAndColumnText0,

        /// <summary>
        ///     Insert the base 1 'row n column m' text for this item
        /// </summary>
        GlobalRowAndColumnText1,

        /// <summary>
        ///     Insert the base 0 number of the local row for this item
        /// </summary>
        LocalRow0,

        /// <summary>
        ///     Insert the base 0 number of the local row for this item
        /// </summary>
        LocalRow1,

        /// <summary>
        ///     Insert the base 0 'row n' text of the local row for this item
        /// </summary>
        LocalRowText0,

        /// <summary>
        ///     Insert the base 1 'row n' text of the local row for this item
        /// </summary>
        LocalRowText1,

        /// <summary>
        ///     Insert the base 1 'row n of total' text of the local row for this item
        /// </summary>
        LocalRowOfTotal,

        /// <summary>
        ///     Insert the number of direct descendants below this item.
        /// </summary>
        ChildRowCount,

        /// <summary>
        ///     Insert the 'n child rows' text for the number of direct descendants below this item.
        /// </summary>
        ChildRowCountText,

        /// <summary>
        ///     Insert the number of cells attached to this item, including the first column.
        /// </summary>
        ColumnCount,

        /// <summary>
        ///     Insert the 'n columns' text of cells attached to this item, including the first column.
        /// </summary>
        ColumnCountText,

        /// <summary>
        ///     Insert the text retrieved from IBranch.GetText(row, column)
        /// </summary>
        DisplayText,

        /// <summary>
        ///     Insert the text retrieved from IBranch.GetTipText(row, column, ToolTipType.Icon)
        /// </summary>
        ImageTipText,

        /// <summary>
        ///     Insert the text associated with the ImageDescriptions array in the  VirtualTreeAccessibilityData.
        ///     The index to use is retrieved from VirtualTreeDisplayData.Image, and the text must correspond
        ///     to an image list returned by that structure. If a custom image list is not returned, the image
        ///     data is retrieved from the control instead of the ImageDescriptions property.
        /// </summary>
        PrimaryImageText,

        /// <summary>
        ///     Similar to PrimaryImageText, except overlay indices are also recognized.
        ///     The index to use is retrieved from VirtualTreeDisplayData.Image and combined in a delimited
        ///     list with the text for any specified overlay indices.
        /// </summary>
        PrimaryImageAndOverlaysText,

        /// <summary>
        ///     Insert the text associated with the StateImageDescriptions array in the  VirtualTreeAccessibilityData.
        ///     The index to use is retrieved from VirtualTreeDisplayData.Image, and the text must correspond
        ///     to an image list returned by that structure. If a custom image list is not returned, the image
        ///     data is retrieved from the control instead of the ImageDescriptions property.
        /// </summary>
        StateImageText,
    }

    /// <summary>
    ///     Structure returned by IBranch.GetAccessibilityData to provide custom accessibility
    ///     information.
    /// </summary>
    internal struct VirtualTreeAccessibilityData
    {
        private string myHelpFile;
        private int myHelpContextId;
        private string myNameFormatString;
        private string myDescriptionFormatString;
        private AccessibilityReplacementField[] myNameReplacementFields;
        private AccessibilityReplacementField[] myDescriptionReplacementFields;
        private string[] myImageDescriptions;
        private string[] myStateImageDescriptions;
        private AccessibleStates[] myStateImageAccessibleStates;
        private string myHelpText;

        /// <summary>
        ///     Empty accessibility data, not special information is provided.
        /// </summary>
        public static readonly VirtualTreeAccessibilityData Empty = new VirtualTreeAccessibilityData();

        /// <summary>
        ///     Construct accessibility data with name settings
        /// </summary>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        public VirtualTreeAccessibilityData(string nameFormat, AccessibilityReplacementField[] nameReplacementFields)
        {
            myHelpFile = null;
            myHelpContextId = 0;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = null;
            myDescriptionReplacementFields = null;
            myImageDescriptions = null;
            myStateImageDescriptions = null;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with name and image text settings
        /// </summary>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="imageDescriptions">
        ///     Description strings corresponding to the different elements in the image list.
        ///     Should be specified if GetDisplayData for this item returns a custom image list and if the name or description
        ///     replacement fields include PrimaryImageText or PrimaryImageAndOverlaysText
        /// </param>
        public VirtualTreeAccessibilityData(
            string nameFormat, AccessibilityReplacementField[] nameReplacementFields, string[] imageDescriptions)
        {
            myHelpFile = null;
            myHelpContextId = 0;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = null;
            myDescriptionReplacementFields = null;
            myImageDescriptions = imageDescriptions;
            myStateImageDescriptions = null;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with name, image text, and state image text settings
        /// </summary>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="imageDescriptions">
        ///     Description strings corresponding to the different elements in the image list.
        ///     Should be specified if GetDisplayData for this item returns a custom image list and if the name or description
        ///     replacement fields include PrimaryImageText or PrimaryImageAndOverlaysText
        /// </param>
        /// <param name="stateImageDescriptions">
        ///     Description strings corresponding to the different elements in the state image list.
        ///     Should be specified if GetDisplayData for this item returns a custom state image list and if the name or description
        ///     replacement fields include StateImageText.
        /// </param>
        public VirtualTreeAccessibilityData(
            string nameFormat, AccessibilityReplacementField[] nameReplacementFields, string[] imageDescriptions,
            string[] stateImageDescriptions)
        {
            myHelpFile = null;
            myHelpContextId = 0;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = null;
            myDescriptionReplacementFields = null;
            myImageDescriptions = imageDescriptions;
            myStateImageDescriptions = stateImageDescriptions;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with name and description settings
        /// </summary>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="descriptionFormat">
        ///     The string to use as the item's accessibility description.
        ///     Specify null for default behavior.
        ///     Used as a format string if descriptionReplacementFields is not null.
        /// </param>
        /// <param name="descriptionReplacementFields">Replacement fields for the description string</param>
        public VirtualTreeAccessibilityData(
            string nameFormat, AccessibilityReplacementField[] nameReplacementFields, string descriptionFormat,
            AccessibilityReplacementField[] descriptionReplacementFields)
        {
            myHelpFile = null;
            myHelpContextId = 0;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = descriptionFormat;
            myDescriptionReplacementFields = descriptionReplacementFields;
            myImageDescriptions = null;
            myStateImageDescriptions = null;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with name, description and help text settings
        /// </summary>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="descriptionFormat">
        ///     The string to use as the item's accessibility description.
        ///     Specify null for default behavior.
        ///     Used as a format string if descriptionReplacementFields is not null.
        /// </param>
        /// <param name="descriptionReplacementFields">Replacement fields for the description string</param>
        /// <param name="helpText">The string to use as the item's accessibility help text</param>
        public VirtualTreeAccessibilityData(
            string nameFormat, AccessibilityReplacementField[] nameReplacementFields, string descriptionFormat,
            AccessibilityReplacementField[] descriptionReplacementFields, string helpText)
        {
            myHelpFile = null;
            myHelpContextId = 0;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = descriptionFormat;
            myDescriptionReplacementFields = descriptionReplacementFields;
            myImageDescriptions = null;
            myStateImageDescriptions = null;
            myStateImageAccessibleStates = null;
            myHelpText = helpText;
        }

        /// <summary>
        ///     Construct accessibility data with name, description, and image text settings
        /// </summary>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="descriptionFormat">
        ///     The string to use as the item's accessibility description.
        ///     Specify null for default behavior.
        ///     Used as a format string if descriptionReplacementFields is not null.
        /// </param>
        /// <param name="descriptionReplacementFields">Replacement fields for the description string</param>
        /// <param name="imageDescriptions">
        ///     Description strings corresponding to the different elements in the image list.
        ///     Should be specified if GetDisplayData for this item returns a custom image list and if the name or description
        ///     replacement fields include PrimaryImageText or PrimaryImageAndOverlaysText
        /// </param>
        public VirtualTreeAccessibilityData(
            string nameFormat, AccessibilityReplacementField[] nameReplacementFields, string descriptionFormat,
            AccessibilityReplacementField[] descriptionReplacementFields, string[] imageDescriptions)
        {
            myHelpFile = null;
            myHelpContextId = 0;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = descriptionFormat;
            myDescriptionReplacementFields = descriptionReplacementFields;
            myImageDescriptions = imageDescriptions;
            myStateImageDescriptions = null;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with name, description, image text, and state image text settings
        /// </summary>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="descriptionFormat">
        ///     The string to use as the item's accessibility description.
        ///     Specify null for default behavior.
        ///     Used as a format string if descriptionReplacementFields is not null.
        /// </param>
        /// <param name="descriptionReplacementFields">Replacement fields for the description string</param>
        /// <param name="imageDescriptions">
        ///     Description strings corresponding to the different elements in the image list.
        ///     Should be specified if GetDisplayData for this item returns a custom image list and if the name or description
        ///     replacement fields include PrimaryImageText or PrimaryImageAndOverlaysText
        /// </param>
        /// <param name="stateImageDescriptions">
        ///     Description strings corresponding to the different elements in the state image list.
        ///     Should be specified if GetDisplayData for this item returns a custom state image list and if the name or description
        ///     replacement fields include StateImageText.
        /// </param>
        public VirtualTreeAccessibilityData(
            string nameFormat, AccessibilityReplacementField[] nameReplacementFields, string descriptionFormat,
            AccessibilityReplacementField[] descriptionReplacementFields, string[] imageDescriptions, string[] stateImageDescriptions)
        {
            myHelpFile = null;
            myHelpContextId = 0;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = descriptionFormat;
            myDescriptionReplacementFields = descriptionReplacementFields;
            myImageDescriptions = imageDescriptions;
            myStateImageDescriptions = stateImageDescriptions;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with help settings
        /// </summary>
        /// <param name="helpFile">The help file for this item</param>
        /// <param name="helpContextId">The help context id for this item</param>
        public VirtualTreeAccessibilityData(string helpFile, int helpContextId)
        {
            myHelpFile = helpFile;
            myHelpContextId = helpContextId;
            myNameFormatString = null;
            myNameReplacementFields = null;
            myDescriptionFormatString = null;
            myDescriptionReplacementFields = null;
            myImageDescriptions = null;
            myStateImageDescriptions = null;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with help and name settings
        /// </summary>
        /// <param name="helpFile">The help file for this item</param>
        /// <param name="helpContextId">The help context id for this item</param>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        public VirtualTreeAccessibilityData(
            string helpFile, int helpContextId, string nameFormat, AccessibilityReplacementField[] nameReplacementFields)
        {
            myHelpFile = helpFile;
            myHelpContextId = helpContextId;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = null;
            myDescriptionReplacementFields = null;
            myImageDescriptions = null;
            myStateImageDescriptions = null;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with help, name, and image text settings
        /// </summary>
        /// <param name="helpFile">The help file for this item</param>
        /// <param name="helpContextId">The help context id for this item</param>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="imageDescriptions">
        ///     Description strings corresponding to the different elements in the image list.
        ///     Should be specified if GetDisplayData for this item returns a custom image list and if the name or description
        ///     replacement fields include PrimaryImageText or PrimaryImageAndOverlaysText
        /// </param>
        public VirtualTreeAccessibilityData(
            string helpFile, int helpContextId, string nameFormat, AccessibilityReplacementField[] nameReplacementFields,
            string[] imageDescriptions)
        {
            myHelpFile = helpFile;
            myHelpContextId = helpContextId;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = null;
            myDescriptionReplacementFields = null;
            myImageDescriptions = imageDescriptions;
            myStateImageDescriptions = null;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with help, name, image text, and state image text settings
        /// </summary>
        /// <param name="helpFile">The help file for this item</param>
        /// <param name="helpContextId">The help context id for this item</param>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="imageDescriptions">
        ///     Description strings corresponding to the different elements in the image list.
        ///     Should be specified if GetDisplayData for this item returns a custom image list and if the name or description
        ///     replacement fields include PrimaryImageText or PrimaryImageAndOverlaysText
        /// </param>
        /// <param name="stateImageDescriptions">
        ///     Description strings corresponding to the different elements in the state image list.
        ///     Should be specified if GetDisplayData for this item returns a custom state image list and if the name or description
        ///     replacement fields include StateImageText.
        /// </param>
        public VirtualTreeAccessibilityData(
            string helpFile, int helpContextId, string nameFormat, AccessibilityReplacementField[] nameReplacementFields,
            string[] imageDescriptions, string[] stateImageDescriptions)
        {
            myHelpFile = helpFile;
            myHelpContextId = helpContextId;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = null;
            myDescriptionReplacementFields = null;
            myImageDescriptions = imageDescriptions;
            myStateImageDescriptions = stateImageDescriptions;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with help, name and description settings
        /// </summary>
        /// <param name="helpFile">The help file for this item</param>
        /// <param name="helpContextId">The help context id for this item</param>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="descriptionFormat">
        ///     The string to use as the item's accessibility description.
        ///     Specify null for default behavior.
        ///     Used as a format string if descriptionReplacementFields is not null.
        /// </param>
        /// <param name="descriptionReplacementFields">Replacement fields for the description string</param>
        public VirtualTreeAccessibilityData(
            string helpFile, int helpContextId, string nameFormat, AccessibilityReplacementField[] nameReplacementFields,
            string descriptionFormat, AccessibilityReplacementField[] descriptionReplacementFields)
        {
            myHelpFile = helpFile;
            myHelpContextId = helpContextId;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = descriptionFormat;
            myDescriptionReplacementFields = descriptionReplacementFields;
            myImageDescriptions = null;
            myStateImageDescriptions = null;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with help, name, description and image text settings
        /// </summary>
        /// <param name="helpFile">The help file for this item</param>
        /// <param name="helpContextId">The help context id for this item</param>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="descriptionFormat">
        ///     The string to use as the item's accessibility description.
        ///     Specify null for default behavior.
        ///     Used as a format string if descriptionReplacementFields is not null.
        /// </param>
        /// <param name="descriptionReplacementFields">Replacement fields for the description string</param>
        /// <param name="imageDescriptions">
        ///     Description strings corresponding to the different elements in the image list.
        ///     Should be specified if GetDisplayData for this item returns a custom image list and if the name or description
        ///     replacement fields include PrimaryImageText or PrimaryImageAndOverlaysText
        /// </param>
        public VirtualTreeAccessibilityData(
            string helpFile, int helpContextId, string nameFormat, AccessibilityReplacementField[] nameReplacementFields,
            string descriptionFormat, AccessibilityReplacementField[] descriptionReplacementFields, string[] imageDescriptions)
        {
            myHelpFile = helpFile;
            myHelpContextId = helpContextId;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = descriptionFormat;
            myDescriptionReplacementFields = descriptionReplacementFields;
            myImageDescriptions = imageDescriptions;
            myStateImageDescriptions = null;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Construct accessibility data with help, name, description, image text, and state image text settings
        /// </summary>
        /// <param name="helpFile">The help file for this item</param>
        /// <param name="helpContextId">The help context id for this item</param>
        /// <param name="nameFormat">
        ///     The string to use as the item's accessibility name.
        ///     Specify null for default behavior.
        ///     Used as a format string if nameReplacementFields is not null.
        /// </param>
        /// <param name="nameReplacementFields">Replacement fields for the name string</param>
        /// <param name="descriptionFormat">
        ///     The string to use as the item's accessibility description.
        ///     Specify null for default behavior.
        ///     Used as a format string if descriptionReplacementFields is not null.
        /// </param>
        /// <param name="descriptionReplacementFields">Replacement fields for the description string</param>
        /// <param name="imageDescriptions">
        ///     Description strings corresponding to the different elements in the image list.
        ///     Should be specified if GetDisplayData for this item returns a custom image list and if the name or description
        ///     replacement fields include PrimaryImageText or PrimaryImageAndOverlaysText
        /// </param>
        /// <param name="stateImageDescriptions">
        ///     Description strings corresponding to the different elements in the state image list.
        ///     Should be specified if GetDisplayData for this item returns a custom state image list and if the name or description
        ///     replacement fields include StateImageText.
        /// </param>
        public VirtualTreeAccessibilityData(
            string helpFile, int helpContextId, string nameFormat, AccessibilityReplacementField[] nameReplacementFields,
            string descriptionFormat, AccessibilityReplacementField[] descriptionReplacementFields, string[] imageDescriptions,
            string[] stateImageDescriptions)
        {
            myHelpFile = helpFile;
            myHelpContextId = helpContextId;
            myNameFormatString = nameFormat;
            myNameReplacementFields = nameReplacementFields;
            myDescriptionFormatString = descriptionFormat;
            myDescriptionReplacementFields = descriptionReplacementFields;
            myImageDescriptions = imageDescriptions;
            myStateImageDescriptions = stateImageDescriptions;
            myStateImageAccessibleStates = null;
            myHelpText = null;
        }

        /// <summary>
        ///     Provide the help file used by accessibility
        /// </summary>
        public string HelpFile
        {
            get { return myHelpFile; }
            set { myHelpFile = value; }
        }

        /// <summary>
        ///     Provide the help context id used by accessibility
        /// </summary>
        public int HelpContextId
        {
            get { return myHelpContextId; }
            set { myHelpContextId = value; }
        }

        /// <summary>
        ///     Provide the help text used by accessibility
        /// </summary>
        public string HelpText
        {
            get { return myHelpText; }
            set { myHelpText = value; }
        }

        /// <summary>
        ///     Provide the string for the accessibility Name. If the string is null or Empty, this defers
        ///     to the IBranch.GetText value. If it is set, it is used directly as the name
        ///     unless the NameReplacementFields array is set, in which case it is used as a format
        ///     string passed string.Format with the corresponding calculated fields.
        /// </summary>
        public string NameFormatString
        {
            get { return myNameFormatString; }
            set { myNameFormatString = value; }
        }

        /// <summary>
        ///     Provide the string for accessibility Description. If the string is null or Empty, this defers
        ///     to the IBranch.GetTipText(ToolTipStyle.Icon) value. If it is set, it is used directly as the name
        ///     unless the DescriptionReplacementFields array is set, in which case it is used as a format
        ///     string passed string.Format with the corresponding calculated fields.
        /// </summary>
        public string DescriptionFormatString
        {
            get { return myDescriptionFormatString; }
            set { myDescriptionFormatString = value; }
        }

        /// <summary>
        ///     Set the value for the NameReplacementFields property if it was not set
        ///     in a constructor.
        /// </summary>
        /// <param name="replacementFields">The new replament fields</param>
        public void SetNameReplacementFields(IList replacementFields)
        {
            NameReplacementFields = replacementFields;
        }

        /// <summary>
        ///     A list of AccessibilityReplacementField indicates which values to supply to
        ///     the name format string. It is recommended that this value be assigned
        ///     from a static AccessibilityReplacementField array.
        /// </summary>
        public IList NameReplacementFields
        {
            get { return myNameReplacementFields; }
            private set { myNameReplacementFields = InterpretReplacementFieldsValue(value); }
        }

        /// <summary>
        ///     Set the value for the DescriptionReplacementFields property if it was not set
        ///     in a constructor.
        /// </summary>
        /// <param name="replacementFields">The new replament fields</param>
        public void SetDescriptionReplacementFields(IList replacementFields)
        {
            DescriptionReplacementFields = replacementFields;
        }

        /// <summary>
        ///     A list of AccessibilityReplacementField indicates which values to supply to
        ///     the description format string. It is recommended that this value be assigned
        ///     from a static AccessibilityReplacementField array.
        /// </summary>
        public IList DescriptionReplacementFields
        {
            get { return myDescriptionReplacementFields; }
            private set { myDescriptionReplacementFields = InterpretReplacementFieldsValue(value); }
        }

        private static AccessibilityReplacementField[] InterpretReplacementFieldsValue(IList value)
        {
            if (value != null)
            {
                var retVal = value as AccessibilityReplacementField[];
                if (retVal == null)
                {
                    retVal = new AccessibilityReplacementField[value.Count];
                    value.CopyTo(retVal, 0);
                }
                return retVal;
            }
            return null;
        }

        /// <summary>
        ///     Set the value for the ImageDescriptions property if it was not set
        ///     in a constructor.
        /// </summary>
        /// <param name="descriptions">The new image descriptions</param>
        public void SetImageDescriptions(IList descriptions)
        {
            myImageDescriptions = InterpretStringsValue(descriptions);
        }

        /// <summary>
        ///     If GetDisplayData supplies a custom image list, then this field must be
        ///     supplied to provide descriptions for the bitmaps if you request PrimaryImageText
        ///     or PrimaryImageAndOverlays replacement fields.
        /// </summary>
        public IList ImageDescriptions
        {
            get { return myImageDescriptions; }
        }

        /// <summary>
        ///     Set the value for the StateImageDescriptions property if it was not set
        ///     in a constructor.
        /// </summary>
        /// <param name="descriptions">The new image descriptions</param>
        public void SetStateImageDescriptions(IList descriptions)
        {
            myStateImageDescriptions = InterpretStringsValue(descriptions);
        }

        /// <summary>
        ///     If GetDisplayData supplies a custom image list, then this field must be
        ///     supplied to provide descriptions for the bitmaps if you request a
        ///     StateImageText replacement fields.
        /// </summary>
        public IList StateImageDescriptions
        {
            get { return myStateImageDescriptions; }
        }

        internal static string[] InterpretStringsValue(IList value)
        {
            if (value != null)
            {
                var retVal = value as string[];
                if (retVal == null)
                {
                    retVal = new string[value.Count];
                    value.CopyTo(retVal, 0);
                }
                return retVal;
            }
            return null;
        }

        /// <summary>
        ///     Set the value for the StateImageAccessibleStates property if it was not set
        ///     in a constructor.
        /// </summary>
        /// <param name="accessibleStates">The new accessible states.</param>
        public void SetStateImageAccessibleStates(IList accessibleStates)
        {
            myStateImageAccessibleStates = InterpretAccessibleStatesValue(accessibleStates);
        }

        /// <summary>
        ///     If GetDisplayData supplies a custom image list, then this field must be
        ///     supplied to provide accessible states for the bitmaps if you want to
        ///     provide custom accessible states.
        /// </summary>
        public IList StateImageAccessibleStates
        {
            get { return myStateImageAccessibleStates; }
        }

        internal static AccessibleStates[] InterpretAccessibleStatesValue(IList value)
        {
            if (value != null)
            {
                var retVal = value as AccessibleStates[];
                if (retVal == null)
                {
                    retVal = new AccessibleStates[value.Count];
                    value.CopyTo(retVal, 0);
                }
                return retVal;
            }
            return null;
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
        /// <returns>Always returns false, there is no need to compare VirtualTreeDisplayData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator ==(VirtualTreeAccessibilityData operand1, VirtualTreeAccessibilityData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Compare two VirtualTreeAccessibilityData structures
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns false, there is no need to compare VirtualTreeDisplayData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool Compare(VirtualTreeAccessibilityData operand1, VirtualTreeAccessibilityData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return false;
        }

        /// <summary>
        ///     Not equal operator. Defers to Compare.
        /// </summary>
        /// <param name="operand1">Left operand</param>
        /// <param name="operand2">Right operand</param>
        /// <returns>Always returns true, there is no need to compare VirtualTreeDisplayData structures</returns>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand1")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "operand2")]
        public static bool operator !=(VirtualTreeAccessibilityData operand1, VirtualTreeAccessibilityData operand2)
        {
            Debug.Assert(false); // There is no need to compare these
            return true;
        }

        #endregion // Equals override and related functions
    }
}
