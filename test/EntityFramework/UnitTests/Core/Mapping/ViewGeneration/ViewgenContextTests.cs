// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;
    using Xunit;

    public class ViewgenContextTests
    {
        public class AssociationsSetsAffectingThisWrapperQueryFactoryTests
        {
            [Fact]
            public void Create_should_return_only_fk_associations()
            {
                var filter = new ViewgenContext.OneToOneFkAssociationsForEntitiesFilter();

                var associationType1
                    = new AssociationType("A1", EdmConstants.TransientNamespace, true, DataSpace.CSpace);

                var entityType = new EntityType("E", "N", DataSpace.CSpace);

                associationType1.AddMember(
                    new AssociationEndMember("S", entityType)
                        {
                            RelationshipMultiplicity = RelationshipMultiplicity.One
                        });
                associationType1.AddMember(
                    new AssociationEndMember("T", entityType)
                        {
                            RelationshipMultiplicity = RelationshipMultiplicity.One
                        });

                associationType1.SetReadOnly();

                var associationSet1 = new AssociationSet("AS1", associationType1);

                var associationType2
                    = new AssociationType("A2", EdmConstants.TransientNamespace, false, DataSpace.CSpace);

                associationType2.AddMember(
                    new AssociationEndMember("S", entityType)
                        {
                            RelationshipMultiplicity = RelationshipMultiplicity.One
                        });
                associationType2.AddMember(
                    new AssociationEndMember("T", entityType)
                        {
                            RelationshipMultiplicity = RelationshipMultiplicity.One
                        });

                associationType2.SetReadOnly();

                var associationSet2 = new AssociationSet("AS2", associationType2);

                var query
                    = filter.Filter(
                        new[] { entityType },
                        new[] { associationSet1, associationSet2 });

                Assert.Same(associationSet1, query.Single());
            }

            [Fact]
            public void Create_should_return_only_one_to_one_associations()
            {
                var filter = new ViewgenContext.OneToOneFkAssociationsForEntitiesFilter();

                var associationType1
                    = new AssociationType("A1", EdmConstants.TransientNamespace, true, DataSpace.CSpace);

                var entityType = new EntityType("E", "N", DataSpace.CSpace);

                associationType1.AddMember(
                    new AssociationEndMember("S", entityType)
                        {
                            RelationshipMultiplicity = RelationshipMultiplicity.One
                        });
                associationType1.AddMember(
                    new AssociationEndMember("T", entityType)
                        {
                            RelationshipMultiplicity = RelationshipMultiplicity.One
                        });

                associationType1.SetReadOnly();

                var associationSet1 = new AssociationSet("AS1", associationType1);

                var associationType2
                    = new AssociationType("A2", EdmConstants.TransientNamespace, true, DataSpace.CSpace);

                associationType2.AddMember(new AssociationEndMember("S", entityType));
                associationType2.AddMember(new AssociationEndMember("T", entityType));

                associationType2.SetReadOnly();

                var associationSet2 = new AssociationSet("AS2", associationType2);

                var query
                    = filter.Filter(
                        new[] { entityType },
                        new[] { associationSet1, associationSet2 });

                Assert.Same(associationSet1, query.Single());
            }

            [Fact]
            public void Create_should_return_only_when_target_entity_in_wrapper()
            {
                var filter = new ViewgenContext.OneToOneFkAssociationsForEntitiesFilter();

                var associationType1
                    = new AssociationType("A1", EdmConstants.TransientNamespace, true, DataSpace.CSpace);

                var entityType = new EntityType("E", "N", DataSpace.CSpace);

                associationType1.AddMember(
                    new AssociationEndMember("S", entityType)
                        {
                            RelationshipMultiplicity = RelationshipMultiplicity.One
                        });
                associationType1.AddMember(
                    new AssociationEndMember("T", entityType)
                        {
                            RelationshipMultiplicity = RelationshipMultiplicity.One
                        });

                associationType1.SetReadOnly();

                var associationSet1 = new AssociationSet("AS1", associationType1);

                var associationType2
                    = new AssociationType("A2", EdmConstants.TransientNamespace, true, DataSpace.CSpace);

                associationType2.AddMember(
                    new AssociationEndMember("S", new EntityType("E", "N", DataSpace.CSpace))
                        {
                            RelationshipMultiplicity = RelationshipMultiplicity.One
                        });
                associationType2.AddMember(
                    new AssociationEndMember("T", entityType)
                        {
                            RelationshipMultiplicity = RelationshipMultiplicity.One
                        });

                associationType2.SetReadOnly();

                var associationSet2 = new AssociationSet("AS2", associationType2);

                var query
                    = filter.Filter(
                        new[] { entityType },
                        new[] { associationSet1, associationSet2 });

                Assert.Same(associationSet1, query.Single());
            }
        }
    }
}
