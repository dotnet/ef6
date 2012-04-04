namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;
    using System.ServiceProcess;

    /// <summary>
    /// Detects whether SQL Express and/or LocalDB are installed/available on this machine.
    /// </summary>
    internal class SqlServerDetector : IDisposable
    {
        public const string BaseConnectionStringTemplate =
            @"Data Source={0}; Integrated Security=True; MultipleActiveResultSets=True";

        private readonly RegistryKeyProxy _localMachine;
        private readonly ServiceControllerProxy _controller;

        /// <summary>
        /// Creates a detector using the given proxies for the HKEY_LOCAL_MACHINE registry key
        /// and ServiceController.
        /// </summary>
        public SqlServerDetector(RegistryKeyProxy localMachine, ServiceControllerProxy controller)
        {
            Contract.Requires(localMachine != null);
            Contract.Requires(controller != null);

            _localMachine = localMachine;
            _controller = controller;
        }


        /// <summary>
        /// Builds a specification for a default connection factory that will use SQL Express if it
        /// running on this machine, otherwise LocalDb.
        /// </summary>
        /// <remarks>
        /// If the SQL Express service is found, then SQL Express will be configured.
        /// Otherwise, if a particular version of LocalDB is found, then that version will be used. If
        /// multiple versions are found then an attempt to use the highest version will be made. If no version
        /// of SQL Express or LocalDB is found, then LocalDB v11.0 (SQL Server 2012) will be used.
        /// </remarks>
        public virtual ConnectionFactorySpecification BuildConnectionFactorySpecification()
        {
            return IsSqlExpressInstalled()
                       ? new ConnectionFactorySpecification(
                           ConnectionFactorySpecification.SqlConnectionFactoryName)
                       : new ConnectionFactorySpecification(
                           ConnectionFactorySpecification.LocalDbConnectionFactoryName,
                           "v" + (TryGetLocalDBVersionInstalled() ?? "11.0"));
        }

        /// <summary>
        /// Returns the highest version of LocalDB installed, or null if none was found.
        /// </summary>
        /// <remarks>
        /// If one version is found, then that version is always returned.
        /// If multiple versions are found, then an attempt to treat those versions as decimal numbers is
        /// made and the highest of these is returned.
        /// </remarks>
        public virtual string TryGetLocalDBVersionInstalled()
        {
            var key = OpenLocalDBInstalledVersions(useWow6432Node: false);

            if (key.SubKeyCount == 0)
            {
                key = OpenLocalDBInstalledVersions(useWow6432Node: true);
            }

            if (key.SubKeyCount == 1)
            {
                return key.GetSubKeyNames()[0];
            }

            var orderableVersions = new List<Tuple<decimal, string>>();
            foreach (var subKey in key.GetSubKeyNames())
            {
                decimal decimalVersion;
                if (Decimal.TryParse(subKey, out decimalVersion))
                {
                    orderableVersions.Add(Tuple.Create(decimalVersion, subKey));
                }
            }

            return orderableVersions.OrderByDescending(v => v.Item1).Select(v => v.Item2).FirstOrDefault();
        }

        /// <summary>
        /// Opens "HKLM\SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions"
        /// or "HKLM\SOFTWARE\Wow6432Node\Microsoft\Microsoft SQL Server Local DB\Installed Versions"
        /// depending on the passed useWow6432Node flag.
        /// Wow6432Node is used when 32-bit VS is looking for 64-bit SQL Server.
        /// </summary>
        private RegistryKeyProxy OpenLocalDBInstalledVersions(bool useWow6432Node)
        {
            var key = _localMachine.OpenSubKey("SOFTWARE");
            if (useWow6432Node)
            {
                key = key.OpenSubKey("Wow6432Node");
            }

            return key
                .OpenSubKey("Microsoft")
                .OpenSubKey("Microsoft SQL Server Local DB")
                .OpenSubKey("Installed Versions");
        }

        /// <summary>
        /// Returns true if SQL Express is running; false otherwise.
        /// </summary>
        public virtual bool IsSqlExpressInstalled()
        {
            try
            {
                return _controller.Status == ServiceControllerStatus.Running;
            }
            catch (InvalidOperationException)
            {
                // InvalidOperationException is thrown if the service is not present, so
                // just return false.
                return false;
            }
        }

        /// <summary>
        /// Disposes the underlying registry key and controller.
        /// </summary>
        public void Dispose()
        {
            _localMachine.Dispose();
            _controller.Dispose();
        }
    }
}
