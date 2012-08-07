// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    ///     Attribute for complex properties
    ///     Implied default AttributeUsage properties Inherited=True, AllowMultiple=False,
    ///     The metadata system expects this and will only look at the first of each of these attributes, even if there are more.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EdmComplexPropertyAttribute : EdmPropertyAttribute
    {
    }
}
