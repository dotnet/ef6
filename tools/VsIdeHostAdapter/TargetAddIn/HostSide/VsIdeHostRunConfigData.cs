// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.Common;
using Microsoft.VisualStudio.TestTools.Common.Xml;
using System.Xml;
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// The data we extend Run Config with.
    /// - Registry Hive, like 8.0Exp.
    /// - Session id for debugging.
    /// </summary>
    [Serializable]
    internal class VsIdeHostRunConfigData: IHostSpecificRunConfigurationData, IXmlTestStore
    {
        #region Private
        private const string RegistryHiveAttributeName = "registryHive";
        private const string AdditionalCommandLineArgumentsAttributeName = "additionalCommandLineArguments";
        private const string AdditionalTestDataAttributeName = "additionalTestData";

        [PersistenceElementName(RegistryHiveAttributeName)]
        private string m_registryHive;

        [PersistenceElementName(AdditionalCommandLineArgumentsAttributeName)]
        private string m_additionalCommandLineArguments;

        [PersistenceElementName(AdditionalTestDataAttributeName)]
        private string m_additionalTestData;

        [NonPersistable]
        // For debugging only. The idea is that as soon as the user changes host type to VS.IDE we create an instance of this data.
        // Note: in Orcas this is obsolete as we introduced debugger extensibility. 
        // TODO: remove in internal version, the sample has been already changed.
        private string m_sessionId = VsIdeHostSession.Id;
        #endregion

        private VsIdeHostRunConfigData() // For XML persistence.
        {
        }

        internal VsIdeHostRunConfigData(string registryHive)
        {
            m_registryHive = registryHive;  // null is OK. null means get current version.
        }

        private VsIdeHostRunConfigData(VsIdeHostRunConfigData other)
        {
            m_registryHive = other.m_registryHive;
            m_additionalCommandLineArguments = other.m_additionalCommandLineArguments;
            m_additionalTestData = other.m_additionalTestData;
            m_sessionId = other.m_sessionId;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]   // We have to implement interface.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMethodsAsStatic")]
        public string RunConfigurationInformation
        {
            get { return Resources.RunConfigDataDescription; }
        }

        public object Clone()
        {
            return new VsIdeHostRunConfigData(this);
        }

        internal string SessionId
        {
            get {return m_sessionId; }
        }

        internal string RegistryHive
        {
            get { return m_registryHive; }
            set { m_registryHive = value; }
        }

        internal string AdditionalCommandLineArguments
        {
            get { return m_additionalCommandLineArguments; }
            set { m_additionalCommandLineArguments = value; }
        }

        internal string AdditionalTestData
        {
            get { return m_additionalTestData; }
            set { m_additionalTestData = value; }
        }

        #region IXmlTestStore Members
        public void Load(System.Xml.XmlElement element, XmlTestStoreParameters parameters)
        {
            // Note: when the attribute is missing, GetAttribute returns empty string.
            this.RegistryHive = element.GetAttribute(RegistryHiveAttributeName);
            this.AdditionalCommandLineArguments = element.GetAttribute(AdditionalCommandLineArgumentsAttributeName);
            this.AdditionalTestData = element.GetAttribute(AdditionalTestDataAttributeName);
        }

        public void Save(System.Xml.XmlElement element, XmlTestStoreParameters parameters)
        {
            if (!string.IsNullOrEmpty(this.RegistryHive))
            {
                element.SetAttribute(RegistryHiveAttributeName, this.RegistryHive);
            }
            if (!string.IsNullOrEmpty(this.AdditionalCommandLineArguments))
            {
                element.SetAttribute(AdditionalCommandLineArgumentsAttributeName, this.AdditionalCommandLineArguments);
            }
            if (!string.IsNullOrEmpty(this.AdditionalTestData))
            {
                element.SetAttribute(AdditionalTestDataAttributeName, this.AdditionalTestData);
            }
        }
        #endregion

    }
}
