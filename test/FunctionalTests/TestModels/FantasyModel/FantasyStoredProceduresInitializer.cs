// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.FantasyModel
{
    public class FantasyStoredProceduresInitializer : DropCreateDatabaseIfModelChanges<FantasyStoredProceduresContext>
    {
        protected override void Seed(FantasyStoredProceduresContext context)
        {
            new FantasyDatabaseSeeder().Seed(context);
        }
    }
}
