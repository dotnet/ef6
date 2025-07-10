// Copyright © 2008, 2018, Oracle and/or its affiliates. All rights reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of XG hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// XG.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of XG Connector/NET, is also subject to the
// Universal FOSS Exception, version 1.0, a copy of which can be found at
// http://oss.oracle.com/licenses/universal-foss-exception.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License, version 2.0, for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace Xugu.Data.EntityFramework
{
    internal abstract class SqlFragment
    {
        static char[] quoteChars = new char[3] { '\'', '"', '`' };

        protected string QuoteIdentifier(string id)
        {
            if (id.IndexOfAny(quoteChars) < 0)
                return String.Format("`{0}`", id);
            else return id;
        }
        protected string QuoteIdentifier(string schema, string id)
        {
            if (id.IndexOfAny(quoteChars) < 0 && schema.IndexOfAny(quoteChars) < 0)
                return String.Format("`{0}`.`{1}`", schema, id);
            else return id;
        }

        public abstract void WriteSql(StringBuilder sql);

        public override string ToString()
        {
            StringBuilder sqlText = new StringBuilder();
            WriteSql(sqlText);
            return sqlText.ToString();
        }

        protected void WriteList(IEnumerable list, StringBuilder sql)
        {
            string sep = "";
            foreach (SqlFragment s in list)
            {
                sql.Append(sep);
                sql.Append("\r\n");
                s.WriteSql(sql);
                sep = ", ";
            }
        }

        internal abstract void Accept(SqlFragmentVisitor visitor);
    }

    /// <summary>
    /// Visitor for SqlFragment and derived classes, useful for postprocessing and optimizations.
    /// </summary>
    internal interface SqlFragmentVisitor
    {
        void Visit(IsNullFragment f);
        void Visit(CaseFragment f);
        void Visit(ExistsFragment f);
        void Visit(LikeFragment f);
        void Visit(FunctionFragment f);
        void Visit(InFragment f);
        void Visit(BinaryFragment f);
        void Visit(ColumnFragment f);
        void Visit(LiteralFragment f);
        void Visit(UnionFragment f);
        void Visit(SortFragment f);
        void Visit(ListFragment f);
        void Visit(SelectStatement f);
        void Visit(TableFragment f);
        void Visit(JoinFragment f);
    }

    /// <summary>
    /// Visitor to replace old table names with new table names. Used as part postprocessing of the code for fusing nested selects.
    /// </summary>
    internal class ReplaceTableNameVisitor : SqlFragmentVisitor
    {
        private string _oldTableName;
        private string _newTableName;
        private Dictionary<string, ColumnFragment> _dicColumns;

        internal ReplaceTableNameVisitor(string oldTableName, string newTableName, Dictionary<string, ColumnFragment> dicColumns)
        {
            _oldTableName = oldTableName;
            _newTableName = newTableName;
            _dicColumns = dicColumns;
        }

        public void Visit(IsNullFragment f)
        {
        }

        public void Visit(CaseFragment f)
        {
        }

        public void Visit(ExistsFragment f)
        {
        }

        public void Visit(LikeFragment f)
        {
        }

        public void Visit(FunctionFragment f)
        {
        }

        public void Visit(InFragment f)
        {
        }

        public void Visit(BinaryFragment f)
        {
        }

        public void Visit(ColumnFragment f)
        {
            ColumnFragment cf;
            if ((f != null) && (f.TableName == _oldTableName))
                f.TableName = _newTableName;
            if (_dicColumns != null && f.ColumnName != null && _dicColumns.TryGetValue(f.ColumnName, out cf))
            {
                // remove alias
                f.ColumnName = cf.ColumnName;
            }
        }

        public void Visit(LiteralFragment f)
        {
            // In Code first, functions like IEnumerable.Contains, are translated to strings (LiteralFragment)
            // instead of a FunctionFragment.
            f.Literal = f.Literal.Replace(string.Format("`{0}`", _oldTableName),
              string.Format("`{0}`", _newTableName));
        }

        public void Visit(UnionFragment f)
        {
        }

        public void Visit(SortFragment f)
        {
        }

        public void Visit(ListFragment f)
        {
        }

        public void Visit(SelectStatement f)
        {
        }

        public void Visit(TableFragment f)
        {
        }

        public void Visit(JoinFragment f)
        {
        }

    }

    internal class BinaryFragment : NegatableFragment
    {
        public SqlFragment Left;
        public SqlFragment Right;
        public string Operator;
        public bool WrapLeft;
        public bool WrapRight;

        public override void WriteSql(StringBuilder sql)
        {
            if (IsNegated && Operator != "=")
                sql.Append("NOT (");

            // do left arg
            if (WrapLeft)
                sql.Append("(");
            Left.WriteSql(sql);
            if (WrapLeft)
                sql.Append(")");

            if (IsNegated && Operator == "=")
                sql.Append(" != ");
            else
                sql.AppendFormat(" {0} ", Operator);

            // now right arg
            if (WrapRight)
                sql.Append("(");
            Right.WriteSql(sql);
            if (WrapRight)
                sql.Append(")");
            if (IsNegated && Operator != "=")
                sql.Append(")");
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            if (Left != null) Left.Accept(visitor);
            if (Right != null) Right.Accept(visitor);
            visitor.Visit(this);
        }
    }

    internal class InFragment : NegatableFragment
    {
        public List<LiteralFragment> InList;
        public SqlFragment Argument;

        internal InFragment()
        {
            InList = new List<LiteralFragment>();
        }

        public override void WriteSql(StringBuilder sql)
        {
            Argument.WriteSql(sql);
            if (IsNegated)
                sql.Append(" NOT ");
            sql.Append(" IN ( ");
            foreach (LiteralFragment lit in InList)
            {
                sql.Append(lit).Append(",");
            }
            sql.Length = sql.Length - 1;
            sql.Append(" )");
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            // Most Accept methods are postorder, this one is preorden due to semantics of ApplyUnionEmulatorVisitor.
            visitor.Visit(this);
            if (Argument != null) Argument.Accept(visitor);
            for (int i = 0; i < InList.Count; i++)
                InList[i].Accept(visitor);
            visitor.Visit(this);
        }
    }

    internal class CaseFragment : SqlFragment
    {
        public List<SqlFragment> When = new List<SqlFragment>();
        public List<SqlFragment> Then = new List<SqlFragment>();
        public SqlFragment Else = null;

        public override void WriteSql(StringBuilder sql)
        {
            sql.Append("CASE");

            // If CASE clause contains an IS NULL value in WHEN statement and has an Else statement, negate WHEN and invert the CASE, 
            // otherwise THEN statement will not be considered and the value will be set to NULL.
            if (When.Count == 1 && When[0] is IsNullFragment && !(((NegatableFragment)When[0]).IsNegated) && Else != null)
            {
                sql.Append(" WHEN (");
                ((NegatableFragment)When[0]).IsNegated = true;
                When[0].WriteSql(sql);
                sql.Append(") THEN (");
                Else.WriteSql(sql);
                sql.Append(") ");
                sql.Append(" ELSE (");
                Then[0].WriteSql(sql);
                sql.Append(") ");
            }
            else
            {
                for (int i = 0; i < When.Count; i++)
                {
                    sql.Append(" WHEN (");
                    When[i].WriteSql(sql);
                    sql.Append(") THEN (");
                    Then[i].WriteSql(sql);
                    sql.Append(") ");
                }
                if (Else != null)
                {
                    sql.Append(" ELSE (");
                    Else.WriteSql(sql);
                    sql.Append(") ");
                }
            }

            sql.Append("END");
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            Else.Accept(visitor);
            for (int i = 0; i < Then.Count; i++)
                Then[i].Accept(visitor);
            for (int i = 0; i < When.Count; i++)
                When[i].Accept(visitor);
            if (Else != null) Else.Accept(visitor);
            visitor.Visit(this);
        }
    }

    internal class ColumnFragment : SqlFragment
    {
        public ColumnFragment(string tableName, string columnName)
        {
            TableName = tableName;
            ColumnName = columnName;
        }

        public SqlFragment Literal { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnAlias { get; set; }
        public string ActualColumnName
        {
            get { return String.IsNullOrEmpty(ColumnName) ? ColumnAlias : ColumnName; }
        }
        public PropertyFragment PropertyFragment { get; set; }

        public override void WriteSql(StringBuilder sql)
        {
            if (Literal != null)
            {
                Debug.Assert(ColumnAlias != null);
                Literal.WriteSql(sql);
            }
            else
            {
                if (TableName != null)
                    sql.AppendFormat("{0}.", QuoteIdentifier(TableName));
                sql.AppendFormat("{0}", QuoteIdentifier(ColumnName));
            }

            if (ColumnAlias != null && ColumnAlias != ColumnName)
                sql.AppendFormat(" AS {0}", QuoteIdentifier(ColumnAlias));
        }

        public ColumnFragment Clone()
        {
            ColumnFragment cf = new ColumnFragment(TableName, ColumnName);
            cf.ColumnAlias = ColumnAlias;
            cf.Literal = Literal;
            return cf;
        }

        public void PushInput(string inputName)
        {
            if (PropertyFragment == null)
                PropertyFragment = new PropertyFragment();
            PropertyFragment.PushProperty(inputName);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ColumnFragment)) return false;
            ColumnFragment column = obj as ColumnFragment;
            if (column.PropertyFragment != null && PropertyFragment != null)
                return column.PropertyFragment.Equals(PropertyFragment);
            if (column.TableName != TableName) return false;
            if (column.ColumnName != ColumnName) return false;
            if (column.ColumnAlias != ColumnAlias) return false;
            return true;
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            if (Literal != null) Literal.Accept(visitor);
            //if (PropertyFragment != null) PropertyFragment.Accept(visitor);
            visitor.Visit(this);
        }
    }

    internal class ExistsFragment : NegatableFragment
    {
        public SqlFragment Argument;

        public ExistsFragment(SqlFragment f)
        {
            Argument = f;
        }

        public override void WriteSql(StringBuilder sql)
        {
            sql.Append(IsNegated ? "NOT " : "");
            sql.Append("EXISTS(");
            Argument.WriteSql(sql);
            sql.Append(")");
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            Argument.Accept(visitor);
            visitor.Visit(this);
        }
    }

    internal class FunctionFragment : SqlFragment
    {
        public bool Distinct;
        public SqlFragment Argument;
        public string Name;
        public bool Quoted;

        public override void WriteSql(StringBuilder sql)
        {
            string name = Quoted ? QuoteIdentifier(Name) : Name;
            sql.AppendFormat("{0}({1}", name, Distinct ? "DISTINCT " : "");
            Argument.WriteSql(sql);
            sql.Append(")");
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            Argument.Accept(visitor);
        }
    }

    internal class IsNullFragment : NegatableFragment
    {
        public SqlFragment Argument;

        public override void WriteSql(StringBuilder sql)
        {
            Argument.WriteSql(sql);
            sql.AppendFormat(" IS {0} NULL", IsNegated ? "NOT" : "");
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            Argument.Accept(visitor);
            visitor.Visit(this);
        }
    }

    internal class LikeFragment : NegatableFragment
    {
        public SqlFragment Argument;
        public SqlFragment Pattern;
        public SqlFragment Escape;

        public override void WriteSql(StringBuilder sql)
        {
            Argument.WriteSql(sql);
            if (IsNegated)
                sql.Append(" NOT ");
            sql.Append(" LIKE ");
            Pattern.WriteSql(sql);
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            Argument.Accept(visitor);
            Pattern.Accept(visitor);
            if (Escape != null) Escape.Accept(visitor);
            visitor.Visit(this);
        }
    }

    internal class ListFragment : SqlFragment
    {
        public List<SqlFragment> Fragments = new List<SqlFragment>();

        public void Append(string s)
        {
            Fragments.Add(new LiteralFragment(s));
        }

        public void Append(SqlFragment s)
        {
            Fragments.Add(s);
        }

        public override void WriteSql(StringBuilder sql)
        {
            foreach (SqlFragment f in Fragments)
                f.WriteSql(sql);
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            for (int i = 0; i < Fragments.Count; i++)
                Fragments[i].Accept(visitor);
            visitor.Visit(this);
        }
    }

    internal class NegatableFragment : SqlFragment
    {
        public bool IsNegated;

        public void Negate()
        {
            IsNegated = !IsNegated;
        }

        public override void WriteSql(StringBuilder sql)
        {
            Debug.Fail("This method should be overridden");
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            // nothing
        }
    }

    internal class LiteralFragment : SqlFragment
    {
        public string Literal;

        public LiteralFragment(string literal)
        {
            Literal = literal;
        }

        public override void WriteSql(StringBuilder sql)
        {
            sql.Append(Literal);
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    internal class PropertyFragment : SqlFragment
    {
        public PropertyFragment()
        {
            Properties = new List<string>();
        }

        public List<string> Properties { get; private set; }

        public override void WriteSql(StringBuilder sql)
        {
            throw new NotImplementedException();
        }

        public string LastProperty
        {
            get { return Properties.Count == 0 ? null : Properties[Properties.Count - 1]; }
        }

        public void Trim(string name)
        {
            int index = Properties.LastIndexOf(name);
            Properties.RemoveRange(index + 1, Properties.Count - index - 1);
        }

        public void PushProperty(string property)
        {
            Properties.Insert(0, property);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PropertyFragment)) return false;
            PropertyFragment prop = obj as PropertyFragment;
            Debug.Assert(Properties != null && prop.Properties != null);
            int aIndex = Properties.Count - 1;
            int bIndex = prop.Properties.Count - 1;
            int cnt = 0;
            while (aIndex >= 0 && bIndex >= 0)
            {
                if (String.Compare(Properties[aIndex--], prop.Properties[bIndex--], true) != 0) return false;
                if (++cnt == 2) break;
            }
            return true;
        }

        public PropertyFragment Clone()
        {
            PropertyFragment newPF = new PropertyFragment();
            foreach (string prop in Properties)
                newPF.Properties.Add(prop);
            return newPF;
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            // nothing
        }
    }

    internal class SortFragment : SqlFragment
    {
        public SortFragment(SqlFragment column, bool ascending)
        {
            Column = column;
            Ascending = ascending;
        }

        public SqlFragment Column
        {
            get;
            set;
        }
        public bool Ascending { get; set; }

        public override void WriteSql(StringBuilder sql)
        {
            ColumnFragment columnFragment = Column as ColumnFragment;
            Debug.Assert(columnFragment != null);
            columnFragment.WriteSql(sql);
            sql.AppendFormat(" {0}", Ascending ? "ASC" : "DESC");
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            Column.Accept(visitor);
            visitor.Visit(this);
        }
    }

    internal class UnionFragment : InputFragment
    {
        public bool Distinct = false;

        public override void WriteInnerSql(StringBuilder sql)
        {
            sql.Append('(');
            Left.WriteSql(sql);
            sql.Append(')');
            sql.Append(Distinct ? " UNION DISTINCT " : " UNION ALL ");
            sql.Append('(');
            Right.WriteSql(sql);
            sql.Append(')');
        }

        public bool HasDifferentNameForColumn(ColumnFragment column)
        {
            Debug.Assert(Left is SelectStatement);
            Debug.Assert(Right is SelectStatement);
            if ((Left as SelectStatement).HasDifferentNameForColumn(column))
                return true;
            return (Right as SelectStatement).HasDifferentNameForColumn(column);
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            visitor.Visit(this);
        }
    }


}
