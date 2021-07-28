// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Reflection;
    using System.Transactions;
    using Xunit.Sdk;

    public class AutoRollbackAttribute : BeforeAfterTestAttribute
    {
        private TransactionScope _transactionScope;

        public override void Before(MethodInfo methodUnderTest)
            => _transactionScope = new TransactionScope();

        public override void After(MethodInfo methodUnderTest)
            => _transactionScope.Dispose();
    }
}
