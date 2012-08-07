// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Internal.UnitTests
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;

    public class FakeSqlGenerator : MigrationSqlGenerator
    {
        public override IEnumerable<MigrationStatement> Generate(
            IEnumerable<MigrationOperation> migrationOperations,
            string providerManifestToken)
        {
            throw new NotImplementedException();
        }
    }
}
