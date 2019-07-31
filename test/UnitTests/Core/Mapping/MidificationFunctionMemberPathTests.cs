// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using Xunit;

    public class MidificationFunctionMemberPathTests
    {
        [Fact]
        public void Cannot_create_with_null_argument()
        {
            var associationType = new AssociationType("AT", "N", false, DataSpace.CSpace);
            var associationSet = new AssociationSet("AS", associationType);

            Assert.Equal(
                "members",
                Assert.Throws<ArgumentNullException>(
                    () => new ModificationFunctionMemberPath(
                        null, associationSet)).ParamName);
        }

        [Fact]
        public void Can_retrieve_properties()
        {
            var source = new EntityType("Source", "N", DataSpace.CSpace);
            var target = new EntityType("Target", "N", DataSpace.CSpace);
            var sourceEnd = new AssociationEndMember("SourceEnd", source);
            var targetEnd = new AssociationEndMember("TargetEnd", target);
            var associationType 
                = AssociationType.Create(
                    "AT",
                    "N",
                    true,
                    DataSpace.CSpace,
                    sourceEnd,
                    targetEnd,
                    null,
                    null);
            var sourceSet = new EntitySet("SourceSet", "S", "T", "Q", source);
            var targetSet = new EntitySet("TargetSet", "S", "T", "Q", target);
            var associationSet
                = AssociationSet.Create(
                    "AS",
                    associationType,
                    sourceSet,
                    targetSet,
                    null);

            var members = new List<EdmMember> { null, targetEnd };
            var memberPath = new ModificationFunctionMemberPath(members, associationSet);

            Assert.Equal(members, memberPath.Members);
            Assert.Equal(targetEnd.Name, memberPath.AssociationSetEnd.Name);
        }
    }
}
