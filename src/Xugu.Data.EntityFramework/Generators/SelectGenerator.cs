// Copyright © 2008, 2017, Oracle and/or its affiliates. All rights reserved.
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

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;


namespace Xugu.Data.EntityFramework
{
  class SelectGenerator : SqlGenerator
  {
    Stack<SelectStatement> selectStatements = new Stack<SelectStatement>();    

    #region Properties

    private SelectStatement CurrentSelect
    {
      get { return selectStatements.Count == 0 ? null : selectStatements.Peek(); }
    }

    #endregion

    public override string GenerateSQL(DbCommandTree tree)
    {
      DbQueryCommandTree commandTree = tree as DbQueryCommandTree;

      SqlFragment fragment = null;

      DbExpression e = commandTree.Query;
      switch (commandTree.Query.ExpressionKind)
      {
        case DbExpressionKind.Project:
          fragment = e.Accept(this);
          Debug.Assert(fragment is SelectStatement);
          break;
      }
      // Apply post-optimizations here:
      fragment = TryFlatteningGroupBy(fragment);
      return fragment.ToString();
    }

    /// <summary>
    /// If input sqlFragment is a group by structure, is flattened to remove some nested correlation queries.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private SqlFragment TryFlatteningGroupBy(SqlFragment input)
    { 
      SelectStatement select = null;
      SelectStatement tmpFrag = null, tmpFrag2 = null, tmpFrag3 = null;
      InputFragment table = null;
      string objName = null, colName = null, queryName = null;
      // First part assert is a kind-of structure we are looking for
      tmpFrag = input as SelectStatement;
      if (tmpFrag == null) goto NoChanges;
      tmpFrag = ( tmpFrag ).From as SelectStatement;
      if (tmpFrag == null) goto NoChanges;
      queryName = tmpFrag.Name;
      if (tmpFrag.Columns.Count < 2) goto NoChanges;
      if (!(tmpFrag.Columns[0] is ColumnFragment)) goto NoChanges;
      colName = objName = (tmpFrag.Columns[0] as ColumnFragment).ActualColumnName;
      tmpFrag2 = tmpFrag.From as SelectStatement;
      if (tmpFrag2 == null) goto NoChanges;
      if (tmpFrag2.Columns.Count < 1 || !(tmpFrag2.Columns[0] is ColumnFragment)) goto NoChanges;
      if (string.CompareOrdinal(objName, tmpFrag2.Columns[0].ActualColumnName) != 0) goto NoChanges;
      if (tmpFrag.Columns[1].Literal == null ) goto NoChanges;
      tmpFrag2 = tmpFrag.Columns[1].Literal as SelectStatement;
      if (tmpFrag2 == null) goto NoChanges;
      if ((tmpFrag2.Columns.Count != 1) || !(tmpFrag2.Columns[0].Literal != null)) goto NoChanges;
      tmpFrag3 = tmpFrag2.From as SelectStatement;
      if (tmpFrag3 == null) goto NoChanges;
      table = tmpFrag3.From as InputFragment;
      if (table == null) goto NoChanges;
      FunctionFragment func = tmpFrag2.Columns[0].Literal as FunctionFragment;
      if( tmpFrag3.Columns.Count != 1 || !( tmpFrag3.Columns[ 0 ] is ColumnFragment )) goto NoChanges;
      if (func == null) goto NoChanges;
      // Yes it is the kind-of type we like, then optimize it
      select = new SelectStatement(this);      
      table.Name = null;
      string tableName = null;
      TableFragment t = tmpFrag3.From as TableFragment;
      if (t == null)
        tableName = tmpFrag3.Columns[0].TableName;
      else
        tableName = t.Table;
      select.From = table;
      select.Columns.Add(new ColumnFragment(tableName, colName));
      select.Columns.Add(new ColumnFragment(tableName, "C0")
      {
        ColumnAlias = "C1",
        Literal = new FunctionFragment() {
          Argument = new ColumnFragment(tableName, tmpFrag3.Columns[0].ActualColumnName),
          Distinct = tmpFrag3.IsDistinct,
          Name = func.Name
        }
      });
      select.Wrap(null); 
      select.Name = queryName;
      select.AddGroupBy(select.Columns[0]);
      (input as SelectStatement).From = select;
NoChanges:
      return input;
    }

    public override SqlFragment Visit(DbDistinctExpression expression)
    {
      SelectStatement select = VisitInputExpressionEnsureSelect(expression.Argument, null, null);
      select.IsDistinct = true;
      return select;
    }

