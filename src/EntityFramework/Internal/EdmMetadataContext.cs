// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Diagnostics.Contracts;

    internal class EdmMetadataContext : DbContext
    {
        public const string TableName = "EdmMetadata";

        static EdmMetadataContext()
        {
            Database.SetInitializer<EdmMetadataContext>(null);
        }

        public EdmMetadataContext(DbConnection existingConnection, bool contextOwnsConnection = true)
            : base(existingConnection, contextOwnsConnection)
        {
        }

#pragma warning disable 612,618
        public virtual IDbSet<EdmMetadata> Metadata { get; set; }
#pragma warning restore 612,618

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            ConfigureEdmMetadata(modelBuilder.ModelConfiguration);
        }

        public static void ConfigureEdmMetadata(ModelConfiguration modelConfiguration)
        {
            Contract.Requires(modelConfiguration != null);

#pragma warning disable 612,618
            modelConfiguration.Entity(typeof(EdmMetadata)).ToTable(TableName);
#pragma warning restore 612,618
        }
    }
}
