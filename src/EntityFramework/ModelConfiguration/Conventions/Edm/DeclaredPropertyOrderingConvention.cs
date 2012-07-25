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
    public sealed class DeclaredPropertyOrderingConvention : IEdmConvention<EdmEntityType>
    {
        private const BindingFlags DefaultBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        internal DeclaredPropertyOrderingConvention()
        {
        }

        void IEdmConvention<EdmEntityType>.Apply(EdmEntityType entityType, EdmModel model)
        {
            // Orders the declared properties in the same order as the CLR properties
            entityType.GetClrType().GetProperties(DefaultBindingFlags).Reverse().Each(
                p =>
                    {
                        var edmProperty = entityType.DeclaredProperties.Where(ep => ep.Name == p.Name).SingleOrDefault();
                        if (edmProperty != null)
                        {
                            entityType.DeclaredProperties.Remove(edmProperty);
                            entityType.DeclaredProperties.Insert(0, edmProperty);
                        }
                    });

            // Moves the declared keys to the head of the declared properties list.
            entityType.DeclaredKeyProperties.Reverse().Each(
                p =>
                    {
                        entityType.DeclaredProperties.Remove(p);
                        entityType.DeclaredProperties.Insert(0, p);
                    });
        }
    }
}
