// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics;
    using System.Linq;

    internal class NullSemantics : BasicOpVisitorOfNode
    {
        private Command _command;

        // Flag that indicates whether the expression tree has changed or not.
        private bool _modified;

        // Flag that indicates whether the expansion of the equality operation 
        // must take the positive or the negative form.
        private bool _negated; 

        private VariableNullabilityTable _variableNullabilityTable 
            = new VariableNullabilityTable(capacity: 32);

        private NullSemantics(Command command)
        {
            _command = command;
        }

        public static bool Process(Command command)
        {
            var processor = new NullSemantics(command);

            command.Root = processor.VisitNode(command.Root);

            return processor._modified;
        }

        protected override Node VisitDefault(Node n)
        {
            var negated = _negated;

            switch (n.Op.OpType)
            {
                case OpType.Not:
                    _negated = !_negated;
                    n = base.VisitDefault(n);
                    break;
                case OpType.Or:
                    n = HandleOr(n);
                    break;
                case OpType.And:
                    n = base.VisitDefault(n);
                    break;
                case OpType.EQ:
                    _negated = false;
                    n = HandleEQ(n, negated);
                    break;
                case OpType.NE:
                    n = HandleNE(n);
                    break;
                default:
                    _negated = false;
                    n = base.VisitDefault(n);
                    break;
            }

            _negated = negated;

            return n;
        }

        private Node HandleOr(Node n)
        {
            // Check for the pattern '(varRef IS NULL) OR expression'.
            var isNullNode =
                n.Child0.Op.OpType == OpType.IsNull
                    ? n.Child0
                    : null;

            if (isNullNode == null
                || isNullNode.Child0.Op.OpType != OpType.VarRef)
            {
                return base.VisitDefault(n);
            }

            // Mark 'variable' as not nullable while 'expression' is visited.
            Var variable = ((VarRefOp)isNullNode.Child0.Op).Var;

            var nullable = _variableNullabilityTable[variable];
            _variableNullabilityTable[variable] = false;

            n.Child1 = VisitNode(n.Child1);

            _variableNullabilityTable[variable] = nullable;

            return n;
        }

        private Node HandleEQ(Node n, bool negated)
        {
            _modified |= 
                !ReferenceEquals(n.Child0, n.Child0 = VisitNode(n.Child0)) ||
                !ReferenceEquals(n.Child1, n.Child1 = VisitNode(n.Child1)) ||
                !ReferenceEquals(n, n = ImplementEquality(n, negated));

            return n;
        }

        private Node HandleNE(Node n)
        {
            Debug.Assert(n.Op.OpType == OpType.NE);

            var comparisonOp = (ComparisonOp)n.Op;

            // Transform a != b into !(a == b)
            n = _command.CreateNode(
                _command.CreateConditionalOp(OpType.Not),
                _command.CreateNode(
                    _command.CreateComparisonOp(OpType.EQ, comparisonOp.UseDatabaseNullSemantics),
                    n.Child0, n.Child1));

            _modified = true;

            return base.VisitDefault(n);
        }

        private bool IsNullableVarRef(Node n)
        {
            return n.Op.OpType == OpType.VarRef 
                && _variableNullabilityTable[((VarRefOp)n.Op).Var];
        }

        private Node ImplementEquality(Node n, bool negated)
        {
            Debug.Assert(n.Op.OpType == OpType.EQ);

            var comparisonOp = (ComparisonOp) n.Op;

            if (comparisonOp.UseDatabaseNullSemantics)
            {
                return n;
            }

            var x = n.Child0;
            var y = n.Child1;

            switch (x.Op.OpType)
            {
                case OpType.Constant:
                case OpType.InternalConstant:
                case OpType.NullSentinel:
                    switch (y.Op.OpType)
                    {
                        case OpType.Constant:
                        case OpType.InternalConstant:
                        case OpType.NullSentinel:
                            return n;
                        case OpType.Null:
                            return False();
                        default:
                            return negated
                                ? And(n, Not(IsNull(Clone(y))))
                                : n;
                    }
                case OpType.Null:
                    switch (y.Op.OpType)
                    {
                        case OpType.Constant:
                        case OpType.InternalConstant:
                        case OpType.NullSentinel:
                            return False();
                        case OpType.Null:
                            return True();
                        default:
                            return IsNull(y);
                    }
                default:
                    switch (y.Op.OpType)
                    {
                        case OpType.Constant:
                        case OpType.InternalConstant:
                        case OpType.NullSentinel:
                            return negated && IsNullableVarRef(n)
                                ? And(n, Not(IsNull(Clone(x))))
                                : n;
                        case OpType.Null:
                            return IsNull(x);
                        default:
                            return negated
                                ? And(n, NotXor(Clone(x), Clone(y)))
                                : Or(n, And(IsNull(Clone(x)), IsNull(Clone(y))));
                    }
            }
        }

        private Node Clone(Node x)
        {
            return OpCopier.Copy(_command, x);
        }

        private Node False()
        {
            return _command.CreateNode(_command.CreateFalseOp());
        }

        private Node True()
        {
            return _command.CreateNode(_command.CreateTrueOp());
        }

        private Node IsNull(Node x)
        {
            return _command.CreateNode(_command.CreateConditionalOp(OpType.IsNull), x);
        }

        private Node Not(Node x)
        {
            return _command.CreateNode(_command.CreateConditionalOp(OpType.Not), x);
        }

        private Node And(Node x, Node y)
        {
            return _command.CreateNode(_command.CreateConditionalOp(OpType.And), x, y);
        }

        private Node Or(Node x, Node y)
        {
            return _command.CreateNode(_command.CreateConditionalOp(OpType.Or), x, y);
        }

        private Node Boolean(bool value)
        {
            return _command.CreateNode(_command.CreateConstantOp(_command.BooleanType, value));
        }

        private Node NotXor(Node x, Node y)
        {
            return 
                _command.CreateNode(
                    _command.CreateComparisonOp(OpType.EQ),
                    _command.CreateNode(
                        _command.CreateCaseOp(_command.BooleanType),
                        IsNull(x), Boolean(true), Boolean(false)),
                    _command.CreateNode(
                        _command.CreateCaseOp(_command.BooleanType),
                        IsNull(y), Boolean(true), Boolean(false)));
        }

        private struct VariableNullabilityTable
        {
            private bool[] _entries;

            public VariableNullabilityTable(int capacity)
            {
                Debug.Assert(capacity > 0);
                _entries = Enumerable.Repeat(true, capacity).ToArray();
            }

            public bool this[Var variable]
            {
                get
                {
                    return variable.Id >= _entries.Length
                        || _entries[variable.Id];
                }

                set
                {
                    EnsureCapacity(variable.Id + 1);
                    _entries[variable.Id] = value;
                }
            }

            private void EnsureCapacity(int minimum)
            {
                Debug.Assert(_entries != null);

                if (_entries.Length < minimum)
                {
                    var capacity = _entries.Length * 2;
                    if (capacity < minimum)
                    {
                        capacity = minimum;
                    }

                    var newEntries = Enumerable.Repeat(true, capacity).ToArray();
                    Array.Copy(_entries, 0, newEntries, 0, _entries.Length);
                    _entries = newEntries;
                }
            }
        }
    }
}
