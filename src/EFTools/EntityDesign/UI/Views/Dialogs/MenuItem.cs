// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Dialogs
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    ///     WPF Menus are not bound to the visual tree. A way to get around this and still
    ///     use routed commands is to dynamically populate a list of these MenuItems and feed
    ///     it as the DataContext of the ContextMenu.
    /// </summary>
    internal class WpfMenuItem
    {
        public string Text { get; set; }
        public List<WpfMenuItem> Children { get; private set; }
        public ICommand Command { get; set; }
        public Image Icon { get; set; }

        public WpfMenuItem(string item)
        {
            Text = item;
            Children = new List<WpfMenuItem>();
        }
    }
}
