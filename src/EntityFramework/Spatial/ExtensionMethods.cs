// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Spatial
{
    internal static class ExtensionMethods
    {
        internal static void CheckNull<T>(this T value, string argumentName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
}
