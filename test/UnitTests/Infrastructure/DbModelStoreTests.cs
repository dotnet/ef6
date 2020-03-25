// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using Xunit;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Xml.Linq;

    public class DbModelStoreTests : TestBase
    {
        [Fact]
        public void GetDefaultSchema_returns_EdmModelExtensions_DefaultSchema()
        {
            var modelStore = new TestableDbModelStore();
            var defaultSchema = modelStore.CallGetDefaultSchema(typeof(DbContext));
            Assert.Equal(EdmModelExtensions.DefaultSchema, defaultSchema);
        }

        internal class TestableDbModelStore : DbModelStore
        {
            public string CallGetDefaultSchema(Type contextType)
            {
                return GetDefaultSchema(contextType);
            }

            public override DbCompiledModel TryLoad(Type contextType)
            {
                throw new NotImplementedException();
            }

            public override XDocument TryGetEdmx(Type contextType)
            {
                throw new NotImplementedException();
            }

            public override void Save(Type contextType, DbModel model)
            {
                throw new NotImplementedException();
            }
        }
    }
}
