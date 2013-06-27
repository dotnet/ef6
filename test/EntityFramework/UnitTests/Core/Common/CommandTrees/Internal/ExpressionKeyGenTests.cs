// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Globalization;
    using Xunit;

    public class ExpressionKeyGenTests
    {
        [Fact]
        public void Visit_DateTime_constant_expression_generates_correct_key()
        {
            var dateTime = DateTime.Now;

            var keyGen = new ExpressionKeyGen();
            keyGen.Visit(DbExpressionBuilder.Constant(dateTime));

            var expectedKey = dateTime.ToString("o")
                + ":Edm.DateTime(Nullable=True,DefaultValue=,Precision=)";

            Assert.Equal(expectedKey, keyGen.Key);
        }

        [Fact]
        public void Visit_DateTimeOffset_constant_expression_generates_correct_key()
        {
            var dateTime = DateTime.Now;
            var dateTimeOffset = new DateTimeOffset(dateTime);

            var keyGen = new ExpressionKeyGen();
            keyGen.Visit(DbExpressionBuilder.Constant(dateTimeOffset));

            var expectedKey = dateTimeOffset.ToString("o")
                + ":Edm.DateTimeOffset(Nullable=True,DefaultValue=,Precision=)";

            Assert.Equal(expectedKey, keyGen.Key);
        }
    }
}
