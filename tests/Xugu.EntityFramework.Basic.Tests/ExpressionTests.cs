// Copyright (c) 2015, 2020, Oracle and/or its affiliates. All rights reserved.
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
    public class ExpressionTests : DefaultFixture
    {
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

        /// <summary>
        /// Using StartsWith on a list when using variable as parameter
        /// </summary>
        [Test]
        public void CheckStartsWithWhenUsingVariable()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                ctx.Database.Connection.Open();
                string str = "Garbage";
                var records = ctx.Products.Where(p => p.Name.StartsWith(str)).ToArray();
                Assert.AreEqual(1, records.Count());
            }
        }

        /// <summary>
        /// Using StartsWith on a list when using a hardcoded value
        /// </summary>
        [Test]
        public void CheckStartsWithWhenUsingValue()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                var records = ctx.Products.Where(p => p.Name.StartsWith("Garbage")).ToArray();
                Assert.AreEqual(1, records.Count());
            }
        }

        /// <summary>
        /// Using EndsWith on a list when using a variable as parameter
        /// </summary>
        [Test]
        public void CheckEndsWithWhenUsingVariable()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                ctx.Database.Connection.Open();
                string str = "Hoop";
                var records = ctx.Products.Where(p => p.Name.EndsWith(str)).ToArray();
                Assert.AreEqual(1, records.Count());
            }
        }

        /// <summary>
        /// Using EndsWith on a list when using a hardcoded value
        /// </summary>
        [Test]
        public void CheckEndsWithWhenUsingValue()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                var records = ctx.Products.Where(p => p.Name.EndsWith("Hoop")).ToArray();
                Assert.AreEqual(1, records.Count());
            }
        }


        /// <summary>
        /// Using Contains on a list when using a variable
        /// </summary>
        [Test]
        public void CheckContainsWhenUsingVariable()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                ctx.Database.Connection.Open();
                string str = "bage";
                var records = ctx.Products.Where(p => p.Name.Contains(str)).ToArray();
                Assert.AreEqual(1, records.Count());
            }
        }


        /// <summary>
        /// Using Contains on a list when using a hardcoded value
        /// </summary>
        [Test]
        public void CheckContainsWhenUsingHardCodedValue()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                ctx.Database.Connection.Open();
                var records = ctx.Products.Where(p => p.Name.Contains("bage")).ToArray();
                Assert.AreEqual(1, records.Count());
            }
        }

        /// <summary>
        /// Using Contains on a list when using a hardcoded value
        /// </summary>
        [Test]
        public void CheckContainsWhenUsingHardCodedValueWithPercentageSymbol()
        {
            using (DefaultContext ctx = new DefaultContext(ConnectionString))
            {
                ctx.Database.Connection.Open();
                var records = ctx.Products.Where(p => p.Name.Contains("%")).ToArray();
                Assert.AreEqual(0, records.Count());
            }
        }

    }
}
