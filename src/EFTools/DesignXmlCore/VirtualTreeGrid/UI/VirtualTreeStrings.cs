// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid
{
    using System.Globalization;
    using System.Reflection;
    using System.Resources;

    internal class VirtualTreeStrings
    {
        private VirtualTreeStrings()
        {
        }

        private static ResourceManager resourceManager;

        internal static ResourceManager ResourceManager
        {
            get
            {
                if (resourceManager == null)
                {
                    resourceManager = new ResourceManager(
                        "Microsoft.Data.Tools.VSXmlDesignerBase.VirtualTreeGrid.VirtualTreeControl", Assembly.GetExecutingAssembly());
                }

                return resourceManager;
            }
        }

        internal static string GetString(string id)
        {
            return ResourceManager.GetString(id, CultureInfo.CurrentUICulture);
        }

        internal const string DropDownAccessibleShortcut = "DropDownAccessibleShortcut";
        internal const string DropDownButtonAccessibleName = "DropDownButtonAccessibleName";
        internal const string BrowseButtonAccessibleName = "BrowseButtonAccessibleName";
        internal const string DuplicateColumnException = "DuplicateColumnException";
        internal const string InvalidColumnOrderArrayException = "InvalidColumnOrderArrayException";
        internal const string HeaderArrayException = "HeaderArrayException";
        internal const string BlankSubItemException = "BlankSubItemException";
        internal const string ColumnOutOfRangeException = "ColumnOutOfRangeException";
        internal const string ComplexColumnRootException = "ComplexColumnRootException";
        internal const string ParentRowException = "ParentRowException";
        internal const string ComplexColumnCellStyleException = "ComplexColumnCellStyleException";
        internal const string PercentageBasedHeadersInvalidException = "PercentageBasedHeadersInvalidException";
        internal const string PercentageBasedHeadersLastInvalidException = "PercentageBasedHeadersLastInvalidException";
        internal const string SelectionModeNoneException = "SelectionModeNoneException";
        internal const string CheckedAccDesc = "CheckedAccDesc";
        internal const string InactiveCheckedAccDesc = "InactiveCheckedAccDesc";
        internal const string UncheckedAccDesc = "UncheckedAccDesc";
        internal const string InactiveUncheckedAccDesc = "InactiveUncheckedAccDesc";
        internal const string PartiallyCheckedAccDesc = "PartiallyCheckedAccDesc";
        internal const string InactivePartiallyCheckedAccDesc = "InactivePartiallyCheckedAccDesc";
        internal const string RowAccDesc = "RowAccDesc";
        internal const string ColumnAccDesc = "ColumnAccDesc";
        internal const string ColumnNumberAccDesc = "ColumnNumberAccDesc";
        internal const string ChildRowNumberAccDesc = "ChildRowNumberAccDesc";
        internal const string RowOfTotalAccDesc = "RowOfTotalAccDesc";
        internal const string RowColumnAccDesc = "RowColumnAccDesc";
        internal const string DefActionExpandAccDesc = "DefActionExpandAccDesc";
        internal const string DefActionCollapseAccDesc = "DefActionCollapseAccDesc";
        internal const string OverlayIndicesRangeExceptionDesc = "OverlayIndicesRangeExceptionDesc";
        internal const string ShiftBranchLevelsExceptionDesc = "ShiftBranchLevelsExceptionDesc";
        internal const string GetSubItemCountExceptionDesc = "GetSubItemCountExceptionDesc";
        internal const string CalcTextHeightLetter = "CalcTextHeightLetter";
    }
}
