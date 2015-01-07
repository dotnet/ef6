// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // Represents the configuration for a specific context type
    // </summary>
    internal class ContextElement : ConfigurationElement
    {
        private const string TypeKey = "type";
        private const string CommandTimeoutKey = "commandTimeout";
        private const string DisableDatabaseInitializationKey = "disableDatabaseInitialization";
        private const string DatabaseInitializerKey = "databaseInitializer";

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [ConfigurationProperty(TypeKey, IsRequired = true)]
        public virtual string ContextTypeName
        {
            get { return (string)this[TypeKey]; }
            set { this[TypeKey] = value; }
        }
        
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [ConfigurationProperty(CommandTimeoutKey)]
        public virtual int? CommandTimeout
        {
            get { return (int?)this[CommandTimeoutKey]; }
            set { this[CommandTimeoutKey] = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [ConfigurationProperty(DisableDatabaseInitializationKey, DefaultValue = false)]
        public virtual bool IsDatabaseInitializationDisabled
        {
            get { return (bool)this[DisableDatabaseInitializationKey]; }
            set { this[DisableDatabaseInitializationKey] = value; }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [ConfigurationProperty(DatabaseInitializerKey)]
        public virtual DatabaseInitializerElement DatabaseInitializer
        {
            get { return (DatabaseInitializerElement)this[DatabaseInitializerKey]; }
            set { this[DatabaseInitializerKey] = value; }
        }
    }
}
