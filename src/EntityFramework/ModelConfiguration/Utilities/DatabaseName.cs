namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;

    internal class DatabaseName
    {
        // Note: This class is currently immutable. If you make it mutable then you
        // must ensure that instances are cloned when cloning the DbModelBuilder.
        private readonly string _name;
        private readonly string _schema;

        public DatabaseName(string name)
            : this(name, null)
        {
        }

        public DatabaseName(string name, string schema)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            _name = name;
            _schema = schema;
        }

        public string Name
        {
            get { return _name; }
        }

        public string Schema
        {
            get { return _schema; }
        }

        public override string ToString()
        {
            var s = _name;

            if (_schema != null)
            {
                s = _schema + "." + s;
            }

            return s;
        }

        public bool Equals(DatabaseName other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(other._name, _name, StringComparison.Ordinal)
                   && string.Equals(other._schema, _schema, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType()
                != typeof(DatabaseName))
            {
                return false;
            }

            return Equals((DatabaseName)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_name.GetHashCode() * 397) ^ (_schema != null ? _schema.GetHashCode() : 0);
            }
        }

        public static void ParseQualifiedTableName(string qualifiedName, out string schemaName, out string tableName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(qualifiedName));

            qualifiedName = qualifiedName.Trim();

            // determine if there is a schema in the tableName
            var lastDot = qualifiedName.LastIndexOf('.');
            schemaName = null;
            tableName = qualifiedName;
            if (lastDot != -1)
            {
                if (lastDot == 0)
                {
                    throw Error.ToTable_InvalidSchemaName(qualifiedName);
                }
                else if (lastDot == tableName.Length - 1)
                {
                    throw Error.ToTable_InvalidTableName(qualifiedName);
                }
                schemaName = qualifiedName.Substring(0, lastDot);
                tableName = qualifiedName.Substring(lastDot + 1);
            }
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                schemaName = null;
            }
        }
    }
}
