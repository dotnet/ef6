// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    public sealed class ForeignKeyIndexConventionTests
    {
        [Fact]
        public void Apply_should_add_index_when_none_present()
        {
            var associationType
                = new AssociationType("A", "N", false, DataSpace.SSpace)
                {
                    Constraint
                        = new ReferentialConstraint(
                            new AssociationEndMember("F", new EntityType("P", "N", DataSpace.SSpace)),
                            new AssociationEndMember("T", new EntityType("D", "N", DataSpace.SSpace)),
                            new EdmProperty[] { },
                            new[] { new EdmProperty("A"), new EdmProperty("B") })
                };

            (new ForeignKeyIndexConvention()).Apply(associationType, null);

            var consolidatedIndexes
                = ConsolidatedIndex.BuildIndexes(
                    associationType.Name,
                    associationType.Constraint.ToProperties.Select(p => Tuple.Create(p.Name, p)));

            var consolidatedIndex = consolidatedIndexes.Single();

            Assert.Equal("IX_A_B", consolidatedIndex.Index.Name);
            Assert.Equal(new[] { "A", "B" }, consolidatedIndex.Columns);
        }

        [Fact]
        public void Apply_should_not_add_index_when_one_present()
        {
            var associationType
                = new AssociationType("A", "N", false, DataSpace.SSpace)
                {
                    Constraint
                        = new ReferentialConstraint(
                            new AssociationEndMember("F", new EntityType("P", "N", DataSpace.SSpace)),
                            new AssociationEndMember("T", new EntityType("D", "N", DataSpace.SSpace)),
                            new EdmProperty[] { },
                            new[] { new EdmProperty("A"), new EdmProperty("B") })
                };

            (new ForeignKeyIndexConvention()).Apply(associationType, null);

            var consolidatedIndexes
                = ConsolidatedIndex.BuildIndexes(
                    associationType.Name,
                    associationType.Constraint.ToProperties.Select(p => Tuple.Create(p.Name, p)));

            Assert.Equal(1, consolidatedIndexes.Count());

            (new ForeignKeyIndexConvention()).Apply(associationType, null);

            consolidatedIndexes
                = ConsolidatedIndex.BuildIndexes(
                    associationType.Name,
                    associationType.Constraint.ToProperties.Select(p => Tuple.Create(p.Name, p)));

            Assert.Equal(1, consolidatedIndexes.Count());
        }
    }
}
