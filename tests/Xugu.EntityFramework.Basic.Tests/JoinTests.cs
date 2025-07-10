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


using System.Linq;
using NUnit.Framework;

namespace Xugu.EntityFramework.Tests
{
    public class JoinTests : DefaultFixture
    {
        [Test]
        public void SimpleJoin()
        {
            using (DefaultContext ctx = GetDefaultContext())
            {
                var q = from b in ctx.Books
                        join a in ctx.Authors
                        on b.Author.Id equals a.Id
                        select new
                        {
                            bookId = b.Id,
                            bookName = b.Name,
                            authorName = a.Name
                        };
                var expected = @"SELECT `Extent1`.`Id`, `Extent1`.`Name`, `Extent2`.`Name` AS `Name1`
                        FROM `Books` AS `Extent1` INNER JOIN `Authors` AS `Extent2` ON `Extent1`.`Author_Id` = `Extent2`.`Id`";
                CheckSql(q.ToString(), expected);
            }
        }

        [Test]
        public void SimpleJoinWithPredicate()
        {
            using (DefaultContext ctx = GetDefaultContext())
            {
                var q = from b in ctx.Books
                        join a in ctx.Authors
                        on b.Author.Id equals a.Id
                        where b.Pages > 300
                        select new
                        {
                            bookId = b.Id,
                            bookName = b.Name,
                            authorName = a.Name
                        };

                var expected = @"SELECT `Extent1`.`Id`, `Extent1`.`Name`, `Extent2`.`Name` AS `Name1` FROM `Books` AS `Extent1` 
                        INNER JOIN `Authors` AS `Extent2` ON `Extent1`.`Author_Id` = `Extent2`.`Id`
                        WHERE `Extent1`.`Pages` > 300";
                CheckSql(q.ToString(), expected);
            }
        }

        [Test]
        public void JoinOnRightSideAsDerivedTable()
        {
            using (DefaultContext ctx = GetDefaultContext())
            {
                var q = from b in ctx.Books
                        join a in ctx.ContractAuthors
                        on b.Author.Id equals a.Author.Id
                        where b.Pages > 300
                        select b;
                string sql = q.ToString();
                var expected = @"SELECT `Extent1`.`Id`, `Extent1`.`Name`, `Extent1`.`PubDate`, `Extent1`.`Pages`, `Extent1`.`Author_Id`
                        FROM `Books` AS `Extent1` INNER JOIN `ContractAuthors` AS `Extent2` ON (`Extent1`.`Author_Id` = 
                        `Extent2`.`Author_Id`) OR ((`Extent1`.`Author_Id` IS  NULL) AND (`Extent2`.`Author_Id` IS  NULL))
                        WHERE `Extent1`.`Pages` > 300";
                CheckSql(sql, expected);
            }
        }

        //    [Test]
        //    public void JoinOfUnionsOnRightSideofJoin()
        //    {
        //      using (testEntities context = new testEntities())
        //      {
        //        string eSql = @"SELECT c.Id, c.Name, Union1.Id, Union1.Name, 
        //                                Union2.Id, Union2.Name FROM 
        //                                testEntities.Companies AS c JOIN (
        //                                ((SELECT t.Id, t.Name FROM testEntities.Toys as t) 
        //                                UNION ALL 
        //                                (SELECT s.Id, s.Name FROM testEntities.Shops as s)) AS Union1
        //                                JOIN 
        //                                ((SELECT a.Id, a.Name FROM testEntities.Authors AS a) 
        //                                UNION ALL 
        //                                (SELECT b.Id, b.Name FROM testEntities.Books AS b)) AS Union2
        //                                ON Union1.Id = Union2.Id) ON c.Id = Union1.Id";
        //        ObjectQuery<DbDataRecord> query = context.CreateQuery<DbDataRecord>(eSql);

