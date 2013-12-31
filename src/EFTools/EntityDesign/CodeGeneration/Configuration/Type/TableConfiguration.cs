// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Represents a model configuration to set the table for an entity.
    /// </summary>
    public class TableConfiguration : IAttributeConfiguration, IFluentConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Gets or sets the database schema of the table.
        /// </summary>
        public string Schema { get; set; }

        /// <inheritdoc />
        public virtual string GetAttributeBody(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");
            Debug.Assert(!string.IsNullOrEmpty(Table), "Table is null or empty.");

            return "Table(" + code.Literal(GetName()) + ")";
        }

        /// <inheritdoc />
        public virtual string GetMethodChain(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");
            Debug.Assert(!string.IsNullOrEmpty(Table), "Table is null or empty.");

            return ".ToTable(" + code.Literal(GetName()) + ")";
        }

        // Internal for testing
        internal string GetName()
        {
            Debug.Assert(!string.IsNullOrEmpty(Table), "Table is null or empty.");

            var builder = new StringBuilder();

            if (!string.IsNullOrEmpty(Schema))
            {
                builder.Append(Escape(Schema));
                builder.Append(".");
            }

            builder.Append(Escape(Table));

            return builder.ToString();
        }

        private static string Escape(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), "name is null or empty.");

            var builder = new StringBuilder();

            var hasDot = name.Contains(".");

            if (hasDot)
            {
                builder.Append("[");
            }

            builder.Append(name);

            if (hasDot)
            {
                builder.Append("]");
            }

            return builder.ToString();
        }
    }
}
