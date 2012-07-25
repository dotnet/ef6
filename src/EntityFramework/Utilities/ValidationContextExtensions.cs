// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Utilities
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Internal;

    internal static class ValidationContextExtensions
    {
        public static void SetDisplayName(
            this ValidationContext validationContext, InternalMemberEntry property, DisplayAttribute displayAttribute)
        {
            var displayName = displayAttribute == null ? null : displayAttribute.Name;
            if (property == null)
            {
                var objectType = ObjectContextTypeCache.GetObjectType(validationContext.ObjectType);
                validationContext.DisplayName = displayName ?? objectType.Name;
                validationContext.MemberName = null;
            }
            else
            {
                validationContext.DisplayName = displayName ?? DbHelpers.GetPropertyPath(property);
                validationContext.MemberName = DbHelpers.GetPropertyPath(property);
            }
        }
    }
}