        //        string[] data = new string[] { 
        //          "1,Hasbro,1,Slinky,1,Tom Clancy",
        //          "1,Hasbro,1,Target,1,Tom Clancy",
        //          "1,Hasbro,1,Slinky,1,Debt of Honor",
        //          "1,Hasbro,1,Target,1,Debt of Honor",
        //          "2,Acme,2,K-Mart,2,Insomnia",
        //          "2,Acme,2,Rubiks Cube,2,Stephen King",
        //          "2,Acme,2,K-Mart,2,Stephen King",
        //          "2,Acme,2,Rubiks Cube,2,Insomnia",
        //          "3,Bandai America,3,Wal-Mart,3,Rainmaker",
        //          "3,Bandai America,3,Lincoln Logs,3,John Grisham",
        //          "3,Bandai America,3,Wal-Mart,3,John Grisham",
        //          "3,Bandai America,3,Lincoln Logs,3,Rainmaker",
        //          "4,Lego Group,4,Legos,4,Hallo",
        //          "4,Lego Group,4,Legos,4,Dean Koontz" 
        //        };
        //        Dictionary<string, string> outData = new Dictionary<string, string>();
        //        string sql = query.ToTraceString();
        //        CheckSql(sql, SQLSyntax.JoinOfUnionsOnRightSideOfJoin);
        //        // check data integrity
        //        foreach (DbDataRecord record in query)
        //        {
        //          outData.Add(string.Format("{0},{1},{2},{3},{4},{5}", record.GetInt32( 0 ),
        //            record.GetString( 1 ), record.GetInt32( 2 ), record.GetString( 3 ), 
        //            record.GetInt32( 4 ), record.GetString( 5 )), null);
        //        }
        //        Assert.AreEqual(data.Length, outData.Count);
        //        for( int i = 0; i < data.Length; i++ )
        //        {
        //          Assert.True(outData.ContainsKey(data[i]));
        //        }
        //      }
        //    }

        //    [Test]
        //    public void t()
        //    {
        //      using (testEntities context = new testEntities())
        //      {
        //        var q = from d in context.Computers
        //                join l in context.Computers on d.Id equals l.Id
        //                where (d is DesktopComputer)
        //                select new { d.Id, d.Brand };
        //        foreach (var o in q)
        //        {
        //        }
        //      }
        //    }

        //    /// <summary>
        //    /// Tests for bug http://bugs.Xugu.com/bug.php?id=61729 
        //    /// (Skip/Take Clauses Causes Null Reference Exception in 6.3.7 and 6.4.1 Only).
        //    /// </summary>
        //    [Test]
        //    public void JoinOfNestedUnionsWithLimit()
        //    {
        //      using (testEntities context = new testEntities())
        //      {
        //        var q = context.Books.Include("Author");
        //        q = q.Include("Publisher");
        //        q = q.Include("Publisher.Books");
        //        string sql = q.ToTraceString();

        //        var  i = 0;
        //        foreach (var o in q.Where(p => p.Id > 0).OrderBy(p => p.Name).ThenByDescending(p => p.Id).Skip(0).Take(32).ToList())
        //        {
        //           switch (i)
        //            {
        //             case 0:
        //               Assert.AreEqual(5, o.Id);
        //               Assert.AreEqual("Debt of Honor", o.Name);
        //             break;
        //             case 1:
        //               Assert.AreEqual(1, o.Id);
        //               Assert.AreEqual("Debt of Honor", o.Name);
        //             break;
        //             case 4:
        //               Assert.AreEqual(3, o.Id);
        //               Assert.AreEqual("Rainmaker", o.Name);
        //             break;             
        //            }
        //           i++;
        //        }
        //      }
        //    }

