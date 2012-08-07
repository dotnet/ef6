// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace ProductivityApiUnitTests
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.Infrastructure;
    using System.Xml;
    using Moq;
    using Xunit;

    /// <summary>
    ///     Unit tests for WriteEdmx methods.
    /// </summary>
    public class WriteEdmxTests : TestBase
    {
        #region Tests for arguments to WriteEdmx methods

        [Fact]
        public void Context_based_WriteEdmx_throws_when_given_null_context()
        {
            Assert.Equal(
                "context",
                Assert.Throws<ArgumentNullException>(() => EdmxWriter.WriteEdmx((DbContext)null, new Mock<XmlWriter>().Object)).ParamName);
        }

        [Fact]
        public void Context_based_WriteEdmx_throws_when_given_null_writer()
        {
            Assert.Equal(
                "writer", Assert.Throws<ArgumentNullException>(() => EdmxWriter.WriteEdmx(new Mock<DbContext>().Object, null)).ParamName);
        }

        [Fact]
        public void Model_based_WriteEdmx_throws_when_given_null_model()
        {
            Assert.Equal(
                "model",
                Assert.Throws<ArgumentNullException>(() => EdmxWriter.WriteEdmx((DbModel)null, new Mock<XmlWriter>().Object)).ParamName);
        }

        [Fact]
        public void Model_based_WriteEdmx_throws_when_given_null_writer()
        {
            Assert.Equal(
                "writer",
                Assert.Throws<ArgumentNullException>(
                    () => EdmxWriter.WriteEdmx(new Mock<DbModel>(new DbDatabaseMapping(), new DbModelBuilder()).Object, null)).ParamName);
        }

        #endregion
    }
}
