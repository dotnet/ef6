// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    /// Represents a model configuration to set the column name, type, and order for a property.
    /// </summary>
    public class ColumnConfiguration : IAttributeConfiguration, IFluentConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the database provider specific data type.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the order that this column should appear in the database table.
        /// </summary>
        public int? Order { get; set; }

        /// <inheritdoc />
        public virtual string GetAttributeBody(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");
            Debug.Assert(Name != null || TypeName != null || Order != null, "Name, TypeName & Order are null.");

            var builder = new StringBuilder();
            builder.Append("Column(");

            if (Name != null)
            {
                builder.Append(code.Literal(Name));
            }

            if (Order.HasValue)
            {
                if (Name != null)
                {
                    builder.Append(", ");
                }

                builder.Append("Order = ");
                builder.Append(code.Literal(Order.Value));
            }

            if (TypeName != null)
            {
                if (Name != null || Order.HasValue)
                {
                    builder.Append(", ");
                }

                builder.Append("TypeName = ");
                builder.Append(code.Literal(TypeName));
            }

            builder.Append(")");

            return builder.ToString();
        }

        /// <inheritdoc />
        public virtual string GetMethodChain(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");
            Debug.Assert(Name != null || TypeName != null || Order != null, "Name, TypeName & Order are null.");

            var builder = new StringBuilder();

            if (Name != null)
            {
                builder.Append(".HasColumnName(");
                builder.Append(code.Literal(Name));
                builder.Append(")");
            }

            if (Order.HasValue)
            {
                builder.Append(".HasColumnOrder(");
                builder.Append(code.Literal(Order.Value));
                builder.Append(")");
            }

            if (TypeName != null)
            {
                builder.Append(".HasColumnType(");
                builder.Append(code.Literal(TypeName));
                builder.Append(")");
            }

            return builder.ToString();
        }
    }
}
