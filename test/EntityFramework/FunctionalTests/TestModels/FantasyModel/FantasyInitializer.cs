// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    public class FantasyInitializer : DropCreateDatabaseIfModelChanges<FantasyContext>
    {
        protected override void Seed(FantasyContext context)
        {
            new FantasyDatabaseSeeder().Seed(context);
        }
    }
}
