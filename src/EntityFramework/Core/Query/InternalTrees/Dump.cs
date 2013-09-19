// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


//
// This module serves as a dump routine for an IQT
// The output is a weird form of Sql - closer to Quel (and perhaps, C#
// comprehensions)
//

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// A dump module for the Iqt
    /// </summary>
    internal class Dump : BasicOpVisitor, IDisposable
    {
        #region private state

        private readonly XmlWriter _writer;

        #endregion

        #region constructors

        private Dump(Stream stream)
            : this(stream, DefaultEncoding)
        {
        }

        private Dump(Stream stream, Encoding encoding)
        {
            var settings = new XmlWriterSettings();
            settings.CheckCharacters = false;
            settings.Indent = true;
            settings.Encoding = encoding;
            _writer = XmlWriter.Create(stream, settings);
            _writer.WriteStartDocument(true);
        }

        #endregion

        #region "public" surface

        internal static readonly Encoding DefaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Driver method to dump the entire tree
        /// </summary>
        internal static string ToXml(Command itree)
        {
            return ToXml(itree.Root);
        }

        /// <summary>
        /// Driver method to dump the a subtree of a tree
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static string ToXml(Node subtree)
        {
            var stream = new MemoryStream();

            using (var dumper = new Dump(stream))
            {
                // Just in case the node we're provided doesn't dump as XML, we'll always stick
                // an XML wrapper around it -- this happens when we're dumping scalarOps, for
                // example, and it's unfortunate if you can't debug them using a dump...
                using (new AutoXml(dumper, "nodes"))
                {
                    dumper.VisitNode(subtree);
                }
            }

            return DefaultEncoding.GetString(stream.ToArray());
        }

#if DEBUG
        internal static string ToXml(ColumnMap columnMap)
        {
            var stream = new MemoryStream();

            using (var dumper = new Dump(stream))
            {
                // Just in case the node we're provided doesn't dump as XML, we'll always stick
                // an XML wrapper around it -- this happens when we're dumping scalarOps, for
                // example, and it's unfortunate if you can't debug them using a dump...
                using (new AutoXml(dumper, "columnMap"))
                {
                    columnMap.Accept(ColumnMapDumper.Instance, dumper);
                }
            }

            return DefaultEncoding.GetString(stream.ToArray());
        }
#endif

        #endregion

        #region Begin/End management

        void IDisposable.Dispose()
        {
            // Technically, calling GC.SuppressFinalize is not required because the class does not
            // have a finalizer, but it does no harm, protects against the case where a finalizer is added
            // in the future, and prevents an FxCop warning.
            GC.SuppressFinalize(this);
            try
            {
                _writer.WriteEndDocument();
                _writer.Flush();
                _writer.Close();
            }
            catch (Exception e)
            {
                if (!e.IsCatchableExceptionType())
                {
                    throw;
                }
                // eat this exception; we don't care if the dumper is failing...
            }
        }

        internal void Begin(string name, Dictionary<string, object> attrs)
        {
            _writer.WriteStartElement(name);
            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    _writer.WriteAttributeString(attr.Key, attr.Value.ToString());
                }
            }
        }

        internal void BeginExpression()
        {
            WriteString("(");
        }

        internal void EndExpression()
        {
            WriteString(")");
        }

        internal void End()
        {
            _writer.WriteEndElement();
        }

        internal void WriteString(string value)
        {
            _writer.WriteString(value);
        }

        #endregion

        #region VisitorMethods

        protected override void VisitDefault(Node n)
        {
            using (new AutoXml(this, n.Op))
            {
                base.VisitDefault(n);
            }
        }

        protected override void VisitScalarOpDefault(ScalarOp op, Node n)
        {
            using (new AutoString(this, op))
            {
                var separator = string.Empty;
                foreach (var chi in n.Children)
                {
                    WriteString(separator);
                    VisitNode(chi);
                    separator = ",";
                }
            }
        }

        protected override void VisitJoinOp(JoinBaseOp op, Node n)
        {
            using (new AutoXml(this, op))
            {
                if (n.Children.Count > 2)
                {
                    using (new AutoXml(this, "condition"))
                    {
                        VisitNode(n.Child2);
                    }
                }
                using (new AutoXml(this, "input"))
                {
                    VisitNode(n.Child0);
                }
                using (new AutoXml(this, "input"))
                {
                    VisitNode(n.Child1);
                }
            }
        }

        public override void Visit(CaseOp op, Node n)
        {
            using (new AutoXml(this, op))
            {
                var i = 0;
                while (i < n.Children.Count)
                {
                    if ((i + 1)
                        < n.Children.Count)
                    {
                        using (new AutoXml(this, "when"))
                        {
                            VisitNode(n.Children[i++]);
                        }
                        using (new AutoXml(this, "then"))
                        {
                            VisitNode(n.Children[i++]);
                        }
                    }
                    else
                    {
                        using (new AutoXml(this, "else"))
                        {
                            VisitNode(n.Children[i++]);
                        }
                    }
                }
            }
        }

        public override void Visit(CollectOp op, Node n)
        {
            using (new AutoXml(this, op))
            {
                VisitChildren(n);
            }
        }

        protected override void VisitConstantOp(ConstantBaseOp op, Node n)
        {
            using (new AutoString(this, op))
            {
                if (null == op.Value)
                {
                    WriteString("null");
                }
                else
                {
                    WriteString("(");
                    WriteString(op.Type.EdmType.FullName);
                    WriteString(")");
                    WriteString(String.Format(CultureInfo.InvariantCulture, "{0}", op.Value));
                }
                VisitChildren(n);
            }
        }

        public override void Visit(DistinctOp op, Node n)
        {
            var attrs = new Dictionary<string, object>();

            var sb = new StringBuilder();
            var separator = string.Empty;

            foreach (var v in op.Keys)
            {
                sb.Append(separator);
                sb.Append(v.Id);
                separator = ",";
            }
            if (0 != sb.Length)
            {
                attrs.Add("Keys", sb.ToString());
            }

            using (new AutoXml(this, op, attrs))
            {
                VisitChildren(n);
            }
        }

        protected override void VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            var attrs = new Dictionary<string, object>();

            var sb = new StringBuilder();
            var separator = string.Empty;

            foreach (var v in op.Keys)
            {
                sb.Append(separator);
                sb.Append(v.Id);
                separator = ",";
            }
            if (0 != sb.Length)
            {
                attrs.Add("Keys", sb.ToString());
            }

            using (new AutoXml(this, op, attrs))
            {
                using (new AutoXml(this, "outputs"))
                {
                    foreach (var v in op.Outputs)
                    {
                        DumpVar(v);
                    }
                }
                VisitChildren(n);
            }
        }

        public override void Visit(IsOfOp op, Node n)
        {
            using (new AutoXml(this, (op.IsOfOnly ? "IsOfOnly" : "IsOf")))
            {
                var separator = string.Empty;
                foreach (var chi in n.Children)
                {
                    WriteString(separator);
                    VisitNode(chi);
                    separator = ",";
                }
            }
        }

        protected override void VisitNestOp(NestBaseOp op, Node n)
        {
            var attrs = new Dictionary<string, object>();

            var ssnOp = op as SingleStreamNestOp;
            if (null != ssnOp)
            {
                attrs.Add("Discriminator", (ssnOp.Discriminator == null) ? "<null>" : ssnOp.Discriminator.ToString());
            }

            var sb = new StringBuilder();
            string separator;

            if (null != ssnOp)
            {
                sb.Length = 0;
                separator = string.Empty;
                foreach (var v in ssnOp.Keys)
                {
                    sb.Append(separator);
                    sb.Append(v.Id);
                    separator = ",";
                }
                if (0 != sb.Length)
                {
                    attrs.Add("Keys", sb.ToString());
                }
            }

            using (new AutoXml(this, op, attrs))
            {
                using (new AutoXml(this, "outputs"))
                {
                    foreach (var v in op.Outputs)
                    {
                        DumpVar(v);
                    }
                }
                foreach (var ci in op.CollectionInfo)
                {
                    var attrs2 = new Dictionary<string, object>();
                    attrs2.Add("CollectionVar", ci.CollectionVar);

                    if (null != ci.DiscriminatorValue)
                    {
                        attrs2.Add("DiscriminatorValue", ci.DiscriminatorValue);
                    }
                    if (0 != ci.FlattenedElementVars.Count)
                    {
                        attrs2.Add("FlattenedElementVars", FormatVarList(sb, ci.FlattenedElementVars));
                    }
                    if (0 != ci.Keys.Count)
                    {
                        attrs2.Add("Keys", ci.Keys);
                    }
                    if (0 != ci.SortKeys.Count)
                    {
                        attrs2.Add("SortKeys", FormatVarList(sb, ci.SortKeys));
                    }
                    using (new AutoXml(this, "collection", attrs2))
                    {
                        ci.ColumnMap.Accept(ColumnMapDumper.Instance, this);
                    }
                }
                VisitChildren(n);
            }
        }

        private static string FormatVarList(StringBuilder sb, VarList varList)
        {
            string separator;
            sb.Length = 0;
            separator = string.Empty;
            foreach (var v in varList)
            {
                sb.Append(separator);
                sb.Append(v.Id);
                separator = ",";
            }
            return sb.ToString();
        }

        private static string FormatVarList(StringBuilder sb, List<SortKey> varList)
        {
            string separator;
            sb.Length = 0;
            separator = string.Empty;
            foreach (var v in varList)
            {
                sb.Append(separator);
                sb.Append(v.Var.Id);
                separator = ",";
            }
            return sb.ToString();
        }

        private void VisitNewOp(Op op, Node n)
        {
            using (new AutoXml(this, op))
            {
                foreach (var chi in n.Children)
                {
                    using (new AutoXml(this, "argument", null))
                    {
                        VisitNode(chi);
                    }
                }
            }
        }

        public override void Visit(NewEntityOp op, Node n)
        {
            VisitNewOp(op, n);
        }

        public override void Visit(NewInstanceOp op, Node n)
        {
            VisitNewOp(op, n);
        }

        public override void Visit(DiscriminatedNewEntityOp op, Node n)
        {
            VisitNewOp(op, n);
        }

        public override void Visit(NewMultisetOp op, Node n)
        {
            VisitNewOp(op, n);
        }

        public override void Visit(NewRecordOp op, Node n)
        {
            VisitNewOp(op, n);
        }

        public override void Visit(PhysicalProjectOp op, Node n)
        {
            using (new AutoXml(this, op))
            {
                using (new AutoXml(this, "outputs"))
                {
                    foreach (var v in op.Outputs)
                    {
                        DumpVar(v);
                    }
                }
                using (new AutoXml(this, "columnMap"))
                {
                    op.ColumnMap.Accept(ColumnMapDumper.Instance, this);
                }
                using (new AutoXml(this, "input"))
                {
                    VisitChildren(n);
                }
            }
        }

        public override void Visit(ProjectOp op, Node n)
        {
            using (new AutoXml(this, op))
            {
                using (new AutoXml(this, "outputs"))
                {
                    foreach (var v in op.Outputs)
                    {
                        DumpVar(v);
                    }
                }
                VisitChildren(n);
            }
        }

        public override void Visit(PropertyOp op, Node n)
        {
            using (new AutoString(this, op))
            {
                VisitChildren(n);
                WriteString(".");
                WriteString(op.PropertyInfo.Name);
            }
        }

        public override void Visit(RelPropertyOp op, Node n)
        {
            using (new AutoString(this, op))
            {
                VisitChildren(n);
                WriteString(".NAVIGATE(");
                WriteString(op.PropertyInfo.Relationship.Name);
                WriteString(",");
                WriteString(op.PropertyInfo.FromEnd.Name);
                WriteString(",");
                WriteString(op.PropertyInfo.ToEnd.Name);
                WriteString(")");
            }
        }

        public override void Visit(ScanTableOp op, Node n)
        {
            using (new AutoXml(this, op))
            {
                DumpTable(op.Table);
                VisitChildren(n);
            }
        }

        public override void Visit(ScanViewOp op, Node n)
        {
            using (new AutoXml(this, op))
            {
                DumpTable(op.Table);
                VisitChildren(n);
            }
        }

        protected override void VisitSetOp(SetOp op, Node n)
        {
            var attrs = new Dictionary<string, object>();
            if (OpType.UnionAll
                == op.OpType)
            {
                var uallOp = (UnionAllOp)op;
                if (null != uallOp.BranchDiscriminator)
                {
                    attrs.Add("branchDiscriminator", uallOp.BranchDiscriminator);
                }
            }
            using (new AutoXml(this, op, attrs))
            {
                using (new AutoXml(this, "outputs"))
                {
                    foreach (var v in op.Outputs)
                    {
                        DumpVar(v);
                    }
                }
                var i = 0;
                foreach (var chi in n.Children)
                {
                    var attrs2 = new Dictionary<string, object>();
                    attrs2.Add("VarMap", op.VarMap[i++].ToString());

                    using (new AutoXml(this, "input", attrs2))
                    {
                        VisitNode(chi);
                    }
                }
            }
        }

        public override void Visit(SortOp op, Node n)
        {
            using (new AutoXml(this, op))
            {
                base.Visit(op, n);
            }
        }

        public override void Visit(ConstrainedSortOp op, Node n)
        {
            var attrs = new Dictionary<string, object>();
            attrs.Add("WithTies", op.WithTies);
            using (new AutoXml(this, op, attrs))
            {
                base.Visit(op, n);
            }
        }

        protected override void VisitSortOp(SortBaseOp op, Node n)
        {
            using (new AutoXml(this, "keys"))
            {
                foreach (var sortKey in op.Keys)
                {
                    var attrs = new Dictionary<string, object>();
                    attrs.Add("Var", sortKey.Var);
                    attrs.Add("Ascending", sortKey.AscendingSort);
                    attrs.Add("Collation", sortKey.Collation);

                    using (new AutoXml(this, "sortKey", attrs))
                    {
                    }
                }
            }
            VisitChildren(n);
        }

        public override void Visit(UnnestOp op, Node n)
        {
            var attrs = new Dictionary<string, object>();
            if (null != op.Var)
            {
                attrs.Add("Var", op.Var.Id);
            }
            using (new AutoXml(this, op, attrs))
            {
                DumpTable(op.Table);
                VisitChildren(n);
            }
        }

        public override void Visit(VarDefOp op, Node n)
        {
            var attrs = new Dictionary<string, object>();
            attrs.Add("Var", op.Var.Id);

            using (new AutoXml(this, op, attrs))
            {
                VisitChildren(n);
            }
        }

        public override void Visit(VarRefOp op, Node n)
        {
            using (new AutoString(this, op))
            {
                VisitChildren(n);
                if (null != op.Type)
                {
                    WriteString("Type=");
                    WriteString(op.Type.ToString());
                    WriteString(", ");
                }
                WriteString("Var=");
                WriteString(op.Var.Id.ToString(CultureInfo.InvariantCulture));
            }
        }

        #endregion

        #region dumper helpers

        private void DumpVar(Var v)
        {
            var attrs = new Dictionary<string, object>();

            attrs.Add("Var", v.Id);
            var cv = v as ColumnVar;
            if (null != cv)
            {
                attrs.Add("Name", cv.ColumnMetadata.Name);
                attrs.Add("Type", cv.ColumnMetadata.Type.ToString());
            }
            using (new AutoXml(this, v.GetType().Name, attrs))
            {
            }
        }

        private void DumpVars(List<Var> vars)
        {
            foreach (var v in vars)
            {
                DumpVar(v);
            }
        }

        private void DumpTable(Table table)
        {
            var attrs = new Dictionary<string, object>();

            attrs.Add("Table", table.TableId);
            if (null != table.TableMetadata.Extent)
            {
                attrs.Add("Extent", table.TableMetadata.Extent.Name);
            }

            using (new AutoXml(this, "Table", attrs))
            {
                DumpVars(table.Columns);
            }
        }

        #region ColumnMap dumper

        internal class ColumnMapDumper : ColumnMapVisitor<Dump>
        {
            internal static ColumnMapDumper Instance = new ColumnMapDumper();

            /// <summary>
            /// Private constructor
            /// </summary>
            private ColumnMapDumper()
            {
            }

            #region Helpers

            /// <summary>
            /// Common CollectionColumnMap code
            /// </summary>
            private void DumpCollection(CollectionColumnMap columnMap, Dump dumper)
            {
                if (columnMap.ForeignKeys.Length > 0)
                {
                    using (new AutoXml(dumper, "foreignKeys"))
                    {
                        VisitList(columnMap.ForeignKeys, dumper);
                    }
                }
                if (columnMap.Keys.Length > 0)
                {
                    using (new AutoXml(dumper, "keys"))
                    {
                        VisitList(columnMap.Keys, dumper);
                    }
                }
                using (new AutoXml(dumper, "element"))
                {
                    columnMap.Element.Accept(this, dumper);
                }
            }

            /// <summary>
            /// Common code to produce an the attributes for the dumper's XML node
            /// </summary>
            private static Dictionary<string, object> GetAttributes(ColumnMap columnMap)
            {
                var attrs = new Dictionary<string, object>();
                attrs.Add("Type", columnMap.Type.ToString());
                return attrs;
            }

            #endregion

            /// <summary>
            /// ComplexTypeColumnMap
            /// </summary>
            internal override void Visit(ComplexTypeColumnMap columnMap, Dump dumper)
            {
                using (new AutoXml(dumper, "ComplexType", GetAttributes(columnMap)))
                {
                    if (columnMap.NullSentinel != null)
                    {
                        using (new AutoXml(dumper, "nullSentinel"))
                        {
                            columnMap.NullSentinel.Accept(this, dumper);
                        }
                    }
                    VisitList(columnMap.Properties, dumper);
                }
            }

            /// <summary>
            /// DiscriminatedCollectionColumnMap
            /// </summary>
            internal override void Visit(DiscriminatedCollectionColumnMap columnMap, Dump dumper)
            {
                using (new AutoXml(dumper, "DiscriminatedCollection", GetAttributes(columnMap)))
                {
                    var attrs = new Dictionary<string, object>();
                    attrs.Add("Value", columnMap.DiscriminatorValue);

                    using (new AutoXml(dumper, "discriminator", attrs))
                    {
                        columnMap.Discriminator.Accept(this, dumper);
                    }
                    DumpCollection(columnMap, dumper);
                }
            }

            /// <summary>
            /// EntityColumnMap
            /// </summary>
            internal override void Visit(EntityColumnMap columnMap, Dump dumper)
            {
                using (new AutoXml(dumper, "Entity", GetAttributes(columnMap)))
                {
                    using (new AutoXml(dumper, "entityIdentity"))
                    {
                        VisitEntityIdentity(columnMap.EntityIdentity, dumper);
                    }
                    VisitList(columnMap.Properties, dumper);
                }
            }

            /// <summary>
            /// PolymorphicColumnMap
            /// </summary>
            internal override void Visit(SimplePolymorphicColumnMap columnMap, Dump dumper)
            {
                using (new AutoXml(dumper, "SimplePolymorphic", GetAttributes(columnMap)))
                {
                    using (new AutoXml(dumper, "typeDiscriminator"))
                    {
                        columnMap.TypeDiscriminator.Accept(this, dumper);
                    }
                    var attrs = new Dictionary<string, object>();
                    foreach (var tc in columnMap.TypeChoices)
                    {
                        attrs.Clear();
                        attrs.Add("DiscriminatorValue", tc.Key);
                        using (new AutoXml(dumper, "choice", attrs))
                        {
                            tc.Value.Accept(this, dumper);
                        }
                    }
                    using (new AutoXml(dumper, "default"))
                    {
                        VisitList(columnMap.Properties, dumper);
                    }
                }
            }

            /// <summary>
            /// MultipleDiscriminatorPolymorphicColumnMap
            /// </summary>
            internal override void Visit(MultipleDiscriminatorPolymorphicColumnMap columnMap, Dump dumper)
            {
                using (new AutoXml(dumper, "MultipleDiscriminatorPolymorphic", GetAttributes(columnMap)))
                {
                    using (new AutoXml(dumper, "typeDiscriminators"))
                    {
                        VisitList(columnMap.TypeDiscriminators, dumper);
                    }
                    var attrs = new Dictionary<string, object>();
                    foreach (var tc in columnMap.TypeChoices)
                    {
                        attrs.Clear();
                        attrs.Add("EntityType", tc.Key);
                        using (new AutoXml(dumper, "choice", attrs))
                        {
                            tc.Value.Accept(this, dumper);
                        }
                    }
                    using (new AutoXml(dumper, "default"))
                    {
                        VisitList(columnMap.Properties, dumper);
                    }
                }
            }

            /// <summary>
            /// RecordColumnMap
            /// </summary>
            internal override void Visit(RecordColumnMap columnMap, Dump dumper)
            {
                using (new AutoXml(dumper, "Record", GetAttributes(columnMap)))
                {
                    if (columnMap.NullSentinel != null)
                    {
                        using (new AutoXml(dumper, "nullSentinel"))
                        {
                            columnMap.NullSentinel.Accept(this, dumper);
                        }
                    }
                    VisitList(columnMap.Properties, dumper);
                }
            }

            /// <summary>
            /// RefColumnMap
            /// </summary>
            internal override void Visit(RefColumnMap columnMap, Dump dumper)
            {
                using (new AutoXml(dumper, "Ref", GetAttributes(columnMap)))
                {
                    using (new AutoXml(dumper, "entityIdentity"))
                    {
                        VisitEntityIdentity(columnMap.EntityIdentity, dumper);
                    }
                }
            }

            /// <summary>
            /// SimpleCollectionColumnMap
            /// </summary>
            internal override void Visit(SimpleCollectionColumnMap columnMap, Dump dumper)
            {
                using (new AutoXml(dumper, "SimpleCollection", GetAttributes(columnMap)))
                {
                    DumpCollection(columnMap, dumper);
                }
            }

            /// <summary>
            /// SimpleColumnMap
            /// </summary>
            internal override void Visit(ScalarColumnMap columnMap, Dump dumper)
            {
                var attrs = GetAttributes(columnMap);
                attrs.Add("CommandId", columnMap.CommandId);
                attrs.Add("ColumnPos", columnMap.ColumnPos);

                using (new AutoXml(dumper, "AssignedSimple", attrs))
                {
                }
            }

            /// <summary>
            /// SimpleColumnMap
            /// </summary>
            internal override void Visit(VarRefColumnMap columnMap, Dump dumper)
            {
                var attrs = GetAttributes(columnMap);
                attrs.Add("Var", (columnMap).Var.Id);
                using (new AutoXml(dumper, "VarRef", attrs))
                {
                }
            }

            /// <summary>
            /// DiscriminatedEntityIdentity
            /// </summary>
            protected override void VisitEntityIdentity(DiscriminatedEntityIdentity entityIdentity, Dump dumper)
            {
                using (new AutoXml(dumper, "DiscriminatedEntityIdentity"))
                {
                    using (new AutoXml(dumper, "entitySetId"))
                    {
                        entityIdentity.EntitySetColumnMap.Accept(this, dumper);
                    }
                    if (entityIdentity.Keys.Length > 0)
                    {
                        using (new AutoXml(dumper, "keys"))
                        {
                            VisitList(entityIdentity.Keys, dumper);
                        }
                    }
                }
            }

            /// <summary>
            /// SimpleEntityIdentity
            /// </summary>
            protected override void VisitEntityIdentity(SimpleEntityIdentity entityIdentity, Dump dumper)
            {
                using (new AutoXml(dumper, "SimpleEntityIdentity"))
                {
                    if (entityIdentity.Keys.Length > 0)
                    {
                        using (new AutoXml(dumper, "keys"))
                        {
                            VisitList(entityIdentity.Keys, dumper);
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        internal struct AutoString : IDisposable
        {
            private readonly Dump _dumper;

            internal AutoString(Dump dumper, Op op)
            {
                _dumper = dumper;
                _dumper.WriteString(ToString(op.OpType));
                _dumper.BeginExpression();
            }

            public void Dispose()
            {
                try
                {
                    _dumper.EndExpression();
                }
                catch (Exception e)
                {
                    if (!e.IsCatchableExceptionType())
                    {
                        throw;
                    }
                    // eat this exception; we don't care if the dumper is failing...
                }
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            internal static string ToString(OpType op)
            {
                // perf: Enum.ToString() actually is very perf intensive in time & memory
                switch (op)
                {
                    case OpType.Aggregate:
                        return "Aggregate";
                    case OpType.And:
                        return "And";
                    case OpType.Case:
                        return "Case";
                    case OpType.Cast:
                        return "Cast";
                    case OpType.Collect:
                        return "Collect";
                    case OpType.Constant:
                        return "Constant";
                    case OpType.ConstantPredicate:
                        return "ConstantPredicate";
                    case OpType.CrossApply:
                        return "CrossApply";
                    case OpType.CrossJoin:
                        return "CrossJoin";
                    case OpType.Deref:
                        return "Deref";
                    case OpType.Distinct:
                        return "Distinct";
                    case OpType.Divide:
                        return "Divide";
                    case OpType.Element:
                        return "Element";
                    case OpType.EQ:
                        return "EQ";
                    case OpType.Except:
                        return "Except";
                    case OpType.Exists:
                        return "Exists";
                    case OpType.Filter:
                        return "Filter";
                    case OpType.FullOuterJoin:
                        return "FullOuterJoin";
                    case OpType.Function:
                        return "Function";
                    case OpType.GE:
                        return "GE";
                    case OpType.GetEntityRef:
                        return "GetEntityRef";
                    case OpType.GetRefKey:
                        return "GetRefKey";
                    case OpType.GroupBy:
                        return "GroupBy";
                    case OpType.GroupByInto:
                        return "GroupByInto";
                    case OpType.GT:
                        return "GT";
                    case OpType.In:
                        return "In";
                    case OpType.InnerJoin:
                        return "InnerJoin";
                    case OpType.InternalConstant:
                        return "InternalConstant";
                    case OpType.Intersect:
                        return "Intersect";
                    case OpType.IsNull:
                        return "IsNull";
                    case OpType.IsOf:
                        return "IsOf";
                    case OpType.LE:
                        return "LE";
                    case OpType.Leaf:
                        return "Leaf";
                    case OpType.LeftOuterJoin:
                        return "LeftOuterJoin";
                    case OpType.Like:
                        return "Like";
                    case OpType.LT:
                        return "LT";
                    case OpType.Minus:
                        return "Minus";
                    case OpType.Modulo:
                        return "Modulo";
                    case OpType.Multiply:
                        return "Multiply";
                    case OpType.MultiStreamNest:
                        return "MultiStreamNest";
                    case OpType.Navigate:
                        return "Navigate";
                    case OpType.NE:
                        return "NE";
                    case OpType.NewEntity:
                        return "NewEntity";
                    case OpType.NewInstance:
                        return "NewInstance";
                    case OpType.DiscriminatedNewEntity:
                        return "DiscriminatedNewEntity";
                    case OpType.NewMultiset:
                        return "NewMultiset";
                    case OpType.NewRecord:
                        return "NewRecord";
                    case OpType.Not:
                        return "Not";
                    case OpType.Null:
                        return "Null";
                    case OpType.NullSentinel:
                        return "NullSentinel";
                    case OpType.Or:
                        return "Or";
                    case OpType.OuterApply:
                        return "OuterApply";
                    case OpType.PhysicalProject:
                        return "PhysicalProject";
                    case OpType.Plus:
                        return "Plus";
                    case OpType.Project:
                        return "Project";
                    case OpType.Property:
                        return "Property";
                    case OpType.Ref:
                        return "Ref";
                    case OpType.RelProperty:
                        return "RelProperty";
                    case OpType.ScanTable:
                        return "ScanTable";
                    case OpType.ScanView:
                        return "ScanView";
                    case OpType.SingleRow:
                        return "SingleRow";
                    case OpType.SingleRowTable:
                        return "SingleRowTable";
                    case OpType.SingleStreamNest:
                        return "SingleStreamNest";
                    case OpType.SoftCast:
                        return "SoftCast";
                    case OpType.Sort:
                        return "Sort";
                    case OpType.Treat:
                        return "Treat";
                    case OpType.UnaryMinus:
                        return "UnaryMinus";
                    case OpType.UnionAll:
                        return "UnionAll";
                    case OpType.Unnest:
                        return "Unnest";
                    case OpType.VarDef:
                        return "VarDef";
                    case OpType.VarDefList:
                        return "VarDefList";
                    case OpType.VarRef:
                        return "VarRef";
                    case OpType.ConstrainedSort:
                        return "ConstrainedSort";
                    default:
                        Debug.Assert(false, "need to special case enum->string: " + op.ToString());
                        return op.ToString();
                }
            }
        }

        internal struct AutoXml : IDisposable
        {
            private readonly string _nodeName;
            private readonly Dump _dumper;

            internal AutoXml(Dump dumper, Op op)
            {
                _dumper = dumper;
                _nodeName = AutoString.ToString(op.OpType);

                var attrs = new Dictionary<string, object>();
                if (null != op.Type)
                {
                    attrs.Add("Type", op.Type.ToString());
                }

                _dumper.Begin(_nodeName, attrs);
            }

            internal AutoXml(Dump dumper, Op op, Dictionary<string, object> attrs)
            {
                _dumper = dumper;
                _nodeName = AutoString.ToString(op.OpType);

                var attrs2 = new Dictionary<string, object>();
                if (null != op.Type)
                {
                    attrs2.Add("Type", op.Type.ToString());
                }

                foreach (var kv in attrs)
                {
                    attrs2.Add(kv.Key, kv.Value);
                }

                _dumper.Begin(_nodeName, attrs2);
            }

            internal AutoXml(Dump dumper, string nodeName)
                : this(dumper, nodeName, null)
            {
            }

            internal AutoXml(Dump dumper, string nodeName, Dictionary<string, object> attrs)
            {
                _dumper = dumper;
                _nodeName = nodeName;
                _dumper.Begin(_nodeName, attrs);
            }

            public void Dispose()
            {
                _dumper.End();
            }
        }
    }
}
