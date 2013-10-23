// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesignerTestInfrastructure.EFDesigner
{
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal static class EntityTypeExtensions
    {
        public static Property GetProperty(this EntityType entityType, string propertyName)
        {
            Debug.Assert(entityType != null, "entityType != null");
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "propertyName must not be null or empty");

            return entityType
                .Properties()
                .SingleOrDefault(p => p.Name.Value == propertyName);
        }

        public static Property GetProperty(this EntityType entityType, string propertyName, string propertyType, bool isKeyProperty)
        {
            Debug.Assert(entityType != null, "entityType != null");
            Debug.Assert(!string.IsNullOrEmpty(propertyName), "propertyName must not be null or empty");
            Debug.Assert(!string.IsNullOrEmpty(propertyType), "propertyType must not be null or empty");

            return entityType
                .Properties()
                .SingleOrDefault(p => p.Name.Value == propertyName && p.TypeName == propertyType && p.IsKeyProperty == isKeyProperty);
        }
    }
}
