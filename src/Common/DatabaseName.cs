// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

#if SQLSERVER
namespace System.Data.Entity.SqlServer.Utilities
#elif SQLSERVERCOMPACT
namespace System.Data.Entity.SqlServerCompact.Utilities
#else
namespace System.Data.Entity.Utilities
#endif
{
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Resources;

    internal class DatabaseName
    {
        private const string NamePartRegex
            = @"(?:(?:\[(?<part{0}>(?:(?:\]\])|[^\]])+)\])|(?<part{0}>[^\.\[\]]+))";

        private static readonly Regex _partExtractor
            = new Regex(
                string.Format(
                    CultureInfo.InvariantCulture,
                    @"^{0}(?:\.{1})?$",
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                    string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)),
                RegexOptions.Compiled);

        public static DatabaseName Parse(string name)
        {
            DebugCheck.NotEmpty(name);

            var match = _partExtractor.Match(name.Trim());

            if (!match.Success)
            {
                throw Error.InvalidDatabaseName(name);
            }

            var part1 = match.Groups["part1"].Value.Replace("]]", "]");
            var part2 = match.Groups["part2"].Value.Replace("]]", "]");

            return !string.IsNullOrWhiteSpace(part2)
                       ? new DatabaseName(part2, part1)
                       : new DatabaseName(part1);
        }

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
            _name = name;
            _schema = !string.IsNullOrEmpty(schema) ? schema : null;
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
            var s = Escape(_name);

            if (_schema != null)
            {
                s = Escape(_schema) + "." + s;
            }

            return s;
        }

        private static string Escape(string name)
        {
            return name.IndexOfAny(new[] { ']', '[', '.' }) != -1
                       ? "[" + name.Replace("]", "]]") + "]"
                       : name;
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

            return (obj.GetType() == typeof(DatabaseName))
                   && Equals((DatabaseName)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_name.GetHashCode() * 397) ^ (_schema != null ? _schema.GetHashCode() : 0);
            }
        }
    }
}
