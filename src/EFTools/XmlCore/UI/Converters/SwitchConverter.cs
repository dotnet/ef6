// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    /// <remarks>
    ///     Transformer which maps from input values to output values, based on a list of SwitchCase children.
    ///     This isn't strictly a C-style 'switch' statement, since cases aren't guaranteed to be unique.
    /// </remarks>
    [ContentProperty("Cases")]
    public sealed class SwitchConverter : DependencyObject, IValueConverter
    {
        private readonly Collection<SwitchCase> _cases;
        private object _defaultValue;

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public SwitchConverter()
        {
            _cases = new Collection<SwitchCase>();
        }

        #region Properties

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public Collection<SwitchCase> Cases
        {
            get { return _cases; }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public object DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        #endregion Properties

        #region IValueConverter implementation

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
            foreach (var switchCase in Cases)
            {
                if (Equals(switchCase.In, value))
                {
                    return switchCase.Out;
                }
            }

            return _defaultValue;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="o">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="targetType">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="parameter">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <param name="culture">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        /// <returns>This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException(
                string.Format(CultureInfo.CurrentCulture, Resources.SwitchConverterErrorMessage, typeof(SwitchConverter).Name));
        }

        #endregion IValueConverter implementation
    }

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public sealed class SwitchCase : DependencyObject
    {
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static readonly DependencyProperty InProperty = DependencyProperty.Register("In", typeof(object), typeof(SwitchCase));
        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public static readonly DependencyProperty OutProperty = DependencyProperty.Register("Out", typeof(object), typeof(SwitchCase));

        #region Properties

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public object In
        {
            get { return GetValue(InProperty); }
            set { SetValue(InProperty, value); }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public object Out
        {
            get { return GetValue(OutProperty); }
            set { SetValue(OutProperty, value); }
        }

        #endregion Properties
    }
}