    public override SqlFragment Visit(DbFilterExpression expression)
    {
      SelectStatement select = VisitInputExpressionEnsureSelect(expression.Input.Expression,
          expression.Input.VariableName, expression.Input.VariableType);
      select = WrapIfNotCompatible(select, expression.ExpressionKind);
      select.Where = expression.Predicate.Accept(this);
      return select;
    }

    public override SqlFragment Visit(DbGroupByExpression expression)
    {
      // first process the input
      DbGroupExpressionBinding e = expression.Input;
      SelectStatement innerSelect = VisitInputExpressionEnsureSelect(e.Expression, e.VariableName, e.VariableType);
      scope.Add(e.GroupVariableName, innerSelect);

      SelectStatement select = WrapIfNotCompatible(innerSelect, expression.ExpressionKind);

      CollectionType ct = (CollectionType)expression.ResultType.EdmType;
      RowType rt = (RowType)ct.TypeUsage.EdmType;

      int propIndex = 0;

      foreach (DbExpression key in expression.Keys)
      {
        var fragment = key.Accept(this);
        select.AddGroupBy(fragment);
        propIndex++;

        var colFragment = fragment as ColumnFragment;

        if (colFragment != null)
        {
          colFragment = colFragment.Clone();
          colFragment.ColumnAlias = String.Format("K{0}", propIndex);
          select.Columns.Add(colFragment);
        }
      }

      for (int agg = 0; agg < expression.Aggregates.Count; agg++)
      {
        DbAggregate a = expression.Aggregates[agg];
        DbFunctionAggregate fa = a as DbFunctionAggregate;
        if (fa == null) throw new NotSupportedException();

        string alias = rt.Properties[propIndex++].Name;
        ColumnFragment functionCol = new ColumnFragment(null, null);
        functionCol.Literal = HandleFunction(fa, a.Arguments[0].Accept(this));
        functionCol.ColumnAlias = alias;
        select.Columns.Add(functionCol);
      }

      return select;
    }

    private SqlFragment HandleFunction(DbFunctionAggregate fa, SqlFragment arg)
    {
      Debug.Assert(fa.Arguments.Count == 1);

      if (fa.Function.NamespaceName != "Edm")
        throw new NotSupportedException();

      FunctionFragment fragment = new FunctionFragment();
      fragment.Name = fa.Function.Name;
      if (fa.Function.Name == "BigCount")
        fragment.Name = "COUNT";
      else
        fragment.Name = fa.Function.Name.ToUpperInvariant();

      fragment.Distinct = fa.Distinct;
      fragment.Argument = arg;
      return fragment;
      //return new CastExpression(aggregate, GetDbType(functionAggregate.ResultType.EdmType));
    }

    public override SqlFragment Visit(DbCrossJoinExpression expression)
    {
      Debug.Assert(expression.Inputs.Count == 2);
      return HandleJoinExpression(expression.Inputs[0], expression.Inputs[1],
          expression.ExpressionKind, null);
    }

