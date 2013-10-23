// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Windows;
using System.Windows.Markup;

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    // All assembly level XAML resources are stored here
    internal partial class AssemblyResources : ResourceDictionary
    {
        private static AssemblyResources _default;

        internal AssemblyResources()
        {
            InitializeComponent();
        }

        // Static accessor to the assembly resource dictionary
        internal static AssemblyResources Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new AssemblyResources();
                }
                return _default;
            }
        }
    }
}
