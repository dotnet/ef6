// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.MappingViews
{
    using System.Data.Entity.Resources;
    using Xunit;

    public class DbMappingViewTests
    {
        [Fact]
        public void Can_create_instance_with_esql()
        {
            const string esql = "SampleEsql";

            var view = new DbMappingView(esql);

            Assert.Equal(esql, view.EntitySql);
        }

        [Fact]
        public void Constructor_validates_preconditions()
        {
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("entitySql"),
                Assert.Throws<ArgumentException>(() => new DbMappingView(null)).Message);
            Assert.Equal(Strings.ArgumentIsNullOrWhitespace("entitySql"),
                Assert.Throws<ArgumentException>(() => new DbMappingView(String.Empty)).Message);
        }
    }
}
