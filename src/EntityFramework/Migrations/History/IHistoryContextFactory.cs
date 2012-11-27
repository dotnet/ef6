// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.History
{
    using System.Data.Common;

    public interface IHistoryContextFactory
    {
        HistoryContext Create(DbConnection existingConnection, bool contextOwnsConnection, string defaultSchema);
    }
}
