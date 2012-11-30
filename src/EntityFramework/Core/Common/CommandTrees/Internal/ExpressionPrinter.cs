// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Prints a command tree
    /// </summary>
    internal class ExpressionPrinter : TreePrinter
    {
        private readonly PrinterVisitor _visitor = new PrinterVisitor();

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DbDeleteCommandTree")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])"
            )]
        internal string Print(DbDeleteCommandTree tree)
        {
            DebugCheck.NotNull(tree);
            DebugCheck.NotNull(tree.Predicate);

            TreeNode targetNode;
            if (tree.Target != null)
            {
                targetNode = _visitor.VisitBinding("Target", tree.Target);
            }
            else
            {
                targetNode = new TreeNode("Target");
            }

            TreeNode predicateNode;
            if (tree.Predicate != null)
            {
                predicateNode = _visitor.VisitExpression("Predicate", tree.Predicate);
            }
            else
            {
                predicateNode = new TreeNode("Predicate");
            }

            return Print(
                new TreeNode(
                    "DbDeleteCommandTree",
                    CreateParametersNode(tree),
                    targetNode,
                    predicateNode));
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DbFunctionCommandTree")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ResultType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EdmFunction")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])"
            )]
        internal string Print(DbFunctionCommandTree tree)
        {
            DebugCheck.NotNull(tree);

            var funcNode = new TreeNode("EdmFunction");
            if (tree.EdmFunction != null)
            {
                funcNode.Children.Add(_visitor.VisitFunction(tree.EdmFunction, null));
            }

            var typeNode = new TreeNode("ResultType");
            if (tree.ResultType != null)
            {
                PrinterVisitor.AppendTypeSpecifier(typeNode, tree.ResultType);
            }

            return Print(new TreeNode("DbFunctionCommandTree", CreateParametersNode(tree), funcNode, typeNode));
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DbInsertCommandTree")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SetClauses")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])"
            )]
        internal string Print(DbInsertCommandTree tree)
        {
            DebugCheck.NotNull(tree);

            TreeNode targetNode = null;
            if (tree.Target != null)
            {
                targetNode = _visitor.VisitBinding("Target", tree.Target);
            }
            else
            {
                targetNode = new TreeNode("Target");
            }

            var clausesNode = new TreeNode("SetClauses");
            foreach (var clause in tree.SetClauses)
            {
                if (clause != null)
                {
                    clausesNode.Children.Add(clause.Print(_visitor));
                }
            }

            TreeNode returningNode = null;
            if (null != tree.Returning)
            {
                returningNode = new TreeNode("Returning", _visitor.VisitExpression(tree.Returning));
            }
            else
            {
                returningNode = new TreeNode("Returning");
            }

            return Print(
                new TreeNode(
                    "DbInsertCommandTree",
                    CreateParametersNode(tree),
                    targetNode,
                    clausesNode,
                    returningNode));
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DbUpdateCommandTree")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SetClauses")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])"
            )]
        internal string Print(DbUpdateCommandTree tree)
        {
            // Predicate should not be null since DbUpdateCommandTree initializes it to DbConstantExpression(true)
            Debug.Assert(tree != null && tree.Predicate != null, "Invalid DbUpdateCommandTree");

            TreeNode targetNode = null;
            if (tree.Target != null)
            {
                targetNode = _visitor.VisitBinding("Target", tree.Target);
            }
            else
            {
                targetNode = new TreeNode("Target");
            }

            var clausesNode = new TreeNode("SetClauses");
            foreach (var clause in tree.SetClauses)
            {
                if (clause != null)
                {
                    clausesNode.Children.Add(clause.Print(_visitor));
                }
            }

            TreeNode predicateNode;
            if (null != tree.Predicate)
            {
                predicateNode = new TreeNode("Predicate", _visitor.VisitExpression(tree.Predicate));
            }
            else
            {
                predicateNode = new TreeNode("Predicate");
            }

            TreeNode returningNode;
            if (null != tree.Returning)
            {
                returningNode = new TreeNode("Returning", _visitor.VisitExpression(tree.Returning));
            }
            else
            {
                returningNode = new TreeNode("Returning");
            }

            return Print(
                new TreeNode(
                    "DbUpdateCommandTree",
                    CreateParametersNode(tree),
                    targetNode,
                    clausesNode,
                    predicateNode,
                    returningNode));
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DbQueryCommandTree")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])"
            )]
        internal string Print(DbQueryCommandTree tree)
        {
            DebugCheck.NotNull(tree);

            var queryNode = new TreeNode("Query");
            if (tree.Query != null)
            {
                PrinterVisitor.AppendTypeSpecifier(queryNode, tree.Query.ResultType);
                queryNode.Children.Add(_visitor.VisitExpression(tree.Query));
            }

            return Print(new TreeNode("DbQueryCommandTree", CreateParametersNode(tree), queryNode));
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])"
            )]
        private static TreeNode CreateParametersNode(DbCommandTree tree)
        {
            var retNode = new TreeNode("Parameters");
            foreach (var paramInfo in tree.Parameters)
            {
                var paramNode = new TreeNode(paramInfo.Key);
                PrinterVisitor.AppendTypeSpecifier(paramNode, paramInfo.Value);
                retNode.Children.Add(paramNode);
            }

            return retNode;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private class PrinterVisitor : DbExpressionVisitor<TreeNode>
        {
            private static readonly Dictionary<DbExpressionKind, string> _opMap = InitializeOpMap();

            private static Dictionary<DbExpressionKind, string> InitializeOpMap()
            {
                var opMap = new Dictionary<DbExpressionKind, string>(12);

                // Arithmetic
                opMap[DbExpressionKind.Divide] = "/";
                opMap[DbExpressionKind.Modulo] = "%";
                opMap[DbExpressionKind.Multiply] = "*";
                opMap[DbExpressionKind.Plus] = "+";
                opMap[DbExpressionKind.Minus] = "-";
                opMap[DbExpressionKind.UnaryMinus] = "-";

                // Comparison
                opMap[DbExpressionKind.Equals] = "=";
                opMap[DbExpressionKind.LessThan] = "<";
                opMap[DbExpressionKind.LessThanOrEquals] = "<=";
                opMap[DbExpressionKind.GreaterThan] = ">";
                opMap[DbExpressionKind.GreaterThanOrEquals] = ">=";
                opMap[DbExpressionKind.NotEquals] = "<>";

                return opMap;
            }

            private int _maxStringLength = 80;
            private bool _infix = true;

            internal TreeNode VisitExpression(DbExpression expr)
            {
                return expr.Accept(this);
            }

            internal TreeNode VisitExpression(string name, DbExpression expr)
            {
                return new TreeNode(name, expr.Accept(this));
            }

            internal TreeNode VisitBinding(string propName, DbExpressionBinding binding)
            {
                return VisitWithLabel(propName, binding.VariableName, binding.Expression);
            }

            internal TreeNode VisitFunction(EdmFunction func, IList<DbExpression> args)
            {
                var funcInfo = new TreeNode();
                AppendFullName(funcInfo.Text, func);

                AppendParameters(funcInfo, func.Parameters.Select(fp => new KeyValuePair<string, TypeUsage>(fp.Name, fp.TypeUsage)));
                if (args != null)
                {
                    AppendArguments(funcInfo, func.Parameters.Select(fp => fp.Name).ToArray(), args);
                }

                return funcInfo;
            }

            private static TreeNode NodeFromExpression(DbExpression expr)
            {
                return new TreeNode(Enum.GetName(typeof(DbExpressionKind), expr.ExpressionKind));
            }

            private static void AppendParameters(TreeNode node, IEnumerable<KeyValuePair<string, TypeUsage>> paramInfos)
            {
                node.Text.Append("(");
                var pos = 0;
                foreach (var paramInfo in paramInfos)
                {
                    if (pos > 0)
                    {
                        node.Text.Append(", ");
                    }
                    AppendType(node, paramInfo.Value);
                    node.Text.Append(" ");
                    node.Text.Append(paramInfo.Key);
                    pos++;
                }
                node.Text.Append(")");
            }

            internal static void AppendTypeSpecifier(TreeNode node, TypeUsage type)
            {
                node.Text.Append(" : ");
                AppendType(node, type);
            }

            internal static void AppendType(TreeNode node, TypeUsage type)
            {
                BuildTypeName(node.Text, type);
            }

            private static void BuildTypeName(StringBuilder text, TypeUsage type)
            {
                var rowType = type.EdmType as RowType;
                var collType = type.EdmType as CollectionType;
                var refType = type.EdmType as RefType;

                if (TypeSemantics.IsPrimitiveType(type))
                {
                    text.Append(type);
                }
                else if (collType != null)
                {
                    text.Append("Collection{");
                    BuildTypeName(text, collType.TypeUsage);
                    text.Append("}");
                }
                else if (refType != null)
                {
                    text.Append("Ref<");
                    AppendFullName(text, refType.ElementType);
                    text.Append(">");
                }
                else if (rowType != null)
                {
                    text.Append("Record[");
                    var idx = 0;
                    foreach (var recColumn in rowType.Properties)
                    {
                        text.Append("'");
                        text.Append(recColumn.Name);
                        text.Append("'");
                        text.Append("=");
                        BuildTypeName(text, recColumn.TypeUsage);
                        idx++;
                        if (idx < rowType.Properties.Count)
                        {
                            text.Append(", ");
                        }
                    }
                    text.Append("]");
                }
                else
                {
                    // Entity, Relationship, Complex
                    if (!string.IsNullOrEmpty(type.EdmType.NamespaceName))
                    {
                        text.Append(type.EdmType.NamespaceName);
                        text.Append(".");
                    }
                    text.Append(type.EdmType.Name);
                }
            }

            private static void AppendFullName(StringBuilder text, EdmType type)
            {
                if (BuiltInTypeKind.RowType
                    != type.BuiltInTypeKind)
                {
                    if (!string.IsNullOrEmpty(type.NamespaceName))
                    {
                        text.Append(type.NamespaceName);
                        text.Append(".");
                    }
                }

                text.Append(type.Name);
            }

            private List<TreeNode> VisitParams(IList<string> paramInfo, IList<DbExpression> args)
            {
                var retInfo = new List<TreeNode>();
                for (var idx = 0; idx < paramInfo.Count; idx++)
                {
                    var paramNode = new TreeNode(paramInfo[idx]);
                    paramNode.Children.Add(VisitExpression(args[idx]));
                    retInfo.Add(paramNode);
                }

                return retInfo;
            }

            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Collections.Generic.List<System.Data.Entity.Core.Common.Utils.TreeNode>)"
                )]
            private void AppendArguments(TreeNode node, IList<string> paramNames, IList<DbExpression> args)
            {
                if (paramNames.Count > 0)
                {
                    node.Children.Add(new TreeNode("Arguments", VisitParams(paramNames, args)));
                }
            }

            private TreeNode VisitWithLabel(string label, string name, DbExpression def)
            {
                var retInfo = new TreeNode(label);
                retInfo.Text.Append(" : '");
                retInfo.Text.Append(name);
                retInfo.Text.Append("'");
                retInfo.Children.Add(VisitExpression(def));

                return retInfo;
            }

            private TreeNode VisitBindingList(string propName, IList<DbExpressionBinding> bindings)
            {
                var bindingInfos = new List<TreeNode>();
                for (var idx = 0; idx < bindings.Count; idx++)
                {
                    bindingInfos.Add(VisitBinding(StringUtil.FormatIndex(propName, idx), bindings[idx]));
                }

                return new TreeNode(propName, bindingInfos);
            }

            private TreeNode VisitGroupBinding(DbGroupExpressionBinding groupBinding)
            {
                var inputInfo = VisitExpression(groupBinding.Expression);
                var retInfo = new TreeNode();
                retInfo.Children.Add(inputInfo);
                retInfo.Text.AppendFormat(
                    CultureInfo.InvariantCulture, "Input : '{0}', '{1}'", groupBinding.VariableName, groupBinding.GroupVariableName);
                return retInfo;
            }

            private TreeNode Visit(string name, params DbExpression[] exprs)
            {
                var retInfo = new TreeNode(name);
                foreach (var expr in exprs)
                {
                    retInfo.Children.Add(VisitExpression(expr));
                }
                return retInfo;
            }

            private TreeNode VisitInfix(DbExpression left, string name, DbExpression right)
            {
                if (_infix)
                {
                    var nullOp = new TreeNode("");
                    nullOp.Children.Add(VisitExpression(left));
                    nullOp.Children.Add(new TreeNode(name));
                    nullOp.Children.Add(VisitExpression(right));

                    return nullOp;
                }
                else
                {
                    return Visit(name, left, right);
                }
            }

            private TreeNode VisitUnary(DbUnaryExpression expr)
            {
                return VisitUnary(expr, false);
            }

            private TreeNode VisitUnary(DbUnaryExpression expr, bool appendType)
            {
                var retInfo = NodeFromExpression(expr);
                if (appendType)
                {
                    AppendTypeSpecifier(retInfo, expr.ResultType);
                }
                retInfo.Children.Add(VisitExpression(expr.Argument));
                return retInfo;
            }

            private TreeNode VisitBinary(DbBinaryExpression expr)
            {
                var retInfo = NodeFromExpression(expr);
                retInfo.Children.Add(VisitExpression(expr.Left));
                retInfo.Children.Add(VisitExpression(expr.Right));
                return retInfo;
            }

            #region DbExpressionVisitor<DbExpression> Members

            public override TreeNode Visit(DbExpression e)
            {
                Check.NotNull(e, "e");

                throw new NotSupportedException(Strings.Cqt_General_UnsupportedExpression(e.GetType().FullName));
            }

            public override TreeNode Visit(DbConstantExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = new TreeNode();
                var stringVal = e.Value as string;
                if (stringVal != null)
                {
                    stringVal = stringVal.Replace("\r\n", "\\r\\n");
                    var appendLength = stringVal.Length;
                    if (_maxStringLength > 0)
                    {
                        appendLength = Math.Min(stringVal.Length, _maxStringLength);
                    }
                    retInfo.Text.Append("'");
                    retInfo.Text.Append(stringVal, 0, appendLength);
                    if (stringVal.Length > appendLength)
                    {
                        retInfo.Text.Append("...");
                    }
                    retInfo.Text.Append("'");
                }
                else
                {
                    retInfo.Text.Append(e.Value);
                }

                return retInfo;
            }

            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            public override TreeNode Visit(DbNullExpression e)
            {
                Check.NotNull(e, "e");

                return new TreeNode("null");
            }

            public override TreeNode Visit(DbVariableReferenceExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = new TreeNode();
                retInfo.Text.AppendFormat("Var({0})", e.VariableName);
                return retInfo;
            }

            public override TreeNode Visit(DbParameterReferenceExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = new TreeNode();
                retInfo.Text.AppendFormat("@{0}", e.ParameterName);
                return retInfo;
            }

            public override TreeNode Visit(DbFunctionExpression e)
            {
                Check.NotNull(e, "e");

                var funcInfo = VisitFunction(e.Function, e.Arguments);
                return funcInfo;
            }

            public override TreeNode Visit(DbLambdaExpression expression)
            {
                Check.NotNull(expression, "expression");

                var lambdaInfo = new TreeNode();
                lambdaInfo.Text.Append("Lambda");

                AppendParameters(
                    lambdaInfo, expression.Lambda.Variables.Select(v => new KeyValuePair<string, TypeUsage>(v.VariableName, v.ResultType)));
                AppendArguments(lambdaInfo, expression.Lambda.Variables.Select(v => v.VariableName).ToArray(), expression.Arguments);
                lambdaInfo.Children.Add(Visit("Body", expression.Lambda.Body));

                return lambdaInfo;
            }

            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            public override TreeNode Visit(DbPropertyExpression e)
            {
                Check.NotNull(e, "e");

                TreeNode inst = null;
                if (e.Instance != null)
                {
                    inst = VisitExpression(e.Instance);
                    if (e.Instance.ExpressionKind == DbExpressionKind.VariableReference
                        ||
                        (e.Instance.ExpressionKind == DbExpressionKind.Property && 0 == inst.Children.Count))
                    {
                        inst.Text.Append(".");
                        inst.Text.Append(e.Property.Name);
                        return inst;
                    }
                }

                var retInfo = new TreeNode(".");
                var prop = e.Property as EdmProperty;
                if (prop != null
                    && !(prop.DeclaringType is RowType))
                {
                    // Entity, Relationship, Complex
                    AppendFullName(retInfo.Text, prop.DeclaringType);
                    retInfo.Text.Append(".");
                }
                retInfo.Text.Append(e.Property.Name);

                if (inst != null)
                {
                    retInfo.Children.Add(new TreeNode("Instance", inst));
                }

                return retInfo;
            }

            public override TreeNode Visit(DbComparisonExpression e)
            {
                Check.NotNull(e, "e");

                return VisitInfix(e.Left, _opMap[e.ExpressionKind], e.Right);
            }

            public override TreeNode Visit(DbLikeExpression e)
            {
                Check.NotNull(e, "e");

                return Visit("Like", e.Argument, e.Pattern, e.Escape);
            }

            public override TreeNode Visit(DbLimitExpression e)
            {
                Check.NotNull(e, "e");

                return Visit((e.WithTies ? "LimitWithTies" : "Limit"), e.Argument, e.Limit);
            }

            public override TreeNode Visit(DbIsNullExpression e)
            {
                Check.NotNull(e, "e");

                return VisitUnary(e);
            }

            public override TreeNode Visit(DbArithmeticExpression e)
            {
                Check.NotNull(e, "e");

                if (DbExpressionKind.UnaryMinus
                    == e.ExpressionKind)
                {
                    return Visit(_opMap[e.ExpressionKind], e.Arguments[0]);
                }
                else
                {
                    return VisitInfix(e.Arguments[0], _opMap[e.ExpressionKind], e.Arguments[1]);
                }
            }

            public override TreeNode Visit(DbAndExpression e)
            {
                Check.NotNull(e, "e");

                return VisitInfix(e.Left, "And", e.Right);
            }

            public override TreeNode Visit(DbOrExpression e)
            {
                Check.NotNull(e, "e");

                return VisitInfix(e.Left, "Or", e.Right);
            }

            public override TreeNode Visit(DbNotExpression e)
            {
                Check.NotNull(e, "e");

                return VisitUnary(e);
            }

            public override TreeNode Visit(DbDistinctExpression e)
            {
                Check.NotNull(e, "e");

                return VisitUnary(e);
            }

            public override TreeNode Visit(DbElementExpression e)
            {
                Check.NotNull(e, "e");

                return VisitUnary(e, true);
            }

            public override TreeNode Visit(DbIsEmptyExpression e)
            {
                Check.NotNull(e, "e");

                return VisitUnary(e);
            }

            public override TreeNode Visit(DbUnionAllExpression e)
            {
                Check.NotNull(e, "e");

                return VisitBinary(e);
            }

            public override TreeNode Visit(DbIntersectExpression e)
            {
                Check.NotNull(e, "e");

                return VisitBinary(e);
            }

            public override TreeNode Visit(DbExceptExpression e)
            {
                Check.NotNull(e, "e");

                return VisitBinary(e);
            }

            private TreeNode VisitCastOrTreat(string op, DbUnaryExpression e)
            {
                TreeNode retInfo = null;
                var argInfo = VisitExpression(e.Argument);
                if (0 == argInfo.Children.Count)
                {
                    argInfo.Text.Insert(0, op);
                    argInfo.Text.Insert(op.Length, '(');
                    argInfo.Text.Append(" As ");
                    AppendType(argInfo, e.ResultType);
                    argInfo.Text.Append(")");

                    retInfo = argInfo;
                }
                else
                {
                    retInfo = new TreeNode(op);
                    AppendTypeSpecifier(retInfo, e.ResultType);
                    retInfo.Children.Add(argInfo);
                }

                return retInfo;
            }

            public override TreeNode Visit(DbTreatExpression e)
            {
                Check.NotNull(e, "e");

                return VisitCastOrTreat("Treat", e);
            }

            public override TreeNode Visit(DbCastExpression e)
            {
                Check.NotNull(e, "e");

                return VisitCastOrTreat("Cast", e);
            }

            public override TreeNode Visit(DbIsOfExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = new TreeNode();
                if (DbExpressionKind.IsOfOnly
                    == e.ExpressionKind)
                {
                    retInfo.Text.Append("IsOfOnly");
                }
                else
                {
                    retInfo.Text.Append("IsOf");
                }

                AppendTypeSpecifier(retInfo, e.OfType);
                retInfo.Children.Add(VisitExpression(e.Argument));

                return retInfo;
            }

            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "OfType")]
            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "OfTypeOnly")]
            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            public override TreeNode Visit(DbOfTypeExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = new TreeNode(e.ExpressionKind == DbExpressionKind.OfTypeOnly ? "OfTypeOnly" : "OfType");
                AppendTypeSpecifier(retInfo, e.OfType);
                retInfo.Children.Add(VisitExpression(e.Argument));

                return retInfo;
            }

            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            public override TreeNode Visit(DbCaseExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = new TreeNode("Case");
                for (var idx = 0; idx < e.When.Count; idx++)
                {
                    retInfo.Children.Add(Visit("When", e.When[idx]));
                    retInfo.Children.Add(Visit("Then", e.Then[idx]));
                }

                retInfo.Children.Add(Visit("Else", e.Else));

                return retInfo;
            }

            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "RelatedEntityReferences")]
            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            public override TreeNode Visit(DbNewInstanceExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                AppendTypeSpecifier(retInfo, e.ResultType);

                if (BuiltInTypeKind.CollectionType
                    == e.ResultType.EdmType.BuiltInTypeKind)
                {
                    foreach (var element in e.Arguments)
                    {
                        retInfo.Children.Add(VisitExpression(element));
                    }
                }
                else
                {
                    var description = (BuiltInTypeKind.RowType == e.ResultType.EdmType.BuiltInTypeKind) ? "Column" : "Property";
                    IList<EdmProperty> properties = TypeHelpers.GetProperties(e.ResultType);
                    for (var idx = 0; idx < properties.Count; idx++)
                    {
                        retInfo.Children.Add(VisitWithLabel(description, properties[idx].Name, e.Arguments[idx]));
                    }

                    if (BuiltInTypeKind.EntityType == e.ResultType.EdmType.BuiltInTypeKind
                        &&
                        e.HasRelatedEntityReferences)
                    {
                        var references = new TreeNode("RelatedEntityReferences");
                        foreach (var relatedRef in e.RelatedEntityReferences)
                        {
                            var refNode = CreateNavigationNode(relatedRef.SourceEnd, relatedRef.TargetEnd);
                            refNode.Children.Add(CreateRelationshipNode((RelationshipType)relatedRef.SourceEnd.DeclaringType));
                            refNode.Children.Add(VisitExpression(relatedRef.TargetEntityReference));

                            references.Children.Add(refNode);
                        }

                        retInfo.Children.Add(references);
                    }
                }
                return retInfo;
            }

            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EntitySet")]
            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            public override TreeNode Visit(DbRefExpression e)
            {
                Check.NotNull(e, "e");

                var retNode = new TreeNode("Ref");
                retNode.Text.Append("<");
                AppendFullName(retNode.Text, TypeHelpers.GetEdmType<RefType>(e.ResultType).ElementType);
                retNode.Text.Append(">");

                var setNode = new TreeNode("EntitySet : ");
                setNode.Text.Append(e.EntitySet.EntityContainer.Name);
                setNode.Text.Append(".");
                setNode.Text.Append(e.EntitySet.Name);

                retNode.Children.Add(setNode);
                retNode.Children.Add(Visit("Keys", e.Argument));

                return retNode;
            }

            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            private static TreeNode CreateRelationshipNode(RelationshipType relType)
            {
                var rel = new TreeNode("Relationship");
                rel.Text.Append(" : ");
                AppendFullName(rel.Text, relType);
                return rel;
            }

            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            private static TreeNode CreateNavigationNode(RelationshipEndMember fromEnd, RelationshipEndMember toEnd)
            {
                var nav = new TreeNode();
                nav.Text.Append("Navigation : ");
                nav.Text.Append(fromEnd.Name);
                nav.Text.Append(" -> ");
                nav.Text.Append(toEnd.Name);
                return nav;
            }

            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            public override TreeNode Visit(DbRelationshipNavigationExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                retInfo.Children.Add(CreateRelationshipNode(e.Relationship));
                retInfo.Children.Add(CreateNavigationNode(e.NavigateFrom, e.NavigateTo));
                retInfo.Children.Add(Visit("Source", e.NavigationSource));

                return retInfo;
            }

            public override TreeNode Visit(DbDerefExpression e)
            {
                Check.NotNull(e, "e");

                return VisitUnary(e);
            }

            public override TreeNode Visit(DbRefKeyExpression e)
            {
                Check.NotNull(e, "e");

                return VisitUnary(e, true);
            }

            public override TreeNode Visit(DbEntityRefExpression e)
            {
                Check.NotNull(e, "e");

                return VisitUnary(e, true);
            }

            public override TreeNode Visit(DbScanExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                retInfo.Text.Append(" : ");
                retInfo.Text.Append(e.Target.EntityContainer.Name);
                retInfo.Text.Append(".");
                retInfo.Text.Append(e.Target.Name);
                return retInfo;
            }

            public override TreeNode Visit(DbFilterExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                retInfo.Children.Add(VisitBinding("Input", e.Input));
                retInfo.Children.Add(Visit("Predicate", e.Predicate));
                return retInfo;
            }

            public override TreeNode Visit(DbProjectExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                retInfo.Children.Add(VisitBinding("Input", e.Input));
                retInfo.Children.Add(Visit("Projection", e.Projection));
                return retInfo;
            }

            public override TreeNode Visit(DbCrossJoinExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                retInfo.Children.Add(VisitBindingList("Inputs", e.Inputs));
                return retInfo;
            }

            public override TreeNode Visit(DbJoinExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                retInfo.Children.Add(VisitBinding("Left", e.Left));
                retInfo.Children.Add(VisitBinding("Right", e.Right));
                retInfo.Children.Add(Visit("JoinCondition", e.JoinCondition));

                return retInfo;
            }

            public override TreeNode Visit(DbApplyExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                retInfo.Children.Add(VisitBinding("Input", e.Input));
                retInfo.Children.Add(VisitBinding("Apply", e.Apply));

                return retInfo;
            }

            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Collections.Generic.List<System.Data.Entity.Core.Common.Utils.TreeNode>)"
                )]
            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            public override TreeNode Visit(DbGroupByExpression e)
            {
                Check.NotNull(e, "e");

                var keys = new List<TreeNode>();
                var aggs = new List<TreeNode>();

                var outputType = TypeHelpers.GetEdmType<RowType>(TypeHelpers.GetEdmType<CollectionType>(e.ResultType).TypeUsage);
                var keyIdx = 0;
                for (var idx = 0; idx < e.Keys.Count; idx++)
                {
                    keys.Add(VisitWithLabel("Key", outputType.Properties[idx].Name, e.Keys[keyIdx]));
                    keyIdx++;
                }

                var aggIdx = 0;
                for (var idx = e.Keys.Count; idx < outputType.Properties.Count; idx++)
                {
                    var aggInfo = new TreeNode("Aggregate : '");
                    aggInfo.Text.Append(outputType.Properties[idx].Name);
                    aggInfo.Text.Append("'");

                    var funcAgg = e.Aggregates[aggIdx] as DbFunctionAggregate;
                    if (funcAgg != null)
                    {
                        var funcInfo = VisitFunction(funcAgg.Function, funcAgg.Arguments);
                        if (funcAgg.Distinct)
                        {
                            funcInfo = new TreeNode("Distinct", funcInfo);
                        }
                        aggInfo.Children.Add(funcInfo);
                    }
                    else
                    {
                        var groupAgg = e.Aggregates[aggIdx] as DbGroupAggregate;
                        Debug.Assert(groupAgg != null, "Invalid DbAggregate");
                        aggInfo.Children.Add(Visit("GroupAggregate", groupAgg.Arguments[0]));
                    }

                    aggs.Add(aggInfo);
                    aggIdx++;
                }

                var retInfo = NodeFromExpression(e);
                retInfo.Children.Add(VisitGroupBinding(e.Input));
                if (keys.Count > 0)
                {
                    retInfo.Children.Add(new TreeNode("Keys", keys));
                }

                if (aggs.Count > 0)
                {
                    retInfo.Children.Add(new TreeNode("Aggregates", aggs));
                }

                return retInfo;
            }

            [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SortOrder")]
            [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
                MessageId =
                    "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])")]
            private TreeNode VisitSortOrder(IList<DbSortClause> sortOrder)
            {
                var keyInfo = new TreeNode("SortOrder");
                foreach (var clause in sortOrder)
                {
                    var key = Visit((clause.Ascending ? "Asc" : "Desc"), clause.Expression);
                    if (!string.IsNullOrEmpty(clause.Collation))
                    {
                        key.Text.Append(" : ");
                        key.Text.Append(clause.Collation);
                    }

                    keyInfo.Children.Add(key);
                }

                return keyInfo;
            }

            public override TreeNode Visit(DbSkipExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                retInfo.Children.Add(VisitBinding("Input", e.Input));
                retInfo.Children.Add(VisitSortOrder(e.SortOrder));
                retInfo.Children.Add(Visit("Count", e.Count));
                return retInfo;
            }

            public override TreeNode Visit(DbSortExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                retInfo.Children.Add(VisitBinding("Input", e.Input));
                retInfo.Children.Add(VisitSortOrder(e.SortOrder));

                return retInfo;
            }

            public override TreeNode Visit(DbQuantifierExpression e)
            {
                Check.NotNull(e, "e");

                var retInfo = NodeFromExpression(e);
                retInfo.Children.Add(VisitBinding("Input", e.Input));
                retInfo.Children.Add(Visit("Predicate", e.Predicate));
                return retInfo;
            }

            #endregion
        }
    }
}
