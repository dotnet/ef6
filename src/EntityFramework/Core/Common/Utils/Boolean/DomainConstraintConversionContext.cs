// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;
    using System.Linq;

    // <summary>
    // Specialization of ConversionContext for DomainConstraint BoolExpr
    // </summary>
    internal sealed class DomainConstraintConversionContext<T_Variable, T_Element> :
        ConversionContext<DomainConstraint<T_Variable, T_Element>>
    {
        // <summary>
        // A map from domain variables to decision diagram variables.
        // </summary>
        private readonly Dictionary<DomainVariable<T_Variable, T_Element>, int> _domainVariableToRobddVariableMap =
            new Dictionary<DomainVariable<T_Variable, T_Element>, int>();

        private Dictionary<int, DomainVariable<T_Variable, T_Element>> _inverseMap;

        // <summary>
        // Translates a domain constraint term to an N-ary DD vertex.
        // </summary>
        internal override Vertex TranslateTermToVertex(TermExpr<DomainConstraint<T_Variable, T_Element>> term)
        {
            var range = term.Identifier.Range;
            var domainVariable = term.Identifier.Variable;
            var domain = domainVariable.Domain;

            if (range.All(element => !domain.Contains(element)))
            {
                // trivially false
                return Vertex.Zero;
            }

            if (domain.All(element => range.Contains(element)))
            {
                // trivially true
                return Vertex.One;
            }

            // determine assignments for this constraints (if the range contains a value in the domain, '1', else '0')
            var children = domain.Select(element => range.Contains(element) ? Vertex.One : Vertex.Zero).ToArray();

            // see if we know this variable
            int robddVariable;
            if (!_domainVariableToRobddVariableMap.TryGetValue(domainVariable, out robddVariable))
            {
                robddVariable = Solver.CreateVariable();
                _domainVariableToRobddVariableMap[domainVariable] = robddVariable;
            }

            // create a new vertex with the given assignments
            return Solver.CreateLeafVertex(robddVariable, children);
        }

        internal override IEnumerable<LiteralVertexPair<DomainConstraint<T_Variable, T_Element>>> GetSuccessors(Vertex vertex)
        {
            InitializeInverseMap();
            var domainVariable = _inverseMap[vertex.Variable];

            // since vertex children are ordinally aligned with domain, handle domain as array
            var domain = domainVariable.Domain.ToArray();

            // foreach unique successor vertex, build up range
            var vertexToRange = new Dictionary<Vertex, Set<T_Element>>();

            for (var i = 0; i < vertex.Children.Length; i++)
            {
                var successorVertex = vertex.Children[i];
                Set<T_Element> range;
                if (!vertexToRange.TryGetValue(successorVertex, out range))
                {
                    range = new Set<T_Element>(domainVariable.Domain.Comparer);
                    vertexToRange.Add(successorVertex, range);
                }
                range.Add(domain[i]);
            }

            foreach (var vertexRange in vertexToRange)
            {
                var successorVertex = vertexRange.Key;
                var range = vertexRange.Value;

                // construct a DomainConstraint including the given range
                var constraint = new DomainConstraint<T_Variable, T_Element>(domainVariable, range.MakeReadOnly());
                var literal = new Literal<DomainConstraint<T_Variable, T_Element>>(
                    new TermExpr<DomainConstraint<T_Variable, T_Element>>(constraint), true);

                yield return new LiteralVertexPair<DomainConstraint<T_Variable, T_Element>>(successorVertex, literal);
            }
        }

        private void InitializeInverseMap()
        {
            if (null == _inverseMap)
            {
                _inverseMap = _domainVariableToRobddVariableMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            }
        }
    }
}
