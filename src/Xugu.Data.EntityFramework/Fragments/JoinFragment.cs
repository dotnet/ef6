// Copyright © 2008, 2017, Oracle and/or its affiliates. All rights reserved.
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

using System.Text;
using System.Diagnostics;

namespace Xugu.Data.EntityFramework
{
  class JoinFragment : InputFragment
  {
    public SqlFragment Condition;
    public string JoinType;

    public override void WriteInnerSql(StringBuilder sql)
    {
      Left.WriteSql(sql);
      sql.AppendFormat(" {0} ", JoinType);
      Right.WriteSql(sql);
      if (Condition != null)
      {
        sql.Append(" ON ");
        Condition.WriteSql(sql);
      }
    }

    //public override void WriteSql(StringBuilder sql)
    //{
    //    // we don't want our join to write out its name
    //    string name = Name;
    //    Name = null;
    //    base.WriteSql(sql);
    //    Name = name;
    //}

    internal override void Accept(SqlFragmentVisitor visitor)
    {
      base.Accept(visitor);
      if (Condition != null)
        Condition.Accept(visitor);
      visitor.Visit(this);
    }
  }
}
