namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Xml.Linq;
    using EnvDTE;

    /// <summary>
    /// Processes a .config file to possibly add an "defaultConnectionFactory" entry and then
    /// save the file, if possible.
    /// </summary>
    internal class ConfigFileProcessor
    {
        /// <summary>
        /// Loads XML from the .config file, manipulates it to possibly add an "defaultConnectionFactory" entry
        /// and then attempts to save the file.
        /// </summary>
        /// <remarks>
        /// If the file cannot be saved then it is not saved and an exception is thrown. Under normal use this should not happen
        /// because NuGet will have ensured that the file is writable. It would be possible to try to do things like try check out
        /// the file from source control, but it doesn't seem like this is valuable enough to implement given it will not normally be used.
        /// </remarks>
        public virtual void ProcessConfigFile(ProjectItem configItem, IEnumerable<Func<XDocument, bool>> manipulators)
        {
            Contract.Requires(configItem != null);

            var fileName = configItem.FileNames[0];
            var config = XDocument.Load(fileName);

            var fileModified = false;
            foreach (var manipulator in manipulators)
            {
                fileModified = manipulator(config) || fileModified;
            }

            if (fileModified)
            {
                try
                {
                    config.Save(fileName);
                }
                catch (Exception ex)
                {
                    throw new IOException(Strings.SaveConnectionFactoryInConfigFailed(fileName), ex);
                }
            }
        }
    }
}