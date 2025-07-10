// Copyright (c) 2008, 2021, Oracle and/or its affiliates.
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

using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Text;

namespace Xugu.Data.EntityFramework
{
    class TableFragment : InputFragment
    {
        public string Schema;
        public string Table;
        public SqlFragment DefiningQuery;
        public TypeUsage Type;
        public List<ColumnFragment> Columns;

        public TableFragment()
        {
            Scoped = true;
        }

        public override void WriteSql(StringBuilder sql)
        {
            if (DefiningQuery != null)
                sql.AppendFormat("({0})", DefiningQuery);
            else
                sql.AppendFormat("{0}",
                  string.IsNullOrWhiteSpace(Schema) || Schema.Equals("SYSDBA", System.StringComparison.OrdinalIgnoreCase)
                  ? QuoteIdentifier(Table) : QuoteIdentifier(Schema, Table));
            base.WriteSql(sql);
        }

        internal override void Accept(SqlFragmentVisitor visitor)
        {
            if (Columns != null)
            {
                foreach (ColumnFragment cf in Columns)
                {
                    cf.Accept(visitor);
                }
            }
            visitor.Visit(this);
        }
    }
}