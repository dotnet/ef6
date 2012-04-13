namespace System.Data.Entity
{
    using System.Configuration;
    using System.Data.Entity.Internal.ConfigFile;
    using System.IO;
    using System.Xml.Linq;

    /// <summary>
    /// Base class for Productivity API tests with simple helper methods.
    /// </summary>
    public class UnitTestBase : TestBase
    {
        #region Creating config documents

        public static Configuration CreateEmptyConfig()
        {
            var tempFileName = Path.GetTempFileName();
            var doc = new XDocument(new XElement("configuration"));
            doc.Save(tempFileName);

            var config = ConfigurationManager.OpenMappedExeConfiguration(
                new ExeConfigurationFileMap() { ExeConfigFilename = tempFileName },
                ConfigurationUserLevel.None);

            config.Sections.Add("entityFramework", new EntityFrameworkSection());

            return config;
        }

        #endregion
    }
}