        //    [Test]
        //    public void JoinOnRightSideNameClash()
        //    {
        //      using (testEntities context = new testEntities())
        //      {
        //        string eSql = @"SELECT c.Id, c.Name, a.Id, a.Name, b.Id, b.Name FROM
        //                                testEntities.Companies AS c JOIN (testEntities.Authors AS a
        //                                JOIN testEntities.Books AS b ON a.Id = b.Id) ON c.Id = a.Id";
        //        ObjectQuery<DbDataRecord> query = context.CreateQuery<DbDataRecord>(eSql);
        //        string sql = query.ToTraceString();
        //        CheckSql(sql, SQLSyntax.JoinOnRightSideNameClash);
        //        foreach (DbDataRecord record in query)
        //        {
        //          Assert.AreEqual(6, record.FieldCount);
        //        }
        //      }
        //    }

        //    /// <summary>
        //    /// Test for fix of Oracle bug 12807366.
        //    /// </summary>
        //    [Test]
        //    public void JoinsAndConcatsWithComposedKeys()
        //    {
        //      using (testEntities1 ctx = new testEntities1())
        //      {
        //        IQueryable<gamingplatform> l2 = ctx.gamingplatform.Where(p => string.IsNullOrEmpty(p.Name)).Take(10);
        //        IQueryable<gamingplatform> l = ctx.gamingplatform.Where(p => string.IsNullOrEmpty(p.Name)).Take(10);
        //        var l4 = ctx.gamingplatform.Where(p => string.IsNullOrEmpty(p.Name)).Take(10);
        //        l = l.Concat(l4);
        //        l = l.Concat(ctx.gamingplatform.Where(p => string.IsNullOrEmpty(p.Name)).Take(10).Distinct());

        //        IQueryable<gamingplatform> q = (from i in l join i2 in l2 on i.Id equals i2.Id select i);
        //        IQueryable<videogameplatform> l3 = from t1 in q
        //                                           join t2 in q.SelectMany(p => p.videogameplatform)
        //                                               on t1.Id equals t2.IdGamingPlatform
        //                                           select t2;
        //        videogameplatform pu = null;

        //        pu = l3.FirstOrDefault();
        //      }
        //    }

        //    /// <summary>
        //    /// Test to fix Oracle bug 13491698
        //    /// </summary>
        //    [Test]
        //    public void CanIncludeWithEagerLoading()
        //    {
        //      var myarray = new ArrayList();
        //      using (var db = new mybooksEntities())
        //      {
        //        var author = db.myauthors.Include("mybooks.myeditions").AsEnumerable().First();
        //        var strquery = ((ObjectQuery)db.myauthors.Include("mybooks.myeditions").AsEnumerable()).ToTraceString();
        //        CheckSql(strquery, SQLSyntax.JoinUsingInclude);
        //        foreach (var book in author.mybooks.ToList())
        //        {
        //          foreach (var edition in book.myeditions.ToList())
        //          {
        //            myarray.Add(edition.Title);
        //          }
        //        }
        //        myarray.Sort();
        //        Assert.AreEqual(0, myarray.IndexOf("Another Book First Edition"));
        //        Assert.AreEqual(1, myarray.IndexOf("Another Book Second Edition"));
        //        Assert.AreEqual(2, myarray.IndexOf("Another Book Third Edition"));
        //        Assert.AreEqual(3, myarray.IndexOf("Some Book First Edition"));
        //        Assert.AreEqual(myarray.Count, 4);
        //      }
        //    }

        //    /// <summary>
        //    /// Tests Fix for Error of "Every derived table must have an alias" in LINQ to Entities when using EF6 + DbFirst + View + Take  (Xugu Bug #72148, Oracle bug #19356006).
        //    /// </summary>
        //    [Test]
        //    public void TakeWithView()
        //    {
        //      using (testEntities1 ctx = new testEntities1())
        //      {        
        //        var q = ctx.vivideogametitle.Take(10);
        //        string sql = q.ToTraceString();
        //        CheckSql(sql, SQLSyntax.TakeWithView);
        //#if DEBUG
        //        Debug.WriteLine(sql);
        //#endif
        //        foreach( var row in q )
        //        {
        //          //
        //        }
        //      }
        //    }
    }
}