using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Internal;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

sealed class ConfigurationProxy : IInternalConfigSystem
{
    readonly IInternalConfigSystem _originalConfig;
    string _testAssemblyPath;
    Configuration _testConfig;

    public ConfigurationProxy(IInternalConfigSystem originalConfig, string testAssemblyPath)
    {
        this._originalConfig = originalConfig;
        this._testAssemblyPath = testAssemblyPath;
    }

    public object GetSection(string configKey)
    {
        //Get original configuration section
        if (_originalConfig == null)
        {
            Trace.TraceWarning("ConfigurationProxy: _originalConfig is null");
            return null;
        }
        object originalSection = _originalConfig.GetSection(configKey);

        if (TestConfig == null)
        {
            Trace.TraceWarning("ConfigurationProxy: TestConfig is null");
            return originalSection;
        }

        //merge app settings
        if (configKey == "appSettings")
        {
            return GetMergedAppSettings(originalSection);
        }

        //merge connectionstrings
        if (configKey == "connectionStrings")
        {
            return GetMergedConnectionStringSection(originalSection);
        }

        //handle request for dataSources section
        if (configKey == "microsoft.visualstudio.testtools")
        {
            return GetMergedDataSourcesSection(configKey, originalSection);
        }
        return originalSection;
    }

    public void RefreshConfig(string sectionName) { _originalConfig.RefreshConfig(sectionName); }

    public static void Register(string testAssemblyPath)
    {
        try
        {
            if (File.Exists(testAssemblyPath + ".config"))
            {
                FieldInfo s_configSystem = typeof(ConfigurationManager).GetField("s_configSystem", BindingFlags.Static | BindingFlags.NonPublic);
                s_configSystem.SetValue(null, new ConfigurationProxy((IInternalConfigSystem)s_configSystem.GetValue(null), testAssemblyPath));
                Trace.WriteLine("ConfigurationProxy: Registered successfully for assembly" + testAssemblyPath);
            }
            else
            {
                Trace.TraceWarning("ConfigurationProxy: skipped registration, no test config file found for assembly " + testAssemblyPath);
            }
        }
        catch (Exception ex)
        {
            Trace.TraceError("ConfigurationProxy: Registration failed for assembly  " + testAssemblyPath + " exception:" + ex.ToString());
            throw;
        }
    }

    public bool SupportsUserConfig
    {
        get { return _originalConfig.SupportsUserConfig; }
    }

    private object GetMergedDataSourcesSection(string configKey, object originalSection)
    {
        var testDataSourcesSection = TestConfig.GetSection(configKey);

        //return original section if test config has no datasources section defined
        if (testDataSourcesSection == null)
        {
            return originalSection;
        }

        var mergedDataSources = new DataSourceElementCollection();

        //copy test datasources
        foreach (DataSourceElement dataSource in ((TestConfigurationSection)testDataSourcesSection).DataSources)
        {
            mergedDataSources.Add(dataSource);
        }

        //merge datasources from original config
        if (originalSection != null)
        {
            foreach (DataSourceElement item in ((TestConfigurationSection)originalSection).DataSources)
            {
                if (!mergedDataSources.Cast<DataSourceElement>().Any(x => x.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    mergedDataSources.Add(item);
                }
            }
        }

        //create merged TestConfigurationSection
        TestConfigurationSection mergedDataSourcesSection = new TestConfigurationSection();
        foreach (DataSourceElement dataSource in mergedDataSources)
        {
            mergedDataSourcesSection.DataSources.Add(dataSource);
        }
        return mergedDataSourcesSection;
    }

    private object GetMergedConnectionStringSection(object originalSection)
    {
        //return if test config has no connectionstring defined
        if (TestConfig.ConnectionStrings == null)
        {
            return originalSection;
        }

        var mergedConnectionStrings = new ConnectionStringSettingsCollection();

        //Copy test connection string to collection
        foreach (ConnectionStringSettings connectionStringSetting in TestConfig.ConnectionStrings.ConnectionStrings)
        {
            mergedConnectionStrings.Add(connectionStringSetting);
        }

        //merge connection strings from original config
        if (originalSection != null)
        {
            foreach (ConnectionStringSettings item in ((ConnectionStringsSection)originalSection).ConnectionStrings)
            {
                if (!mergedConnectionStrings.Cast<ConnectionStringSettings>().Any(x => x.Name.Equals(item.Name, StringComparison.CurrentCultureIgnoreCase)))
                {
                    mergedConnectionStrings.Add(item);
                }
            }
        }

        //create merged ConnectionStringsSection
        ConnectionStringsSection connectionStringsSection = new ConnectionStringsSection();
        foreach (ConnectionStringSettings connectionStringSetting in mergedConnectionStrings)
        {
            connectionStringsSection.ConnectionStrings.Add(connectionStringSetting);
        }
        return connectionStringsSection;
    }

    private object GetMergedAppSettings(object originalSection)
    {
        var cfg = new NameValueCollection((NameValueCollection)originalSection);
        foreach (string k in TestConfig.AppSettings.Settings.AllKeys)
        {
            if (cfg[k] == null)
            {
                cfg.Add(k, TestConfig.AppSettings.Settings[k].Value);
            }
            else
            {
                cfg[k] = TestConfig.AppSettings.Settings[k].Value;
            }
        }
        return cfg;
    }

    private Configuration TestConfig
    {
        get
        {
            if (_testConfig == null)
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = _testAssemblyPath + ".config";
                _testConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                if (_testConfig == null)
                {
                    Trace.TraceWarning("ConfigurationProxy: _testConfig is null");
                }
            }
            return _testConfig;
        }
    }

}
