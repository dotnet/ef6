// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Internal.Linq;
    using System.Data.Entity.ModelConfiguration.Internal.UnitTests;
    using Moq;

    public static class MockHelper
    {
        internal static InternalSqlSetQuery CreateInternalSqlSetQuery(string sql, params object[] parameters)
        {
            return new InternalSqlSetQuery(new Mock<InternalSetForMock<FakeEntity>>().Object, sql, false, parameters);
        }

        internal static InternalSqlNonSetQuery CreateInternalSqlNonSetQuery(string sql, params object[] parameters)
        {
            return new InternalSqlNonSetQuery(new Mock<InternalContextForMock>().Object, typeof(object), sql, parameters);
        }
    }
}
