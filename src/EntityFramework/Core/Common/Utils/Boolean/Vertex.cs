// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    // <summary>
    // A node in a Reduced Ordered Boolean Decision Diagram. Reads as:
    // if 'Variable' then 'Then' else 'Else'
    // Invariant: the Then and Else children must refer to 'deeper' variables,
    // or variables with a higher value. Otherwise, the graph is not 'Ordered'.
    // All creation of vertices is mediated by the Solver class which ensures
    // each vertex is unique. Otherwise, the graph is not 'Reduced'.
    // </summary>
    internal sealed class Vertex : IEquatable<Vertex>
    {
        // <summary>
        // Initializes a sink BDD node (zero or one)
        // </summary>
        private Vertex()
        {
            Variable = int.MaxValue;
            Children = new Vertex[] { };
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.EntityUtil.BoolExprAssert(System.Boolean,System.String)")]
        internal Vertex(int variable, Vertex[] children)
        {
            if (!(variable < int.MaxValue))
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.BoolExprAssert, 0, "exceeded number of supported variables");
            }

            AssertConstructorArgumentsValid(variable, children);

            Variable = variable;
            Children = children;
        }

        [Conditional("DEBUG")]
        private static void AssertConstructorArgumentsValid(int variable, Vertex[] children)
        {
            DebugCheck.NotNull(children);
            Debug.Assert(2 <= children.Length, "internal vertices must have at least two children");
            Debug.Assert(0 < variable, "internal vertices must have 0 < variable");
            foreach (var child in children)
            {
                Debug.Assert(variable < child.Variable, "children must have greater variable");
            }
        }

        // <summary>
        // Sink node representing the Boolean function '1' (true)
        // </summary>
        internal static readonly Vertex One = new Vertex();

        // <summary>
        // Sink node representing the Boolean function '0' (false)
        // </summary>
        internal static readonly Vertex Zero = new Vertex();

        // <summary>
        // Gets the variable tested by this vertex. If this is a sink node, returns
        // int.MaxValue since there is no variable to test (and since this is a leaf,
        // this non-existent variable is 'deeper' than any existing variable; the
        // variable value is larger than any real variable)
        // </summary>
        internal readonly int Variable;

        // <summary>
        // Note: do not modify elements.
        // Gets the result when Variable evaluates to true. If this is a sink node,
        // returns null.
        // </summary>
        internal readonly Vertex[] Children;

        // <summary>
        // Returns true if this is '1'.
        // </summary>
        internal bool IsOne()
        {
            return ReferenceEquals(One, this);
        }

        // <summary>
        // Returns true if this is '0'.
        // </summary>
        internal bool IsZero()
        {
            return ReferenceEquals(Zero, this);
        }

        // <summary>
        // Returns true if this is '0' or '1'.
        // </summary>
        internal bool IsSink()
        {
            return Variable == int.MaxValue;
        }

        public bool Equals(Vertex other)
        {
            return ReferenceEquals(this, other);
        }

        public override bool Equals(object obj)
        {
            Debug.Fail("used typed Equals");
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (IsOne())
            {
                return "_1_";
            }
            if (IsZero())
            {
                return "_0_";
            }
            return String.Format(CultureInfo.InvariantCulture, "<{0}, {1}>", Variable, StringUtil.ToCommaSeparatedString(Children));
        }
    }
}
