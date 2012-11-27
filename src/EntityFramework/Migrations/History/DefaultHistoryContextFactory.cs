// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.Data.Common;
    using System.Data.Entity.Utilities;

    internal class DefaultHistoryContextFactory : IHistoryContextFactory
    {
        public HistoryContext Create(DbConnection existingConnection, bool contextOwnsConnection, string defaultSchema)
        {
            Check.NotNull(existingConnection, "existingConnection");

            return new HistoryContext(existingConnection, contextOwnsConnection, defaultSchema);
        }
    }
}
