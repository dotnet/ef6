// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Query.LinqToEntities
{
    using System.Linq;
    using Xunit;

    public class DevDiv573440_Context : DbContext
    {
        public DbSet<DevDiv573440_Party> Parties { get; set; }
        public DbSet<DevDiv573440_Contact> Contacts { get; set; }
    }

    public class DevDiv573440_Party
    {
        public int ID { get; set; }
    }

    public class DevDiv573440_Contact
    {
        public int ID { get; set; }
        public int PartyID { get; set; }
        public string Name { get; set; }
    }

    public class ApplyOpRulesTests : FunctionalTestBase
    {
        [Fact]
        public void DevDiv573440_Rule_OuterApplyIntoScalarSubquery_must_run_before_Rule_OuterApplyOverProject()
        {
            const string expectedSql =
@"SELECT
[Extent1].[ID] AS [ID],
(SELECT TOP (1)
        [Extent2].[Name] AS [Name]
        FROM [dbo].[DevDiv573440_Contact] AS [Extent2]
        WHERE [Extent2].[PartyID] = [Extent1].[ID]) AS [C1]
FROM [dbo].[DevDiv573440_Party] AS [Extent1]";

            Database.SetInitializer<DevDiv573440_Context>(null);

            using (var context = new DevDiv573440_Context())
            {
                var query = from party in context.Parties
                            select new { Id = party.ID, contact = context.Contacts.Where(x => x.PartyID == party.ID).Select(x => x.Name).FirstOrDefault() };
                QueryTestHelpers.VerifyQuery(query, expectedSql);
            }
        }
    }
}