    public override SqlFragment Visit(DbApplyExpression expression)
    {

      DbExpressionBinding inputBinding = expression.Input;
      InputFragment input = VisitInputExpression(inputBinding.Expression, inputBinding.VariableName, inputBinding.VariableType);
      DbExpressionBinding applyBinding = expression.Apply;
      InputFragment apply = VisitInputExpression(applyBinding.Expression, applyBinding.VariableName, applyBinding.VariableType);
      SelectStatement select = new SelectStatement(this);
      bool isInputSelect = false;
      if (!(input is TableFragment))
      {
        input.Wrap(scope);
        isInputSelect = true;
      }
      apply.Wrap(scope);
      select.From = input;
      select.Wrap(scope);
      if (apply is SelectStatement)
      {
        SelectStatement applySel = apply as SelectStatement;
        foreach (ColumnFragment f in applySel.Columns)
        {
          SelectStatement newColSelect = new SelectStatement(this);
          newColSelect.From = applySel.From;
          newColSelect.Where = applySel.Where;
          if (isInputSelect)
          {
            VisitAndReplaceTableName(newColSelect.Where, (input as SelectStatement).From.Name, input.Name, null);
          }
          newColSelect.Limit = applySel.Limit;
          newColSelect.OrderBy = applySel.OrderBy;
          newColSelect.Skip = applySel.Skip;
          newColSelect.GroupBy = applySel.GroupBy;
          newColSelect.IsDistinct = applySel.IsDistinct;
          newColSelect.Columns.Add(f);

          newColSelect.Wrap(scope);
          scope.Add(applySel.From.Name, applySel.From);

          ColumnFragment newCol = new ColumnFragment(apply.Name, f.ColumnName);
          newCol.Literal = newColSelect;
          newCol.PushInput(newCol.ColumnName);
          newCol.PushInput(apply.Name);
          select.AddColumn(newCol, scope);
          if (string.IsNullOrEmpty(newCol.ColumnAlias))
          {
            newColSelect.Name = newCol.ColumnName;
            newCol.ColumnAlias = newCol.ColumnName;
          }
          scope.Remove(newColSelect);
        }
        scope.Remove(applySel.From);
        scope.Remove(apply);
      }
      else if (apply is UnionFragment)
      {
        UnionFragment uf = apply as UnionFragment;
        if (uf.Left == null || uf.Right == null)
          throw new Exception("Union fragment is not properly formed.");

        var left = uf.Left as SelectStatement;
        var right = uf.Right as SelectStatement;

        if (left == null || right == null)
          throw new NotImplementedException();

        var whereleft = left.Where as BinaryFragment;
        var whereright = right.Where as BinaryFragment;

        if (whereleft == null || whereright == null)
          throw new NotImplementedException();
        
        LiteralFragment literalFragmentWhere = null;        

        //checking where left
        if (whereleft.Left is ColumnFragment && whereleft.Right is ColumnFragment)
        {
          // we replace the where part for a dummy one 
          if (whereright.Left is ColumnFragment && whereright.Right is ColumnFragment)
          {
            literalFragmentWhere = new LiteralFragment("1 = 1");               
          }
        }

        if (literalFragmentWhere == null)
          throw new NotImplementedException();

        var leftouterjoin = new JoinFragment();
        leftouterjoin.JoinType = Metadata.GetOperator(DbExpressionKind.LeftOuterJoin);
        leftouterjoin.Name = apply.Name;        

        // validating that column fragment on the left matches the name
        // for the input fragment
        var leftColumnFragment = whereleft.Left as ColumnFragment;

        if (leftColumnFragment == null)
          throw new NotImplementedException();

        if (!leftColumnFragment.TableName.Equals(input.Name))
          new NotImplementedException();

        var conditionJoin = new BinaryFragment();
        conditionJoin.Left = whereleft.Left;

        //update to the new reference
        var newColumnFragment = whereright.Right as ColumnFragment;
        if (newColumnFragment != null)
        {          
          newColumnFragment.TableName = uf.Name;
        }
        conditionJoin.Right = newColumnFragment;
        conditionJoin.Operator = whereleft.Operator;

        leftouterjoin.Condition = conditionJoin;

        (uf.Left as SelectStatement).Where = literalFragmentWhere;
        (uf.Right as SelectStatement).Where = literalFragmentWhere;
        
        leftouterjoin.Left = input;
        leftouterjoin.Right = uf;

        return leftouterjoin;
      }
      return select;
    }

    public override SqlFragment Visit(DbJoinExpression expression)
    { 
      return HandleJoinExpression(expression.Left, expression.Right,
          expression.ExpressionKind, expression.JoinCondition);
    }


    private SqlFragment HandleJoinExpression(DbExpressionBinding left, DbExpressionBinding right,
        DbExpressionKind joinType, DbExpression joinCondition)
    {
      JoinFragment join = new JoinFragment();
      join.JoinType = Metadata.GetOperator(joinType);

      join.Left = VisitInputExpression(left.Expression, left.VariableName, left.VariableType);
      join.Left = WrapJoinInputIfNecessary(join.Left, false);

      join.Right = VisitInputExpression(right.Expression, right.VariableName, right.VariableType);
      join.Right = WrapJoinInputIfNecessary(join.Right, true);

      if (join.Right is SelectStatement)
      {
        SelectStatement select = join.Right as SelectStatement;
        if (select.IsWrapped)
          select.Name = right.VariableName;
      }

      // now handle the ON case
      if (joinCondition != null)
        join.Condition = joinCondition.Accept(this);
      return join;
    }

    public SelectStatement WrapIfNotCompatible(SelectStatement select, DbExpressionKind expressionKind)
    {
      if (select.IsCompatible(expressionKind)) return select;
      SelectStatement newSelect = new SelectStatement(this);
      select.Wrap(scope);
      select.Scoped = true;
      newSelect.From = select;
      return newSelect;
    }

