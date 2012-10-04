// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Convention to enable cascade delete for any required relationships.
    /// </summary>
    public class OneToManyCascadeDeleteConvention : IEdmConvention<AssociationType>
    {
        public void Apply(AssociationType edmDataModelItem, EdmModel model)
        {
            Contract.Assert(edmDataModelItem.SourceEnd != null);
            Contract.Assert(edmDataModelItem.TargetEnd != null);

            if (edmDataModelItem.IsSelfReferencing()) // EF DDL gen will fail for self-ref
            {
                return;
            }

            var configuration = edmDataModelItem.GetConfiguration() as NavigationPropertyConfiguration;

            if ((configuration != null)
                && (configuration.DeleteAction != null))
            {
                return;
            }

            AssociationEndMember principalEnd = null;

            if (edmDataModelItem.IsRequiredToMany())
            {
                principalEnd = edmDataModelItem.SourceEnd;
            }
            else if (edmDataModelItem.IsManyToRequired())
            {
                principalEnd = edmDataModelItem.TargetEnd;
            }

            if (principalEnd != null)
            {
                principalEnd.DeleteBehavior = OperationAction.Cascade;
            }
        }
    }
}
