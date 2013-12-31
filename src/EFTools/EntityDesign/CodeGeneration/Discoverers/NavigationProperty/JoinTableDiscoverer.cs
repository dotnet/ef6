// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    internal class JoinTableDiscoverer : INavigationPropertyConfigurationDiscoverer
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public IFluentConfiguration Discover(NavigationProperty navigationProperty, DbModel model)
        {
            Debug.Assert(navigationProperty != null, "navigationProperty is null.");
            Debug.Assert(model != null, "model is null.");

            var fromEndMember = navigationProperty.FromEndMember;
            var toEndMember = navigationProperty.ToEndMember;

            if (fromEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many
                || toEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many)
            {
                // Doesn't apply
                return null;
            }

            var associationSet = model.ConceptualModel.Container.AssociationSets.First(
                s => s.ElementType == navigationProperty.RelationshipType);
            var associationSetMapping = model.ConceptualToStoreMapping.AssociationSetMappings.First(
                m => m.AssociationSet == associationSet);
            var sourceEndColumnProperties = associationSetMapping.SourceEndMapping.Properties.Select(m => m.Column);
            var targetEndColumnProperties = associationSetMapping.TargetEndMapping.Properties.Select(m => m.Column);
            var storeEntitySet = associationSetMapping.StoreEntitySet;
            var fromEndIsSourceEnd =
                navigationProperty.RelationshipType.RelationshipEndMembers.First() == fromEndMember;
            var leftKeys = fromEndIsSourceEnd
                ? sourceEndColumnProperties.Select(p => p.Name)
                : targetEndColumnProperties.Select(p => p.Name);
            var rightKeys = fromEndIsSourceEnd
                ? targetEndColumnProperties.Select(p => p.Name)
                : sourceEndColumnProperties.Select(p => p.Name);

            // NOTE: Join table names are nondeterministic at runtime, so we'll always
            //       configure them during reverse engineer
            var configuration = new JoinTableConfiguration { Table = storeEntitySet.Table ?? storeEntitySet.Name };

            if (storeEntitySet.Schema != "dbo")
            {
                configuration.Schema = storeEntitySet.Schema;
            }

            var fromEndEntityType = (EntityType)fromEndMember.GetEntityType();

            if (!fromEndEntityType.KeyMembers.Zip(leftKeys, (m, n) => new { KeyMember = m, KeyName = n })
                .All(p => p.KeyName == p.KeyMember.DeclaringType.Name + "_" + p.KeyMember.Name))
            {
                foreach (var key in leftKeys)
                {
                    configuration.LeftKeys.Add(key);
                }
            }

            var toEndEntityType = toEndMember.GetEntityType();

            if (!toEndEntityType.KeyMembers.Zip(rightKeys, (m, n) => new { KeyMember = m, KeyName = n })
                .All(p => p.KeyName == p.KeyMember.DeclaringType.Name + "_" + p.KeyMember.Name))
            {
                foreach (var key in rightKeys)
                {
                    configuration.RightKeys.Add(key);
                }
            }

            return configuration;
        }
    }
}

