// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.SqlClient;
    using Xunit;

    public class HistoryOperationTests
    {
        [Fact]
        public void Can_get_and_set_properties()
        {
            var historyOperation = new HistoryOperation(new[] { new DbInsertCommandTree() });

            Assert.NotEmpty(historyOperation.CommandTrees);
        }

        [Fact]
        public void Ctor_should_validate_preconditions()
        {
            Assert.Equal(
                new ArgumentNullException("commandTrees").Message,
                Assert.Throws<ArgumentNullException>(() => new HistoryOperation(null)).Message);
        }
    }
}
