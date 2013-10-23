// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Data.Entity.Design.UI.ViewModels.Explorer;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public class ExplorerTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="item">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="container">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var explorerElement = item as ExplorerEFElement;
            if (null == explorerElement)
            {
                return base.SelectTemplate(item, container);
            }

            var fwkElem = container as FrameworkElement;
            if (null == fwkElem)
            {
                return base.SelectTemplate(item, container);
            }

            // return the template with same name as class of incoming object
            DataTemplate template = null;
            try
            {
                template = (DataTemplate)fwkElem.FindResource(explorerElement.GetType().Name);
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                // Do not pass on exception - this causes VS crash
                // Instead, allow to fall through with template == null
            }

            if (null == template)
            {
                return base.SelectTemplate(item, container);
            }

            return template;
        }
    }
}
