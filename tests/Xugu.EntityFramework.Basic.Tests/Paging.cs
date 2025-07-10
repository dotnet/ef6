// Copyright (c) 2014, 2020, Oracle and/or its affiliates. All rights reserved.
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
using System.Linq;
using NUnit.Framework;

namespace Xugu.EntityFramework.Tests
{
    public class Paging : DefaultFixture
    {
        public override void SetUp()
        {
            LoadData();
        }

        void LoadData()
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
        public void Take()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                var q = ctx.Books.Take(2);
                var sql = q.ToString();
                CheckSql(sql,
                  @"SELECT `Id`, `Name`, `PubDate`, `Pages`, `Author_Id` FROM `Books` LIMIT 2");
            }
        }

        [Test]
        public void Skip()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                var q = ctx.Books.OrderBy(b => b.Pages).Skip(3);
                var sql = q.ToString();
                CheckSql(sql,
                  @"SELECT `Extent1`.`Id`, `Extent1`.`Name`, `Extent1`.`PubDate`, `Extent1`.`Pages`, `Extent1`.`Author_Id`
            FROM `Books` AS `Extent1` ORDER BY `Extent1`.`Pages` ASC LIMIT 3,18446744073709551615");
            }
        }

        [Test]
        public void SkipAndTakeSimple()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                var q = ctx.Books.OrderBy(b => b.Pages).Skip(3).Take(4);
                var sql = q.ToString();
                CheckSql(sql,
                  @"SELECT `Extent1`.`Id`, `Extent1`.`Name`, `Extent1`.`PubDate`, `Extent1`.`Pages`, `Extent1`.`Author_Id`
            FROM `Books` AS `Extent1` ORDER BY `Extent1`.`Pages` ASC LIMIT 3,4");
            }
        }

        // <summary>
        // Tests fix for bug #64749 - Entity Framework - Take().Count() fails with EntityCommandCompilationException.
        // </summary>
        [Test]
        public void TakeWithCount()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                ctx.Database.Connection.Open();
                long cnt = ctx.Products.Take(2).LongCount();
                Assert.AreEqual(2, cnt);
            }
        }
    }
}