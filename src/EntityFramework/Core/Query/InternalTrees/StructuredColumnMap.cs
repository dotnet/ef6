// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Text;

    /// <summary>
    ///     Represents a column map for a structured column
    /// </summary>
    internal abstract class StructuredColumnMap : ColumnMap
    {
        private readonly ColumnMap[] m_properties;

        /// <summary>
        ///     Structured columnmap constructor
        /// </summary>
        /// <param name="type"> datatype for this column </param>
        /// <param name="name"> column name </param>
        /// <param name="properties"> list of properties </param>
        internal StructuredColumnMap(TypeUsage type, string name, ColumnMap[] properties)
            : base(type, name)
        {
            DebugCheck.NotNull(properties);
            m_properties = properties;
        }

        /// <summary>
        ///     Get the null sentinel column, if any.  Virtual so only derived column map
        ///     types that can have NullSentinel have to provide storage, etc.
        /// </summary>
        internal virtual SimpleColumnMap NullSentinel
        {
            get { return null; }
        }

        /// <summary>
        ///     Get the list of properties that constitute this structured type
        /// </summary>
        internal ColumnMap[] Properties
        {
            get { return m_properties; }
        }

        /// <summary>
        ///     Debugging support
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            var separator = String.Empty;
            sb.Append("{");
            foreach (var c in Properties)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", separator, c);
                separator = ",";
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}
