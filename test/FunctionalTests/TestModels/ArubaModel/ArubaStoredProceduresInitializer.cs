// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    public class ArubaStoredProceduresInitializer : DropCreateDatabaseIfModelChanges<ArubaStoredProceduresContext>
    {
        protected override void Seed(ArubaStoredProceduresContext context)
        {
            new ArubaDatabaseSeeder().Seed(context);
        }
    }
}


