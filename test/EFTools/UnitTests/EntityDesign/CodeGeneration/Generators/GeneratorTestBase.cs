// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    public class GeneratorTestBase
    {
        private static DbModel _model;

        protected static DbModel Model
        {
            get
            {
                if (_model == null)
                {
                    var modelBuilder = new DbModelBuilder();
                    modelBuilder.Entity<Entity>();
                    _model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
                }

                return _model;
            }
        }

        private class Entity
        {
            public int Id { get; set; }
        }
    }
}
