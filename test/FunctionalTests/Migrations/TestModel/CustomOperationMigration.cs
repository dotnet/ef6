// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.CustomOperations
{
    using System.Data.Entity.Migrations.Infrastructure;

    internal class CustomOperationMigration : DbMigration, IMigrationMetadata
    {
        public override void Up()
        {
            this.Comment("This is a test.");
        }

        public string Id
        {
            get { return "000000000000001_CustomOperationMigration"; }
        }

        public string Source
        {
            get { return null; }
        }

        public string Target
        {
            get { return MigrationMetadataHelper.GetModel<EmptyModel>(); }
        }
    }
}
