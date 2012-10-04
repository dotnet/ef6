// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using Xunit;

    public class EdmMemberTests
    {
        private class TestEdmMember : EdmMember
        {
            public override BuiltInTypeKind BuiltInTypeKind
            {
                get { throw new NotImplementedException(); }
            }
        }

        [Fact]
        public void Can_set_type_usage()
        {
            var member = new TestEdmMember();
            var typeUsage = new TypeUsage();

            Assert.Null(member.TypeUsage);

            member.TypeUsage = typeUsage;

            Assert.Same(typeUsage, member.TypeUsage);
        }
    }
}
