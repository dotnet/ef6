// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace PlanCompilerTests
{
    using System.Data.Entity;
    using System.Linq;
    using AdvancedPatternsModel;
    using Xunit;

    /// <summary>
    ///     Tests for anonymous types in Linq queries.
    /// </summary>
    public class AnonymousTypeTests : FunctionalTestBase
    {
        #region Infrastructure/setup

        #endregion

        #region Tests for anonymous types produced after a join statement

        [Fact]
        private void AnonymousType_join_selecting_one_member()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var query = context.WorkOrders.Select(wo => wo.EmployeeId).Join(
                    context.Employees.Select(e => e.EmployeeId), a => a, b => b, (a, b) => new
                                                                                               {
                                                                                                   a
                                                                                               });
                var sql = query.ToString();
                Assert.True(sql != null);
            }
        }

        [Fact]
        private void AnonymousType_join_selecting_two_members()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var query = context.WorkOrders.Select(wo => wo.EmployeeId).Join(
                    context.Employees.Select(e => e.EmployeeId), a => a, b => b, (a, b) => new
                                                                                               {
                                                                                                   a,
                                                                                                   b
                                                                                               });
                var sql = query.ToString();
                Assert.True(sql != null);
            }
        }

        [Fact]
        private void AnonymousType_join_selecting_one_member_and_one_constant()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var query = context.WorkOrders.Select(wo => wo.EmployeeId).Join(
                    context.Employees.Select(e => e.EmployeeId), a => a, b => b, (a, b) => new
                                                                                               {
                                                                                                   a,
                                                                                                   c = 1
                                                                                               });
                var sql = query.ToString();
                Assert.True(sql != null);
            }
        }

        [Fact]
        private void AnonymousType_join_selecting_two_members_and_one_constant()
        {
            using (var context = new AdvancedPatternsMasterContext())
            {
                var query = context.WorkOrders.Select(wo => wo.EmployeeId).Join(
                    context.Employees.Select(e => e.EmployeeId), a => a, b => b, (a, b) => new
                                                                                               {
                                                                                                   a,
                                                                                                   b,
                                                                                                   c = 1
                                                                                               });
                var sql = query.ToString();
                Assert.True(sql != null);
            }
        }

        #endregion
    }
}
