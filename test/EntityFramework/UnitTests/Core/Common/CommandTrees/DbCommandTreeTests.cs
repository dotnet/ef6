// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using Moq;
    using Xunit;

    public class DbCommandTreeTests : TestBase
    {
        public class ConcreteDbCommandTree : DbCommandTree
        {
            public ConcreteDbCommandTree(MetadataWorkspace workspace, DataSpace dataSpace)
                : base(workspace, dataSpace)
            {
            }

            public override DbCommandTreeKind CommandTreeKind
            {
                get { throw new NotImplementedException(); }
            }

            internal override IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters()
            {
                throw new NotImplementedException();
            }

            internal override void DumpStructure(ExpressionDumper dumper)
            {
                throw new NotImplementedException();
            }

            internal override string PrintTree(ExpressionPrinter printer)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Data_space_and_workspace_are_set_from_constructor()
        {
            var workspace = new Mock<MetadataWorkspace>().Object;
            var tree = new ConcreteDbCommandTree(workspace, DataSpace.CSpace);

            Assert.Same(workspace, tree.MetadataWorkspace);
            Assert.Equal(DataSpace.CSpace, tree.DataSpace);
        }
    }
}
