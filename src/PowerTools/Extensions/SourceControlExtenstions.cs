namespace Microsoft.DbContextPackage.Extensions
{
    using System.Diagnostics.Contracts;
    using EnvDTE;

    internal static class SourceControlExtenstions
    {
        public static bool CheckOutItemIfNeeded(this SourceControl sourceControl, string itemName)
        {
            Contract.Requires(sourceControl != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(itemName));

            if (sourceControl.IsItemUnderSCC(itemName) && !sourceControl.IsItemCheckedOut(itemName))
            {
                return sourceControl.CheckOutItem(itemName);
            }

            return false;
        }
    }
}
