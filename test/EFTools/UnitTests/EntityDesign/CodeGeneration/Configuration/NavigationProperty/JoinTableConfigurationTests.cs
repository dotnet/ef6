// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using Xunit;

    public class JoinTableConfigurationTests
    {
        [Fact]
        public void GetMethodChain_returns_chain_when_table()
        {
            var configuration = new JoinTableConfiguration { Table = "Subscriptions" };
            var code = new CSharpCodeHelper();

            Assert.Equal(".Map(m => m.ToTable(\"Subscriptions\"))", configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain_when_table_and_schema()
        {
            var configuration = new JoinTableConfiguration { Table = "Subscriptions", Schema = "Sales" };
            var code = new CSharpCodeHelper();

            Assert.Equal(".Map(m => m.ToTable(\"Subscriptions\", \"Sales\"))", configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain_when_one_left_key()
        {
            var configuration = new JoinTableConfiguration { LeftKeys = { "CustomerId" } };
            var code = new CSharpCodeHelper();

            Assert.Equal(".Map(m => m.MapLeftKey(\"CustomerId\"))", configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain_when_more_than_one_left_key()
        {
            var configuration = new JoinTableConfiguration { LeftKeys = { "CustomerId1", "CustomerId2" } };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                ".Map(m => m.MapLeftKey(new[] { \"CustomerId1\", \"CustomerId2\" }))",
                configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain_when_one_right_key()
        {
            var configuration = new JoinTableConfiguration { RightKeys = { "ServiceId" } };
            var code = new CSharpCodeHelper();

            Assert.Equal(".Map(m => m.MapRightKey(\"ServiceId\"))", configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain_when_more_than_one_right_key()
        {
            var configuration = new JoinTableConfiguration { RightKeys = { "ServiceId1", "ServiceId2" } };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                ".Map(m => m.MapRightKey(new[] { \"ServiceId1\", \"ServiceId2\" }))",
                configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chain_when_all()
        {
            var configuration = new JoinTableConfiguration
                {
                    Table = "Subscriptions",
                    LeftKeys = { "CustomerId" },
                    RightKeys = { "ServiceId" }
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                ".Map(m => m.ToTable(\"Subscriptions\").MapLeftKey(\"CustomerId\").MapRightKey(\"ServiceId\"))",
                configuration.GetMethodChain(code));
        }
    }
}
