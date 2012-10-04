// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Diagnostics.Contracts;

    [ContractClass(typeof(KeyDiscoveryConventionContracts))]
    public abstract class KeyDiscoveryConvention : IEdmConvention<EntityType>
    {
        public void Apply(EntityType edmDataModelItem, EdmModel model)
        {
            if ((edmDataModelItem.DeclaredKeyProperties.Count > 0)
                || (edmDataModelItem.BaseType != null))
            {
                return;
            }

            var keyProperty = MatchKeyProperty(edmDataModelItem, edmDataModelItem.GetDeclaredPrimitiveProperties());

            if (keyProperty != null)
            {
                keyProperty.Nullable = false;
                edmDataModelItem.AddKeyMember(keyProperty);
            }
        }

        protected abstract EdmProperty MatchKeyProperty(
            EntityType entityType, IEnumerable<EdmProperty> primitiveProperties);

        #region Base Member Contracts

        [ContractClassFor(typeof(KeyDiscoveryConvention))]
        private abstract class KeyDiscoveryConventionContracts : KeyDiscoveryConvention
        {
            protected override EdmProperty MatchKeyProperty(
                EntityType entityType, IEnumerable<EdmProperty> primitiveProperties)
            {
                Contract.Requires(entityType != null);
                Contract.Requires(primitiveProperties != null);

                return null;
            }
        }

        #endregion
    }
}
