// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Represents a model configuration to set the join table and column names of a many-to-many association.
    /// </summary>
    public class JoinTableConfiguration : IFluentConfiguration
    {
        private readonly ICollection<string> _leftKeys = new List<string>();
        private readonly ICollection<string> _rightKeys = new List<string>();

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Gets or sets the schema of the table.
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Gets the names of the columns for the foreign key to the table of the entity on the left.
        /// </summary>
        public ICollection<string> LeftKeys
        {
            get { return _leftKeys; }
        }

        /// <summary>
        /// Gets the names of the columns for the foreign key to the table of the entity on the right.
        /// </summary>
        public ICollection<string> RightKeys
        {
            get { return _rightKeys; }
        }

        /// <inheritdoc />
        public virtual string GetMethodChain(CodeHelper code)
        {
            Debug.Assert(code != null, "code is null.");
            Debug.Assert(
                Table != null || _leftKeys.Any() || _rightKeys.Any(),
                "Table is null and _leftKeys and _rightKeys are empty.");

            var builder = new StringBuilder();
            builder.Append(".Map(");
            builder.Append(code.BeginLambda("m"));
            builder.Append("m");

            if (Table != null)
            {
                builder.Append(".ToTable(");
                builder.Append(code.Literal(Table));

                if (Schema != null)
                {
                    builder.Append(", ");
                    builder.Append(code.Literal(Schema));
                }

                builder.Append(")");
            }

            if (_leftKeys.Count != 0)
            {
                builder.Append(".MapLeftKey(");
                builder.Append(code.Literal(_leftKeys));
                builder.Append(")");
            }

            if (_rightKeys.Count != 0)
            {
                builder.Append(".MapRightKey(");
                builder.Append(code.Literal(_rightKeys));
                builder.Append(")");
            }

            builder.Append(")");

            return builder.ToString();
        }
    }
}
