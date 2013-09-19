// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using Xunit;

    public class SqlStringBuilderTests
    {
        [Fact]
        public void AppendKeyWord_should_case_based_on_property()
        {
            var sqlStringBuilder = new SqlStringBuilder();

            sqlStringBuilder.AppendKeyword("select");

            Assert.Equal("select", sqlStringBuilder.ToString());

            sqlStringBuilder.UpperCaseKeywords = true;

            sqlStringBuilder.AppendKeyword(" from");

            Assert.Equal("select FROM", sqlStringBuilder.ToString());
        }

        [Fact]
        public void Methods_should_delegate_to_underlying_string_builder()
        {
            var sqlStringBuilder = new SqlStringBuilder();

            sqlStringBuilder.Append("foo");
            sqlStringBuilder.AppendLine("bar");
            sqlStringBuilder.AppendLine();

            Assert.Equal(10, sqlStringBuilder.Length);

            sqlStringBuilder.Length = 0;

            Assert.Equal(0, sqlStringBuilder.Length);
        }
    }
}
