// Copyright (c) 2014, 2021, Oracle and/or its affiliates.
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

using System.Linq;
using NUnit.Framework;

namespace Xugu.EntityFramework.Tests
{
    public class SimpleQuery : DefaultFixture
    {
        [OneTimeTearDown]
        public new void OneTimeTearDown()
        {
            ExecSQL($"DROP DATABASE IF EXISTS `blogcontext`;");
        }

        [Test]
        public void SimpleFindAll()
        {

            using (DefaultContext ctx = GetDefaultContext())
            {
                var q = from b in ctx.Books select b;
                string sql = q.ToString();

                var expected = @"SELECT `Extent1`.`Id`, `Extent1`.`Name`, `Extent1`.`PubDate`, `Extent1`.`Pages`, 
                        `Extent1`.`Author_Id` FROM `Books` AS `Extent1`";
                CheckSql(sql, expected);
            }
        }

        [Test]
        public void SimpleFindAllWithCondition()
        {

            using (DefaultContext ctx = GetDefaultContext())
            {
                var q = from b in ctx.Books where b.Id == 1 select b;
                string sql = q.ToString();

                var expected = @"SELECT `Extent1`.`Id`, `Extent1`.`Name`, `Extent1`.`PubDate`, `Extent1`.`Pages`, 
                        `Extent1`.`Author_Id` FROM `Books` AS `Extent1` WHERE 1 = `Extent1`.`Id`";
                CheckSql(sql, expected);
            }
        }

        /// <summary>
        /// Bug #32358174 - Xugu.DATA.XuguCLIENT.XuguEXCEPTION: TABLE 'DB.DB.DB.TABLE' DOESN'T EXIST
        /// </summary>
        [Test]
        public void TablesWithSchemaWithoutUsingProperty()
        {
            using (BlogContext context = new BlogContext(ConnectionString.Replace("db-simplequery", "blogcontext")))
            {
                context.Database.Connection.Open();
                context.Database.Delete();
                context.Database.CreateIfNotExists();

                context.User.Add(new User { Id = 1, Name = "foo" });
                context.User.Add(new User { Id = 2, Name = "bar" });
                context.SaveChanges();

                var q = (from u in context.User select u).ToArray();
                Assert.IsTrue(q.Length == 2);
            }
        }
    }
}