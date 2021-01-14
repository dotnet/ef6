// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    public class ArubaInitializer : DropCreateDatabaseIfModelChanges<ArubaContext>
    {
        protected override void Seed(ArubaContext context)
        {
            new ArubaDatabaseSeeder().Seed(context);
        }
    }
}
