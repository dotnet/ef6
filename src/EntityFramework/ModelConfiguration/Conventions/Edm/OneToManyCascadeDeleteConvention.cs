// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Convention to enable cascade delete for any required relationships.
    /// </summary>
    public sealed class OneToManyCascadeDeleteConvention : IEdmConvention<EdmAssociationType>
    {
        internal OneToManyCascadeDeleteConvention()
        {
        }

        void IEdmConvention<EdmAssociationType>.Apply(EdmAssociationType associationType, EdmModel model)
        {
            Contract.Assert(associationType.SourceEnd != null);
            Contract.Assert(associationType.TargetEnd != null);

            if (associationType.IsSelfReferencing() // EF DDL gen will fail for self-ref
                || associationType.HasDeleteAction())
            {
                return;
            }

            EdmAssociationEnd principalEnd = null;

            if (associationType.IsRequiredToMany())
            {
                principalEnd = associationType.SourceEnd;
            }
            else if (associationType.IsManyToRequired())
            {
                principalEnd = associationType.TargetEnd;
            }

            if (principalEnd != null)
            {
                principalEnd.DeleteAction = EdmOperationAction.Cascade;
            }
        }
    }
}
