// Copyright (c) 2016, 2020, Oracle and/or its affiliates. All rights reserved.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License, version 2.0, as
// published by the Free Software Foundation.
//
// This program is also distributed with certain software (including
// but not limited to OpenSSL) that is licensed under separate terms,
// as designated in a particular file or component or in included license
// documentation.  The authors of MySQL hereby grant you an
// additional permission to link the program and your derivative works
// with the separately licensed software that they have included with
// MySQL.
//
// Without limiting anything contained in the foregoing, this file,
// which is part of MySQL Connector/NET, is also subject to the
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
using System.Data.Entity.Infrastructure;
using NUnit.Framework;
using System;

namespace Xugu.EntityFramework.CodeFirst.Tests
{
    public class TransactionTests : CodeFirstFixture
    {
        [Test]
        public void DisposeNestedTransactions()
        {
            using (SakilaDb context = new SakilaDb())
            {
                context.Database.Connection.Open();
                using (var trans = context.Database.BeginTransaction())
                {
                    Assert.Throws<Exception>(() => context.Database.ExecuteSqlCommand("update abc"));
                }
            }

            // new second transaction
            using (SakilaDb context = new SakilaDb())
            {
                context.Database.Connection.Open();
                using (var trans = context.Database.BeginTransaction())
                {
                    Assert.Throws<Exception>(() => context.Database.ExecuteSqlCommand("update abc"));
                }
            }
        }

        [Test]
        public void TESTTransactions()
        {
            using (var context = new EducationContext())
            {
                context.Database.Connection.Open();
                using (var trans = context.Database.BeginTransaction())
                {
                    Assert.Throws<Exception>(() => context.Database.ExecuteSqlCommand("update abc"));
                }

                //context.Database.ExecuteSqlCommand("ALTER TABLE `passports` DROP PRIMARY KEY");
            }
        }

        [Test]
        public void NestedTransactionsUniqueKey()
        {
            using (SakilaDb context = new SakilaDb())
            {
                context.Database.Connection.Open();
                context.Database.Initialize(true);
                //context.Database.ExecuteSqlCommand("show auto_commit;");
                //var store = new store
                //{
                //    manager_staff_id = 1
                //};
                //context.stores.Add(store);
                //for (int i = 0; i < 10; i++)
                //{
                //    Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                //}
            }
        }
    }
}
