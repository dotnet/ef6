// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using Xunit;

    public class ModelGeneratorUtilsTests
    {
        public class CreateValidEcmaNameTests
        {
            [Fact]
            public void CreateValidEcmaName_does_not_change_valid_ECMA_name()
            {
                Assert.Equal("foo", ModelGeneratorUtils.CreateValidEcmaName("foo", 'a'));
            }

            [Fact]
            public void CreateValidEcmaName_replaces_invalid_ECMA_chars_with_underscore()
            {
                Assert.Equal("f_o", ModelGeneratorUtils.CreateValidEcmaName("f#o", 'a'));
            }

            [Fact]
            public void CreateValidEcmaName_can_handle_empty_name()
            {
                Assert.Equal("a", ModelGeneratorUtils.CreateValidEcmaName(string.Empty, 'a'));
            }

            [Fact]
            public void CreateValidEcmaName_prepends_name_with_ECMA_char_if_the_first_char_was_replaced_with_underscore()
            {
                Assert.Equal("a_foo", ModelGeneratorUtils.CreateValidEcmaName("@foo", 'a'));
            }
        }
    }
}
