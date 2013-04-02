// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Mappers;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Convention to move primary key properties to appear first.
    /// </summary>
    public class DeclaredPropertyOrderingConvention : IEdmConvention<EntityType>
    {
        public void Apply(EntityType edmDataModelItem, EdmModel model)
        {
            Check.NotNull(edmDataModelItem, "edmDataModelItem");
            Check.NotNull(model, "model");

            if (edmDataModelItem.BaseType == null)
            {
                // Performance: avoid converting to .Each<>() Linq expressions in order to avoid closure allocations   
                foreach (var p in edmDataModelItem.KeyProperties)
                {
                    edmDataModelItem.RemoveMember(p);
                    edmDataModelItem.AddKeyMember(p);
                }

                foreach (var p in 
                    new PropertyFilter()
                    .GetProperties(edmDataModelItem.GetClrType(), declaredOnly: false, includePrivate: true))
                {
                    var property
                        = edmDataModelItem
                            .DeclaredProperties
                            .SingleOrDefault(ep => ep.Name == p.Name);

                    if ((property != null)
                        && !edmDataModelItem.KeyProperties.Contains(property))
                    {
                        edmDataModelItem.RemoveMember(property);
                        edmDataModelItem.AddMember(property);
                    }
                }
            }
        }
    }
}
