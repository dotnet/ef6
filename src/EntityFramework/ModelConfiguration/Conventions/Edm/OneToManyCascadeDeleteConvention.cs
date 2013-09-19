// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Convention to enable cascade delete for any required relationships.
    /// </summary>
    public class OneToManyCascadeDeleteConvention : IConceptualModelConvention<AssociationType>
    {
        /// <inheritdoc />
        public virtual void Apply(AssociationType item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            Debug.Assert(item.SourceEnd != null);
            Debug.Assert(item.TargetEnd != null);

            if (item.IsSelfReferencing()) // EF DDL gen will fail for self-ref
            {
                return;
            }

            var configuration = item.GetConfiguration() as NavigationPropertyConfiguration;

            if ((configuration != null)
                && (configuration.DeleteAction != null))
            {
                return;
            }

            AssociationEndMember principalEnd = null;

            if (item.IsRequiredToMany())
            {
                principalEnd = item.SourceEnd;
            }
            else if (item.IsManyToRequired())
            {
                principalEnd = item.TargetEnd;
            }

            if (principalEnd != null)
            {
                principalEnd.DeleteBehavior = OperationAction.Cascade;
            }
        }
    }
}
