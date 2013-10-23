// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;

    internal class SchemaQualifiedName : Tuple<string, string, string>
    {
        internal string SchemaName
        {
            get { return Item1; }
        }

        internal string TableName
        {
            get { return Item2; }
        }

        internal string EntityName
        {
            get { return Item3; }
        }

        internal SchemaQualifiedName(string tableName)
            : this(null, tableName, null)
        {
        }

        internal SchemaQualifiedName(string schemaName, string tableName, string entityName = null)
            : base(schemaName, tableName, entityName)
        {
        }
    }
}
