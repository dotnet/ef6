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

using System.Collections.Generic;
using System;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;


namespace Xugu.Data.EntityFramework
{
    class InsertGenerator : SqlGenerator
    {
        public override string GenerateSQL(DbCommandTree tree)
        {
            DbInsertCommandTree commandTree = tree as DbInsertCommandTree;

            InsertStatement statement = new InsertStatement();

            DbExpressionBinding e = commandTree.Target;
            statement.Target = (InputFragment)e.Expression.Accept(this);

            scope.Add("target", statement.Target);

            foreach (DbSetClause setClause in commandTree.SetClauses)
                statement.Sets.Add(setClause.Property.Accept(this));

            if (values == null)
                values = new Dictionary<EdmMember, SqlFragment>();

            foreach (DbSetClause setClause in commandTree.SetClauses)
            {
                DbExpression value = setClause.Value;
                SqlFragment valueFragment = value.Accept(this);
                statement.Values.Add(valueFragment);

                if (value.ExpressionKind != DbExpressionKind.Null)
                {
                    EdmMember property = ((DbPropertyExpression)setClause.Property).Property;
                    values.Add(property, valueFragment);
                }
            }

            if (commandTree.Returning != null)
                statement.ReturningSelect = GenerateReturningSql(commandTree, commandTree.Returning);

            return statement.ToString();
        }

        protected virtual SelectStatement GenerateReturningSql(DbModificationCommandTree tree, DbExpression returning)
        {
            SelectStatement select = base.GenerateReturningSql(tree, returning);

            ListFragment where = new ListFragment();

            EntitySetBase table = ((DbScanExpression)tree.Target.Expression).Target;
            bool foundIdentity = false;
            where.Append(" rownum > 0");
            foreach (EdmMember keyMember in table.ElementType.KeyMembers)
            {
                SqlFragment value;
                if (!values.TryGetValue(keyMember, out value))
                {
                    if (foundIdentity)
                        throw new NotSupportedException();
                    foundIdentity = true;
                    PrimitiveTypeKind type = ((PrimitiveType)keyMember.TypeUsage.EdmType.BaseType).PrimitiveTypeKind;
                    if ((type == PrimitiveTypeKind.Byte) || (type == PrimitiveTypeKind.SByte) ||
                        (type == PrimitiveTypeKind.Int16) || (type == PrimitiveTypeKind.Int32) ||
                        (type == PrimitiveTypeKind.Int64) || (type == PrimitiveTypeKind.Decimal && IsValidXGDataType(keyMember.TypeUsage.EdmType.FullName)))
                    {
                        value = new LiteralFragment("last_insert_id()");
                    }
                    else if (keyMember.TypeUsage.EdmType.BaseType.Name == "Guid")
                    {
                        value = new LiteralFragment(string.Format("ANY(SELECT guid FROM {0}.tmpIdentity_{1} ORDER BY ROWID DESC LIMIT 1)", (table as MetadataItem).MetadataProperties["Schema"].Value, (table as MetadataItem).MetadataProperties["Table"].Value));
                    }
                }
                where.Append(String.Format(" AND `{0}`=", keyMember));
                where.Append(value);
            }
            select.Where = where;
            return select;
        }

        private bool IsValidXGDataType(string dataType)
        {
            return (new List<string>() { "XG.ubigint" }).Contains(dataType);
        }
    }
}
