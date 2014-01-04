// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    internal class DefaultTransactionHandler : TransactionHandler
    {
        private readonly HashSet<DbTransaction> _transactions = new HashSet<DbTransaction>();

        public override string BuildDatabaseInitializationScript()
        {
            return string.Empty;
        }

        public override void BeganTransaction(DbConnection connection, BeginTransactionInterceptionContext interceptionContext)
        {
            if (!MatchesParentContext(connection, interceptionContext))
            {
                return;
            }

            Debug.Assert(!_transactions.Contains(interceptionContext.Result), "The transaction has already been registered");
            _transactions.Add(interceptionContext.Result);
        }

        public override void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
        {
            if (transaction.Connection != null && !MatchesParentContext(transaction.Connection, interceptionContext)
                || !_transactions.Contains(transaction))
            {
                return;
            }

            _transactions.Remove(transaction);

            if (interceptionContext.Exception != null)
            {
                interceptionContext.Exception = new CommitFailedException(Strings.CommitFailed, interceptionContext.Exception);
            }
        }
    }
}