    private InputFragment WrapJoinInputIfNecessary(InputFragment fragment, bool isRightPart)
    {
      if (fragment is SelectStatement || fragment is UnionFragment)
      {
        fragment.Wrap(scope);
        fragment.Scoped = true;
      }
      else if (fragment is JoinFragment && isRightPart)
      {
        SelectStatement select = new SelectStatement(this);
        select.From = fragment;
        select.Name = fragment.Name;
        select.Wrap(scope);
        return select;
      }
      return fragment;
    }

    public override SqlFragment Visit(DbNewInstanceExpression expression)
    {
      Debug.Assert(expression.ResultType.EdmType is CollectionType);

      SelectStatement s = new SelectStatement(this);

      ColumnFragment c = new ColumnFragment(null, null);
      if (expression.Arguments.Count != 0)
        c.Literal = (LiteralFragment)expression.Arguments[0].Accept(this);
      else
        c.Literal = new LiteralFragment("NULL");
      c.ColumnAlias = "X";
      s.Columns.Add(c);
      return s;
    }

    public override SqlFragment Visit(DbProjectExpression expression)
    {
      SelectStatement select = VisitInputExpressionEnsureSelect(expression.Input.Expression,
          expression.Input.VariableName, expression.Input.VariableType);

      // see if we need to wrap this select inside a new select
      select = WrapIfNotCompatible(select, expression.ExpressionKind);

      Debug.Assert(expression.Projection is DbNewInstanceExpression);
      VisitNewInstanceExpression(select, expression.Projection as DbNewInstanceExpression);

      return select;
    }

    private SelectStatement VisitInputExpressionEnsureSelect(DbExpression e, string name, TypeUsage type)
    {
      InputFragment fragment = VisitInputExpression(e, name, type);
      if (fragment is SelectStatement) return (fragment as SelectStatement);

      SelectStatement s = new SelectStatement(this);

      // if the fragment is a union then it needs to be wrapped
      if (fragment is UnionFragment)
        (fragment as UnionFragment).Wrap(scope);

      s.From = fragment;
      return s;
    }

    public override SqlFragment Visit(DbElementExpression expression)
    {
      SelectStatement s = VisitInputExpressionEnsureSelect(expression.Argument, null, null);
      s.Wrap(scope);
      return s;
    }

    public override SqlFragment Visit(DbSortExpression expression)
    {
      SelectStatement select = VisitInputExpressionEnsureSelect(expression.Input.Expression,
          expression.Input.VariableName, expression.Input.VariableType);

      select = WrapIfNotCompatible(select, expression.ExpressionKind);

      foreach (DbSortClause sortClause in expression.SortOrder)
      {
        select.AddOrderBy(new SortFragment(
            sortClause.Expression.Accept(this), sortClause.Ascending));
      }
      return select;
    }

    public override SqlFragment Visit(DbLimitExpression expression)
    {
      SelectStatement select = (SelectStatement)VisitInputExpressionEnsureSelect(
          expression.Argument, null, null);
      select = WrapIfNotCompatible(select, expression.ExpressionKind);
      select.Limit = expression.Limit.Accept(this);
      return select;
    }

    public override SqlFragment Visit(DbSkipExpression expression)
    {
      SelectStatement select = VisitInputExpressionEnsureSelect(expression.Input.Expression, expression.Input.VariableName,
          expression.Input.VariableType);

      select = WrapIfNotCompatible(select, DbExpressionKind.Sort);
      foreach (DbSortClause sortClause in expression.SortOrder)
      {
        select.AddOrderBy(
            new SortFragment(sortClause.Expression.Accept(this), sortClause.Ascending));
      }

      // if we wrapped above, then this wrap will not create a new one so there
      // is no harm in calling it
      select = WrapIfNotCompatible(select, expression.ExpressionKind);
      select.Skip = expression.Count.Accept(this);
      return select;
    }

    public override SqlFragment Visit(DbUnionAllExpression expression)
    {
      UnionFragment f = new UnionFragment();
      Debug.Assert(expression.Left is DbProjectExpression);
      Debug.Assert(expression.Right is DbProjectExpression);
      
      SelectStatement left = VisitInputExpressionEnsureSelect(expression.Left, null, null);
      Debug.Assert(left.Name == null);
      //            left.Wrap(null);

      SelectStatement right = VisitInputExpressionEnsureSelect(expression.Right, null, null);
      Debug.Assert(right.Name == null);
      //          right.Wrap(null);
      f.Left = left;
      f.Right = right;
      
      return f;
    }
  }
}
