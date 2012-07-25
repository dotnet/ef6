// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Internal.ConfigFile
{
    using System.Configuration;

    internal class ProviderElement : ConfigurationElement
    {
        private const string InvariantNameKey = "invariantName";
        private const string TypeKey = "type";
        private const string MigrationSqlGeneratorKey = "migrationSqlGenerator";

        [ConfigurationProperty(InvariantNameKey, IsRequired = true)]
        public string InvariantName
        {
            get { return (string)this[InvariantNameKey]; }
            set { this[InvariantNameKey] = value; }
        }

        [ConfigurationProperty(TypeKey, IsRequired = true)]
        public string ProviderTypeName
        {
            get { return (string)this[TypeKey]; }
            set { this[TypeKey] = value; }
        }

        [ConfigurationProperty(MigrationSqlGeneratorKey)]
        public MigrationSqlGeneratorElement SqlGeneratorElement
        {
            get { return (MigrationSqlGeneratorElement)this[MigrationSqlGeneratorKey]; }
            set { this[MigrationSqlGeneratorKey] = value; }
        }
    }
}
