// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// A PatternMatchRule allows for a pattern to be specified to identify interesting
    /// subtrees, rather than just an OpType
    /// </summary>
    internal sealed class PatternMatchRule : Rule
    {
        #region private state

        private readonly Node m_pattern;

        #endregion

        #region constructors

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="pattern"> The pattern to look for </param>
        /// <param name="processDelegate"> The callback to invoke when such a pattern is identified </param>
        internal PatternMatchRule(Node pattern, ProcessNodeDelegate processDelegate)
            : base(pattern.Op.OpType, processDelegate)
        {
            DebugCheck.NotNull(pattern);
            DebugCheck.NotNull(pattern.Op);
            m_pattern = pattern;
        }

        #endregion

        #region private methods

        private bool Match(Node pattern, Node original)
        {
            if (pattern.Op.OpType
                == OpType.Leaf)
            {
                return true;
            }
            if (pattern.Op.OpType
                != original.Op.OpType)
            {
                return false;
            }
            if (pattern.Children.Count
                != original.Children.Count)
            {
                return false;
            }
            for (var i = 0; i < pattern.Children.Count; i++)
            {
                if (!Match(pattern.Children[i], original.Children[i]))
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region overridden methods

        internal override bool Match(Node node)
        {
            return Match(m_pattern, node);
        }

        #endregion
    }
}
