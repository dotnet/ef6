// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Windows.Controls;
    using System.Windows.Data;

    internal class RowDataInfoValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var group = (BindingGroup)value;

            var errorMessages = new StringBuilder();
            foreach (var item in group.Items)
            {
                // aggregate errors
                var info = item as IDataErrorInfo;
                if (info != null)
                {
                    if (string.IsNullOrWhiteSpace(info.Error) == false)
                    {
                        errorMessages.Append((errorMessages.Length != 0 ? ", " : "") + info.Error);
                    }
                }
            }

            if (errorMessages.Length > 0)
            {
                return new ValidationResult(false, errorMessages.ToString().TrimEnd());
            }
            return ValidationResult.ValidResult;
        }
    }
}
