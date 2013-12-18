// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics;

    internal class NullSemantics : BasicOpVisitorOfNode
    {
        private Command _command;
        private bool _modified;
        private bool _negated;

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

        protected override void VisitChildren(Node n)
        {
            var negated = _negated;

            if (n.Op.OpType == OpType.Not)
            {
                _negated = !_negated;
            }

            base.VisitChildren(n);

            _negated = negated;
        }

        protected override Node VisitScalarOpDefault(ScalarOp op, Node n)
        {
            if (op.OpType != OpType.EQ)
            {
                return base.VisitScalarOpDefault(op, n);
            }

            var result = ImplementEquality(n);
            _modified |= !ReferenceEquals(n, result);
            return result;
        }

        private Node ImplementEquality(Node n)
        {
            Debug.Assert(n.Op.OpType == OpType.EQ);

            var x = n.Child0;
            var y = n.Child1;

            switch (x.Op.OpType)
            {
                case OpType.Constant:
                case OpType.InternalConstant:
                    switch (y.Op.OpType)
                    {
                        case OpType.Constant:
                        case OpType.InternalConstant:
                            return n;
                        case OpType.Null:
                            return False();
                        default:
                            return _negated
                                ? And(n, Not(IsNull(y)))
                                : n;
                    }
                case OpType.Null:
                    switch (y.Op.OpType)
                    {
                        case OpType.Constant:
                        case OpType.InternalConstant:
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
                            return _negated
                                ? And(n, Not(IsNull(x)))
                                : n;
                        case OpType.Null:
                            return IsNull(x);
                        default:
                            return _negated
                                ? And(n, Not(Or(IsNull(x), IsNull(y))))
                                : Or(n, And(IsNull(x), IsNull(y)));
                    }
            }
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
            return _command.CreateNode(
                _command.CreateConditionalOp(OpType.IsNull),
                OpCopier.Copy(_command, x));
        }

        private Node Not(Node x)
        {
            return _command.CreateNode(
                _command.CreateConditionalOp(OpType.Not),
                OpCopier.Copy(_command, x));
        }

        private Node And(Node x, Node y)
        {
            return _command.CreateNode(
                _command.CreateConditionalOp(OpType.And),
                OpCopier.Copy(_command, x),
                OpCopier.Copy(_command, y));
        }

        private Node Or(Node x, Node y)
        {
            return _command.CreateNode(
                _command.CreateConditionalOp(OpType.Or),
                OpCopier.Copy(_command, x),
                OpCopier.Copy(_command, y));
        }
    }
}
