// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Database
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;

    /// <summary>
    ///     Represents the full name of an object (e.g. a table) on a database
    ///     Consists of the name of the object plus the name of the schema
    ///     to which it belongs
    /// </summary>
    internal struct DatabaseObject
    {
        internal string Schema;
        internal string Name;

        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }

            if (typeof(DatabaseObject) != obj.GetType())
            {
                return false;
            }
            var objAsDatabaseObject = (DatabaseObject)obj;

            return (Schema == objAsDatabaseObject.Schema && Name == objAsDatabaseObject.Name);
        }

        public override int GetHashCode()
        {
            var schemaHashCode = (Schema != null ? Schema.GetHashCode() : 0);
            var nameHashCode = (Name != null ? Name.GetHashCode() : 0);
            return schemaHashCode ^ nameHashCode;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, Resources.DatabaseObjectNameFormat, Schema, Name);
        }

        internal static DatabaseObject CreateFromEntitySet(StorageEntitySet ses)
        {
            var dbObj = new DatabaseObject();
            dbObj.Schema = ses.DatabaseSchemaName;
            dbObj.Name = ses.DatabaseTableName;
            return dbObj;
        }

        internal static DatabaseObject CreateFromFunction(Function func)
        {
            var dbObj = new DatabaseObject();
            dbObj.Schema = func.DatabaseSchemaName;
            dbObj.Name = func.DatabaseFunctionName;
            return dbObj;
        }

        internal static DatabaseObject CreateFromEntityStoreSchemaFilterEntry(EntityStoreSchemaFilterEntry entry, string defaultSchemaName)
        {
            var dbObj = new DatabaseObject();
            dbObj.Name = entry.Name;
            dbObj.Schema = entry.Schema;

            // sometimes the database returns null for the schema whereas the EDM wants to
            // use the EntityContainer name as the default schema name - here we allow
            // overriding the schema name if it is not defined from the EntityStoreSchemaFilterEntry
            if (null == dbObj.Schema)
            {
                dbObj.Schema = defaultSchemaName;
            }

            return dbObj;
        }

        internal static DatabaseObject CreateFromSchemaProcedure(IRawDataSchemaProcedure schemaProcedure)
        {
            var dbObj = new DatabaseObject();
            dbObj.Name = schemaProcedure.Name;
            dbObj.Schema = schemaProcedure.Schema;
            return dbObj;
        }
    }

    internal class DatabaseObjectComparer : IComparer<DatabaseObject>
    {
        public int Compare(DatabaseObject x, DatabaseObject y)
        {
            var compareSchemas = String.Compare(x.Schema, y.Schema, StringComparison.CurrentCulture);
            if (compareSchemas != 0)
            {
                return compareSchemas;
            }
            else
            {
                return String.Compare(x.Name, y.Name, StringComparison.CurrentCulture);
            }
        }
    }
}
