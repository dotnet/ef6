// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Serialization.Xml.Internal.Ssdl
{
    using System.Data.Entity.Edm.Db;

    internal static class DbModelSsdlHelper
    {
        private const string selfRefRoleNameSuffix = "Self";

        /// <summary>
        ///     Return role name pair
        /// </summary>
        /// <param name="firstTable"> </param>
        /// <param name="secondTable"> </param>
        /// <returns> </returns>
        internal static string[] GetRoleNamePair(DbTableMetadata firstTable, DbTableMetadata secondTable)
        {
            return new[]
                       {
                           firstTable.Name,
                           firstTable != secondTable ? secondTable.Name : secondTable.Name + selfRefRoleNameSuffix
                       };
        }
    }
}
