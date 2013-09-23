// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Utilities;
    using System.Text;

    // <summary>
    // A class representing a set of constraints. It uses generic parameters
    // so that we can get strong typing and avoid downcasts
    // </summary>
    internal class SchemaConstraints<TKeyConstraint> : InternalBase
        where TKeyConstraint : InternalBase
    {
        // effects: Creates an empty set of constraints
        internal SchemaConstraints()
        {
            m_keyConstraints = new List<TKeyConstraint>();
        }

        // Use different lists so we can enumerate the right kind of constraints
        private readonly List<TKeyConstraint> m_keyConstraints;

        internal IEnumerable<TKeyConstraint> KeyConstraints
        {
            get { return m_keyConstraints; }
        }

        // effects: Adds a key constraint to this
        internal void Add(TKeyConstraint constraint)
        {
            DebugCheck.NotNull(constraint);

            m_keyConstraints.Add(constraint);
        }

        // effects: Converts constraints to human-readable strings and adds them to builder
        private static void ConstraintsToBuilder<Constraint>(IEnumerable<Constraint> constraints, StringBuilder builder)
            where Constraint : InternalBase
        {
            foreach (var constraint in constraints)
            {
                constraint.ToCompactString(builder);
                builder.Append(Environment.NewLine);
            }
        }

        internal override void ToCompactString(StringBuilder builder)
        {
            ConstraintsToBuilder(m_keyConstraints, builder);
        }
    }
}
