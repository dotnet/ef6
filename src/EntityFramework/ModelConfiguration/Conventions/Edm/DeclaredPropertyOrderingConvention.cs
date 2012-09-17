// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Convention to move primary key properties to appear first.
    /// </summary>
    public class DeclaredPropertyOrderingConvention : IEdmConvention<EdmEntityType>
    {
        private const BindingFlags DefaultBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public void Apply(EdmEntityType edmDataModelItem, EdmModel model)
        {
            // Orders the declared properties in the same order as the CLR properties
            edmDataModelItem.GetClrType().GetProperties(DefaultBindingFlags).Reverse().Each(
                p =>
                    {
                        var edmProperty = edmDataModelItem.DeclaredProperties.SingleOrDefault(ep => ep.Name == p.Name);
                        if (edmProperty != null)
                        {
                            edmDataModelItem.DeclaredProperties.Remove(edmProperty);
                            edmDataModelItem.DeclaredProperties.Insert(0, edmProperty);
                        }
                    });

            // Moves the declared keys to the head of the declared properties list.
            edmDataModelItem.DeclaredKeyProperties.Reverse().Each(
                p =>
                    {
                        edmDataModelItem.DeclaredProperties.Remove(p);
                        edmDataModelItem.DeclaredProperties.Insert(0, p);
                    });
        }
    }
}
