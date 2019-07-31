// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.WrappingProvider
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Text;

    public class WrappingSqlGenerator<TAdoNetBase> : MigrationSqlGenerator
        where TAdoNetBase : DbProviderFactory
    {
        private readonly Func<MigrationSqlGenerator> _baseGenerator;

        public WrappingSqlGenerator(Func<MigrationSqlGenerator> baseGenerator)
        {
            _baseGenerator = baseGenerator;
        }

        public override IEnumerable<MigrationStatement> Generate(
            IEnumerable<MigrationOperation> migrationOperations,
            string providerManifestToken)
        {
            var items = new StringBuilder();
            foreach (var operation in migrationOperations)
            {
                items.Append(operation.GetType().Name).Append(' ');
            }

            WrappingAdoNetProvider<TAdoNetBase>.Instance.Log.Add(
                new LogItem("Generate", null, items.ToString()));

            return _baseGenerator().Generate(migrationOperations, providerManifestToken);
        }
    }
}
