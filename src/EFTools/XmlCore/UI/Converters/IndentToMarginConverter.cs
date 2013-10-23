// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Converters
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class IndentToMarginConverter : IValueConverter
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="targetType">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="parameter">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="culture">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Thickness((double)value, 0, 0, 0);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="value">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="targetType">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="parameter">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="culture">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(false, "IndentToMarginConverter can only be used for forward conversion.");
            return null;
        }
    }
}
