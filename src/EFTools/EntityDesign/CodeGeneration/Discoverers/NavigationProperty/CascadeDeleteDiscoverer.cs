// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;

    internal class CascadeDeleteDiscoverer : INavigationPropertyConfigurationDiscoverer
    {
        public IFluentConfiguration Discover(NavigationProperty navigationProperty, DbModel model)
        {
            Debug.Assert(navigationProperty != null, "navigationProperty is null.");
            Debug.Assert(model != null, "model is null.");

            var fromEndMember = navigationProperty.FromEndMember;
            var toEndMember = navigationProperty.ToEndMember;

            if (fromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many
                && toEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
            {
                // Doesn't apply
                return null;
            }

            var deleteBehavior = fromEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many
                    && toEndMember.RelationshipMultiplicity != RelationshipMultiplicity.One
                ? fromEndMember.DeleteBehavior
                : toEndMember.DeleteBehavior;

            var defaultDeleteBehavior =
                (((fromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One
                                && toEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                            || (fromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many
                                && toEndMember.RelationshipMultiplicity == RelationshipMultiplicity.One))
                        && fromEndMember.GetEntityType() != toEndMember.GetEntityType())
                    ? OperationAction.Cascade
                    : OperationAction.None;

            if (deleteBehavior == defaultDeleteBehavior)
            {
                // By convention
                return null;
            }

            return new CascadeDeleteConfiguration { DeleteBehavior = deleteBehavior };
        }
    }
}

