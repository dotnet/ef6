// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Convention to enable cascade delete for any required relationships.
    /// </summary>
    public class OneToManyCascadeDeleteConvention : IEdmConvention<EdmAssociationType>
    {
        public void Apply(EdmAssociationType edmDataModelItem, EdmModel model)
        {
            Contract.Assert(edmDataModelItem.SourceEnd != null);
            Contract.Assert(edmDataModelItem.TargetEnd != null);

            if (edmDataModelItem.IsSelfReferencing() // EF DDL gen will fail for self-ref
                || edmDataModelItem.HasDeleteAction())
            {
                return;
            }

            EdmAssociationEnd principalEnd = null;

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
                principalEnd.DeleteAction = EdmOperationAction.Cascade;
            }
        }
    }
}
