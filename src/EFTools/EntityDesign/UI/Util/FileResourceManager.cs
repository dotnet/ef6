// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Util
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;

    internal class FileResourceManager
    {
        private static FileResourceManager _instance;
        private readonly string _componentName;

        private FileResourceManager(Assembly resourceAssembly)
        {
            _componentName = resourceAssembly.ToString();
        }

        public static FileResourceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileResourceManager(typeof(FileResourceManager).Assembly);
                }
                return _instance;
            }
        }

        public static ResourceDictionary GetResourceDictionary(string name)
        {
            return (ResourceDictionary)Instance.LoadObject(name);
        }

        public static FrameworkElement GetElement(string name)
        {
            return (FrameworkElement)Instance.LoadObject(name);
        }

        [SuppressMessage("Microsoft.Globalization",
            "CA1308:NormalizeStringsToUppercase",
            Justification = "The lowercase string is used in a URI")]
        private object LoadObject(string name)
        {
            name = name.ToLower(CultureInfo.InvariantCulture);
            name = name.Replace("\\", "/");

            var uri = new Uri(_componentName + ";component/" + name, UriKind.RelativeOrAbsolute);

            return Application.LoadComponent(uri);
        }
    }
}
