// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.GearsOfWarModel
{
    public class GearsOfWarStoredProceduresInitializer : DropCreateDatabaseIfModelChanges<GearsOfWarStoredProceduresContext>
    {
        protected override void Seed(GearsOfWarStoredProceduresContext context)
        {
            new GearsOfWarDatabaseSeeder().Seed(context);
        }
    }
}
