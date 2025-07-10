// Copyright (c) 2013, 2020 Oracle and/or its affiliates.
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

using System.Data;
using XuguClient;
using System.Linq;
using NUnit.Framework;

namespace Xugu.EntityFramework.Tests
{
    public class SetOperators : DefaultFixture
    {
        public override void SetUp()
        {
            LoadData();
        }

        public override void LoadData()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                ctx.Database.Connection.Open();
                ctx.Products.Add(new Product() { Name = "Garbage Truck", MinAge = 8 });
                ctx.Products.Add(new Product() { Name = "Fire Truck", MinAge = 12 });
                ctx.Products.Add(new Product() { Name = "Hula Hoop", MinAge = 18 });
                ctx.SaveChanges();
            }
        }

        [Test]
        public void Any()
        {
            // find all authors that are in our db with no books
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                var q = from a in ctx.Authors where !a.Books.Any() select a;
                string sql = q.ToString();
                CheckSql(sql,
                  @"SELECT `Extent1`.`Id`, `Extent1`.`Name`, `Extent1`.`Age`, `Extent1`.`Address_City`, `Extent1`.`Address_Street`, 
          `Extent1`.`Address_State`, `Extent1`.`Address_ZipCode` FROM `Authors` AS `Extent1` WHERE NOT EXISTS(SELECT
          1 AS `C1` FROM `Books` AS `Extent2` WHERE `Extent1`.`Id` = `Extent2`.`Author_Id`)");
            }
        }

        [Test]
        public void FirstSimple()
        {
            XGCommand cmd = new XGCommand("SELECT Id FROM Products", Connection);
            int id = (int)cmd.ExecuteScalar();

            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                var q = from p in ctx.Products
                        select p;
                Product product = q.First() as Product;

                Assert.AreEqual(id, product.Id);
            }
        }

        [Test]
        public void FirstPredicate()
        {
            XGCommand cmd = new XGCommand("SELECT Id FROM Products WHERE MinAge > 8", Connection);
            int id = (int)cmd.ExecuteScalar();

            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                var q = from p in ctx.Products
                        where p.MinAge > 8
                        select p;
                Product product = q.First() as Product;
                Assert.AreEqual(id, product.Id);
            }
        }
    }
}