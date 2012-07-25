// ﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
using System;
using System.Data.Objects;
using Xunit;

namespace ProviderTests
{
    public sealed class CreateDatabaseTests
    {
        [Fact]
        public void GenerateDatabaseScript()
        {
            Assert.True(!string.IsNullOrWhiteSpace(new ObjectContext("name=NorthwindAttach").CreateDatabaseScript()));
        }

        [Fact]
        public void CreateDatabase()
        {
            using (var nwEntities = new ObjectContext("name=NorthwindAttach"))
            {
                if (nwEntities.DatabaseExists())
                {
                    nwEntities.DeleteDatabase();
                }

                nwEntities.CreateDatabase();

                Assert.True(nwEntities.DatabaseExists());
            }
        }
    }
}
