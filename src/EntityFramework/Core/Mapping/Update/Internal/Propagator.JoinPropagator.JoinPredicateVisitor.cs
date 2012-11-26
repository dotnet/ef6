// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    internal partial class Propagator
    {
        private partial class JoinPropagator
        {
            /// <summary>
            ///     Extracts equi-join properties from a join condition.
            /// </summary>
            /// <remarks>
            ///     Assumptions:
            ///     <list>
            ///         <item>Only conjunctions of equality predicates are supported</item>
            ///         <item>Each equality predicate is of the form (left property == right property). The order
            ///             is important.</item>
            ///     </list>
            /// </remarks>
            private class JoinConditionVisitor : UpdateExpressionVisitor<object>
            {
                /// <summary>
                ///     Initializes a join predicate visitor. The visitor will populate the given property
                ///     lists with expressions describing the left and right hand side of equi-join
                ///     sub-clauses.
                /// </summary>
                private JoinConditionVisitor()
                {
                    m_leftKeySelectors = new List<DbExpression>();
                    m_rightKeySelectors = new List<DbExpression>();
                }

                private readonly List<DbExpression> m_leftKeySelectors;
                private readonly List<DbExpression> m_rightKeySelectors;
                private static readonly string _visitorName = typeof(JoinConditionVisitor).FullName;

                protected override string VisitorName
                {
                    get { return _visitorName; }
                }

                /// <summary>
                ///     Determine properties from the left and right inputs to an equi-join participating
                ///     in predicate.
                /// </summary>
                /// <remarks>
                ///     The property definitions returned are 'aligned'. If the join predicate reads:
                ///     <code>a = b AND c = d AND e = f</code>
                ///     then the output is as follows:
                ///     <code>leftProperties = {a, c, e}
                ///         rightProperties = {b, d, f}</code>
                ///     See Walker class for an explanation of this coding pattern.
                /// </remarks>
                internal static void GetKeySelectors(
                    DbExpression joinCondition, out ReadOnlyCollection<DbExpression> leftKeySelectors,
                    out ReadOnlyCollection<DbExpression> rightKeySelectors)
                {
                    Contract.Requires(joinCondition != null);

                    // Constructs a new predicate visitor, which implements a visitor for expression nodes
                    // and returns no values. This visitor instead builds up a list of properties as leaves
                    // of the join predicate are visited.
                    var visitor = new JoinConditionVisitor();

                    // Walk the predicate using the predicate visitor.
                    joinCondition.Accept(visitor);

                    // Retrieve properties discovered visiting predicate leaf nodes.
                    leftKeySelectors = visitor.m_leftKeySelectors.AsReadOnly();
                    rightKeySelectors = visitor.m_rightKeySelectors.AsReadOnly();

                    Debug.Assert(
                        leftKeySelectors.Count == rightKeySelectors.Count,
                        "(Update/JoinPropagator) The equi-join must have an equal number of left and right properties");
                }

                /// <summary>
                ///     Visit and node after its children have visited. There is nothing to do here
                ///     because only leaf equality nodes contain properties extracted by this visitor.
                /// </summary>
                /// <param name="node"> And expression node </param>
                /// <returns> Results ignored by this visitor implementation. </returns>
                public override object Visit(DbAndExpression node)
                {
                    Visit(node.Left);
                    Visit(node.Right);

                    return null;
                }

                /// <summary>
                ///     Perform work for an equality expression node.
                /// </summary>
                /// <param name="node"> Equality expresion node </param>
                /// <returns> Results ignored by this visitor implementation. </returns>
                public override object Visit(DbComparisonExpression node)
                {
                    if (DbExpressionKind.Equals
                        == node.ExpressionKind)
                    {
                        m_leftKeySelectors.Add(node.Left);
                        m_rightKeySelectors.Add(node.Right);
                        return null;
                    }
                    else
                    {
                        throw ConstructNotSupportedException(node);
                    }
                }
            }
        }
    }
}
