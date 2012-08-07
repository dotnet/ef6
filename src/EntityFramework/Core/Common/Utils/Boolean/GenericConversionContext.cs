// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     Generic implementation of a ConversionContext
    /// </summary>
    internal sealed class GenericConversionContext<T_Identifier> : ConversionContext<T_Identifier>
    {
        private readonly Dictionary<TermExpr<T_Identifier>, int> _variableMap = new Dictionary<TermExpr<T_Identifier>, int>();
        private Dictionary<int, TermExpr<T_Identifier>> _inverseVariableMap;

        internal override Vertex TranslateTermToVertex(TermExpr<T_Identifier> term)
        {
            int variable;
            if (!_variableMap.TryGetValue(term, out variable))
            {
                variable = Solver.CreateVariable();
                _variableMap.Add(term, variable);
            }
            return Solver.CreateLeafVertex(variable, Solver.BooleanVariableChildren);
        }

        internal override IEnumerable<LiteralVertexPair<T_Identifier>> GetSuccessors(Vertex vertex)
        {
            var successors = new LiteralVertexPair<T_Identifier>[2];

            Debug.Assert(2 == vertex.Children.Length);
            var then = vertex.Children[0];
            var @else = vertex.Children[1];

            // get corresponding term expression
            InitializeInverseVariableMap();
            var term = _inverseVariableMap[vertex.Variable];

            // add positive successor (then)
            var literal = new Literal<T_Identifier>(term, true);
            successors[0] = new LiteralVertexPair<T_Identifier>(then, literal);

            // add negative successor (else)
            literal = literal.MakeNegated();
            successors[1] = new LiteralVertexPair<T_Identifier>(@else, literal);
            return successors;
        }

        private void InitializeInverseVariableMap()
        {
            if (null == _inverseVariableMap)
            {
                _inverseVariableMap = _variableMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            }
        }
    }
}
