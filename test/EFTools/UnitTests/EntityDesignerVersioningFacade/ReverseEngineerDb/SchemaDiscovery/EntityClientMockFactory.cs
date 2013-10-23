// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb.SchemaDiscovery
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity.Core.EntityClient;
    using Moq;

    // intentionally not static because of closures
    internal class EntityClientMockFactory
    {
        public Mock<EntityDataReader> CreateMockEntityDataReader(IList<object[]> returnRows)
        {
            var index = 0;
            object[] currentRow = null;

            var mockReader = new Mock<EntityDataReader>();

            mockReader
                .Setup(r => r.FieldCount)
                .Returns(returnRows != null ? returnRows[0].Length : 0);

            mockReader
                .Setup(r => r.Read())
                .Callback(() => currentRow = returnRows != null && index < returnRows.Count ? returnRows[index++] : null)
                .Returns(() => currentRow != null);

            mockReader
                .Setup(r => r.GetValues(It.IsAny<object[]>()))
                .Callback<object[]>(
                    buffer =>
                        {
                            if (currentRow != null)
                            {
                                Array.Copy(currentRow, buffer, currentRow.Length);
                            }
                        });

            return mockReader;
        }

        public Mock<EntityCommand> CreateMockEntityCommand(IList<object[]> returnRows)
        {
            var mockCommand = new Mock<EntityCommand>();
            mockCommand.CallBase = true;
            mockCommand
                .Setup(c => c.ExecuteReader(It.IsAny<CommandBehavior>()))
                .Returns(CreateMockEntityDataReader(returnRows).Object);

            return mockCommand;
        }
    }
}
