// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Core
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Resources;

    internal static class ResourceUtils
    {
        internal static string LookupResource(Type resourceManagerProvider, string resourceKey)
        {
            foreach (
                var staticProperty in
                    resourceManagerProvider.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            {
                if (staticProperty.PropertyType == typeof(ResourceManager))
                {
                    var resourceManager = staticProperty.GetValue(null, null) as ResourceManager;
                    Debug.Assert(
                        null != resourceManager, "Unable to find ResourceManager property in class:" + resourceManagerProvider.Name);

                    if (null != resourceManager)
                    {
                        try
                        {
                            return resourceManager.GetString(resourceKey);
                        }
                        catch (MissingManifestResourceException)
                        {
                            Debug.Fail("Could not find resource string with key: " + resourceKey);
                        }
                    }
                }
            }
            return resourceKey; // Fallback with the key name 
        }
    }
}
