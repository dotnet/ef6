// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Data.Entity.SqlServer.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    internal class SqlStringBuilder
    {
        private readonly StringBuilder _sql;

        public SqlStringBuilder()
        {
            _sql = new StringBuilder();
        }

        public SqlStringBuilder(int capacity)
        {
            _sql = new StringBuilder(capacity);
        }

        public bool UpperCaseKeywords { get; set; }
        internal StringBuilder InnerBuilder { get { return _sql; } }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification="Keywords are known safe for lowercasing")]
        public SqlStringBuilder AppendKeyword(string keyword)
        {
            DebugCheck.NotNull(keyword);

            _sql.Append(
                UpperCaseKeywords
                    ? keyword.ToUpperInvariant()
                    : keyword.ToLowerInvariant());

            return this;
        }

        public SqlStringBuilder AppendLine()
        {
            _sql.AppendLine();

            return this;
        }

        public SqlStringBuilder AppendLine(string s)
        {
            _sql.AppendLine(s);

            return this;
        }

        public SqlStringBuilder Append(string s)
        {
            _sql.Append(s);

            return this;
        }

        public int Length
        {
            get { return _sql.Length; }
        }

        public override string ToString()
        {
            return _sql.ToString();
        }
    }
}
