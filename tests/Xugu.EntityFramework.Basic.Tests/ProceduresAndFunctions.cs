// Copyright (c) 2013, 2023, Oracle and/or its affiliates. All rights reserved.
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
using System;

namespace Xugu.EntityFramework.Tests
{
    public class ProceduresAndFunctions : DefaultFixture
    {
        public override void TearDown()
        {
            ExecSQL($"DROP FUNCTION IF EXISTS `spFunc`");
        }

        /// <summary>
        /// Validates a stored procedure call using Code First
        /// Bug #14008699
        [Test]
        public void CallStoredProcedure()
        {
            XGCommand cmd = new XGCommand("CREATE PROCEDURE CallStoredProcedure() AS BEGIN SELECT 5; END", Connection);
            cmd.ExecuteNonQuery();

            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                long count = ctx.Database.SqlQuery<long>("call CallStoredProcedure").First();
                Assert.AreEqual(5, count);
            }
        }

        /// <summary>
        /// Bug #45277	Calling User Defined Function using eSql causes NullReferenceException
        /// </summary>
        [Test]
        public void UserDefinedFunction()
        {
            XGCommand cmd = new XGCommand("CREATE OR REPLACE PROCEDURE spFunc(parameter OUT INTEGER) AS BEGIN parameter:=3; END;", Connection);
            cmd.ExecuteNonQuery();

            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                int val = ctx.Database.SqlQuery<int>(@"DECLARE OUTRET INT; BEGIN CALL spFunc(OUTRET); END;").Single();
                Assert.AreEqual(3, val);
            }
        }

        /// <summary>
        /// Bug #56806	Default Command Timeout has no effect in connection string
        /// </summary>
        [Test]
        public void CommandTimeout()
        {
            XGCommand cmd = new XGCommand("CREATE OR REPLACE PROCEDURE spFunc(parameter OUT INTEGER) AS BEGIN SLEEP(5);parameter:=4; END", Connection);
            cmd.ExecuteNonQuery();

            //var sb = new XGConnectionStringBuilder(ConnectionString);
            //sb.DefaultCommandTimeout = 3;
            //sb.UseDefaultCommandTimeoutForEF = true;
            //using (DefaultContext ctx = new DefaultContext(sb.ToString()))
            //{
            //  var exception = Assert.Throws<XuguException>(() =>
            //  {
            //    int val = ctx.Database.SqlQuery<int>(@"SELECT spFunc()").Single();
            //  });
            //}
        }
    }
}