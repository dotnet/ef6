// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Data structure supporting storage of facts and proof (resolution) of queries given
    /// those facts.
    /// For instance, we may know the following facts:
    /// A --> B
    /// A
    /// Given these facts, the knowledge base can prove the query:
    /// B
    /// through resolution.
    /// </summary>
    /// <typeparam name="T_Identifier"> Type of leaf term identifiers in fact expressions. </typeparam>
    internal class KnowledgeBase<T_Identifier>
    {
        private readonly List<BoolExpr<T_Identifier>> _facts;
        private Vertex _knowledge;
        private readonly ConversionContext<T_Identifier> _context;

        /// <summary>
        /// Initialize a new knowledge base.
        /// </summary>
        internal KnowledgeBase()
        {
            _facts = new List<BoolExpr<T_Identifier>>();
            _knowledge = Vertex.One; // we know '1', but nothing else at present
            _context = IdentifierService<T_Identifier>.Instance.CreateConversionContext();
        }

        /// <summary>
        /// Adds all facts from another knowledge base
        /// </summary>
        /// <param name="kb"> The other knowledge base </param>
        internal void AddKnowledgeBase(KnowledgeBase<T_Identifier> kb)
        {
            foreach (var fact in kb._facts)
            {
                AddFact(fact);
            }
        }

        /// <summary>
        /// Adds the given fact to this KB.
        /// </summary>
        /// <param name="fact"> Simple fact. </param>
        internal virtual void AddFact(BoolExpr<T_Identifier> fact)
        {
            _facts.Add(fact);
            var converter = new Converter<T_Identifier>(fact, _context);
            var factVertex = converter.Vertex;
            _knowledge = _context.Solver.And(_knowledge, factVertex);
        }

        /// <summary>
        /// Adds the given implication to this KB, where implication is of the form:
        /// condition --> implies
        /// </summary>
        /// <param name="condition"> Condition </param>
        /// <param name="implies"> Entailed expression </param>
        internal void AddImplication(BoolExpr<T_Identifier> condition, BoolExpr<T_Identifier> implies)
        {
            AddFact(new Implication(condition, implies));
        }

        /// <summary>
        /// Adds an equivalence to this KB, of the form:
        /// left iff. right
        /// </summary>
        /// <param name="left"> Left operand </param>
        /// <param name="right"> Right operand </param>
        internal void AddEquivalence(BoolExpr<T_Identifier> left, BoolExpr<T_Identifier> right)
        {
            AddFact(new Equivalence(left, right));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Facts:");
            foreach (var fact in _facts)
            {
                builder.Append("\t").AppendLine(fact.ToString());
            }
            return builder.ToString();
        }

        // Protected class improving debugging output for implication facts 
        // (fact appears as A --> B rather than !A + B)
        protected class Implication : OrExpr<T_Identifier>
        {
            private readonly BoolExpr<T_Identifier> _condition;
            private readonly BoolExpr<T_Identifier> _implies;

            // These properties are used for the satisfiability test optimization
            internal BoolExpr<T_Identifier> Condition
            {
                get { return _condition; }
            }

            internal BoolExpr<T_Identifier> Implies
            {
                get { return _implies; }
            }

            // (condition --> implies) iff. (!condition OR implies) 
            internal Implication(BoolExpr<T_Identifier> condition, BoolExpr<T_Identifier> implies)
                : base(condition.MakeNegated(), implies)
            {
                _condition = condition;
                _implies = implies;
            }

            public override string ToString()
            {
                return StringUtil.FormatInvariant("{0} --> {1}", _condition, _implies);
            }
        }

        // Protected class improving debugging output for equivalence facts 
        // (fact appears as A <--> B rather than (!A + B) . (A + !B))
        protected class Equivalence : AndExpr<T_Identifier>
        {
            private readonly BoolExpr<T_Identifier> _left;
            private readonly BoolExpr<T_Identifier> _right;

            // These properties are used for the satisfiability test optimization
            internal BoolExpr<T_Identifier> Left
            {
                get { return _left; }
            }

            internal BoolExpr<T_Identifier> Right
            {
                get { return _right; }
            }

            // (left iff. right) iff. (left --> right AND right --> left)
            internal Equivalence(BoolExpr<T_Identifier> left, BoolExpr<T_Identifier> right)
                : base(new Implication(left, right), new Implication(right, left))
            {
                _left = left;
                _right = right;
            }

            public override string ToString()
            {
                return StringUtil.FormatInvariant("{0} <--> {1}", _left, _right);
            }
        }
    }
}
