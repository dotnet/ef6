// Copyright © 2008, 2018, Oracle and/or its affiliates. All rights reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of Xugu hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// Xugu.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of Xugu Connector/NET, is also subject to the
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

using XuguClient;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;


namespace Xugu.Data.EntityFramework
{
  class UpdateGenerator : SqlGenerator
  {
    private bool _onReturningSelect;

    public override string GenerateSQL(DbCommandTree tree)
    {
      DbUpdateCommandTree commandTree = tree as DbUpdateCommandTree;

      UpdateStatement statement = new UpdateStatement();

      _onReturningSelect = false;
      statement.Target = commandTree.Target.Expression.Accept(this);
      scope.Add("target", statement.Target as InputFragment);

      if (values == null)
        values = new Dictionary<EdmMember, SqlFragment>();

      foreach (DbSetClause setClause in commandTree.SetClauses)
      {
        statement.Properties.Add(setClause.Property.Accept(this));
        DbExpression value = setClause.Value;
        SqlFragment valueFragment = value.Accept(this);
        statement.Values.Add(valueFragment);

        if (value.ExpressionKind != DbExpressionKind.Null)
        {
          EdmMember property = ((DbPropertyExpression)setClause.Property).Property;
          values.Add(property, valueFragment);
        }
      }
      
      statement.Where = commandTree.Predicate.Accept(this);

      _onReturningSelect = true;
      if (commandTree.Returning != null)
        statement.ReturningSelect = GenerateReturningSql(commandTree, commandTree.Returning);

      return statement.ToString();
    }

    protected override SelectStatement GenerateReturningSql(DbModificationCommandTree tree, DbExpression returning)
    {
      SelectStatement select = base.GenerateReturningSql(tree, returning);
      ListFragment where = new ListFragment();
      where.Append(" rownum = 1 and (");
      where.Append( ((DbUpdateCommandTree)tree).Predicate.Accept(this) );
      where.Append(")");
      select.Where = where;

      return select;
    }

    private Stack<EdmMember> _columnsVisited = new Stack<EdmMember>();

    public override SqlFragment Visit(DbAndExpression expression)
    {
      if (_onReturningSelect)
      {
        if (IsExcludedCondition(expression.Left))
        {
          return expression.Right.Accept(this);
        }

        if (IsExcludedCondition(expression.Right))
        {
          return expression.Left.Accept(this);
        }
      }

      return base.Visit(expression);
    }

    private bool IsExcludedCondition(DbExpression e)
    {
      var expr = e as DbComparisonExpression;
      if (expr == null) return false;
      var propExpr = expr.Left as DbPropertyExpression;
      if (propExpr == null) return false;

      Facet item = null;
      if (propExpr.Property.TypeUsage.Facets.TryGetValue("StoreGeneratedPattern", false, out item))
      {
        return (StoreGeneratedPattern)item.Value == StoreGeneratedPattern.Computed;
      }
      return false;
    }

    protected override SqlFragment VisitBinaryExpression(DbExpression left, DbExpression right, string op)
    {
      BinaryFragment f = new BinaryFragment();
      f.Operator = op;
      f.Left = left.Accept(this);
      f.WrapLeft = ShouldWrapExpression(left);
      if (f.Left is ColumnFragment)
      {
        _columnsVisited.Push( (( DbPropertyExpression )left ).Property );
      }
      f.Right = right.Accept(this);
      if (f.Left is ColumnFragment)
      {
        _columnsVisited.Pop();
      }
      f.WrapRight = ShouldWrapExpression(right);
      return f;
    }

    public override SqlFragment Visit(DbConstantExpression expression)
    {
      SqlFragment value = null;
      if ( _onReturningSelect && values.TryGetValue(_columnsVisited.Peek(), out value))
      {
        if (value is LiteralFragment)
        {
          XGParameters par = Parameters.Find(p => p.ParameterName == ( value as LiteralFragment ).Literal );
          if (par != null)
            return new LiteralFragment(par.ParameterName);
        }
      }
      return base.Visit(expression);
    }
  }
}
