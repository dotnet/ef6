// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.SqlServer.Utilities;

    /// <summary>
    /// Used to create an instance of <see cref="SqlSpatialServices"/> for a specific SQL Types assembly
    /// such that it can be used for converting EF spatial types backed by one version to those backed by
    /// the version actually in use in this app domain.
    /// </summary>
    internal class SqlSpatialServicesForConversions : SqlSpatialServices
    {
        private readonly SqlTypesAssembly _sqlTypesAssembly;

        public SqlSpatialServicesForConversions(SqlTypesAssembly sqlTypesAssembly)
        {
            DebugCheck.NotNull(sqlTypesAssembly);

            _sqlTypesAssembly = sqlTypesAssembly;
        }

        public override SqlTypesAssembly SqlTypes
        {
            get { return _sqlTypesAssembly; }
        }

        public override bool NativeTypesAvailable
        {
            get { return true; }
        }
    }
}
