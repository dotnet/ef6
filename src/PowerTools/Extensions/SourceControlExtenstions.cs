// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Extensions
{
    using EnvDTE;
    using Utilities;

    internal static class SourceControlExtensions
    {
        public static bool CheckOutItemIfNeeded(this SourceControl sourceControl, string itemName)
        {
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            DebugCheck.NotNull(sourceControl);
            DebugCheck.NotEmpty(itemName);

            if (sourceControl.IsItemUnderSCC(itemName) && !sourceControl.IsItemCheckedOut(itemName))
            {
                return sourceControl.CheckOutItem(itemName);
            }

            return false;
        }
    }
}
