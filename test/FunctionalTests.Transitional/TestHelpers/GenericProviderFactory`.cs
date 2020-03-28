// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Common;
    using System.Data.Entity.Utilities;

    public class GenericProviderFactory<T> : DbProviderFactory
        where T : DbProviderFactory
    {
        public static GenericProviderFactory<T> Instance = new GenericProviderFactory<T>();

        private GenericProviderFactory()
        {
            var providerTable =
                (DataTable)
                typeof(DbProviderFactories).GetOnlyDeclaredMethod("GetProviderTable").Invoke(null, null);

            var row = providerTable.NewRow();
            row["Name"] = "GenericProviderFactory";
            row["InvariantName"] = InvariantProviderName;
            row["Description"] = "Fake GenericProviderFactory";
            row["AssemblyQualifiedName"] = GetType().AssemblyQualifiedName;
            providerTable.Rows.Add(row);
        }

        public string InvariantProviderName
        {
            get { return "My.Generic.Provider." + typeof(T).Name; }
        }

        public override DbConnection CreateConnection()
        {
            return new GenericConnection<T>();
        }
    }
}
