// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Resources;

    internal class DefaultTransactionHandler : TransactionHandler
    {
        public override string BuildDatabaseInitializationScript()
        {
            return string.Empty;
        }

        public override void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
            if (!MatchesParentContext(transaction.Connection, interceptionContext))
            {
                return;
            }

            if (interceptionContext.Exception != null)
            {
                interceptionContext.Exception = new CommitFailedException(Strings.CommitFailed, interceptionContext.Exception);
            }
        }
    }
}
