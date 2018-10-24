using System;

namespace Microsoft.DbContextPackage.Handlers
{
    internal class AboutHandler
    {
        private readonly DbContextPackage _package;

        public AboutHandler(DbContextPackage package)
        {
            _package = package;
        }

        public void ShowDialog()
        {
            VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                var dialog = new AboutDialog(_package);
                dialog.ShowDialog();
            }
            catch (Exception exception)
            {
                _package.LogError("An error occured when showing the About dialog", exception);
            }
        }
    }
}