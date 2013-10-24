// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public class ModificationFunctionParameterBindingTests
    {
        [Fact]
        public void Cannot_create_with_null_argument()
        {
            var parameter = new FunctionParameter();
            var memberPath = new ModificationFunctionMemberPath(Enumerable.Empty<EdmMember>(), null);

            Assert.Equal(
                "parameter",
                Assert.Throws<ArgumentNullException>(
                    () => new ModificationFunctionParameterBinding(
                        null, memberPath, false)).ParamName);

            Assert.Equal(
                "memberPath",
                Assert.Throws<ArgumentNullException>(
                    () => new ModificationFunctionParameterBinding(
                        parameter, null, false)).ParamName);
        }

        [Fact]
        public void Can_retrieve_properties()
        {
            var parameter = new FunctionParameter();
            var memberPath = new ModificationFunctionMemberPath(Enumerable.Empty<EdmMember>(), null);
            var parameterBinding = new ModificationFunctionParameterBinding(parameter, memberPath, true);

            Assert.Same(parameter, parameterBinding.Parameter);
            Assert.Same(memberPath, parameterBinding.MemberPath);
            Assert.Equal(true, parameterBinding.IsCurrent);
        }

        [Fact]
        public void SetReadOnly_is_called_on_child_mapping_items()
        {
            var parameter = new FunctionParameter();
            var memberPath = new ModificationFunctionMemberPath(Enumerable.Empty<EdmMember>(), null);
            var parameterBinding = new ModificationFunctionParameterBinding(parameter, memberPath, true);

            Assert.False(memberPath.IsReadOnly);
            parameterBinding.SetReadOnly();
            Assert.True(memberPath.IsReadOnly);
        }
    }
}
