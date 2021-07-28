// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.GearsOfWarModel
{
    public class GearsOfWarInitializer : DropCreateDatabaseIfModelChanges<GearsOfWarContext>
    {
        protected override void Seed(GearsOfWarContext context)
        {
            new GearsOfWarDatabaseSeeder().Seed(context);
        }
    }
}
