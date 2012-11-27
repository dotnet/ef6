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

            edmDataModelItem.DeclaredKeyProperties
                            .Each(
                                p =>
                                    {
                                        edmDataModelItem.RemoveMember(p);
                                        edmDataModelItem.AddKeyMember(p);
                                    });

            new PropertyFilter(model.Version)
                .GetProperties(edmDataModelItem.GetClrType(), declaredOnly: false, includePrivate: true)
                .Each(
                    p =>
                        {
                            var property
                                = edmDataModelItem
                                    .DeclaredProperties
                                    .SingleOrDefault(ep => ep.Name == p.Name);

                            if ((property != null)
                                && !edmDataModelItem.DeclaredKeyProperties.Contains(property))
                            {
                                edmDataModelItem.RemoveMember(property);
                                edmDataModelItem.AddMember(property);
                            }
                        });
        }
    }
}
