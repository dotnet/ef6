// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;

    // <summary>
    //     A validation rule that makes use of the business objects IDataErrorInfo interface is applied at the property level
    // </summary>
    internal class CellDataInfoValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // obtain the bound business object
            var expression = value as BindingExpression;

            Debug.Assert(expression != null, "passed in value parameter is not a type of: BindingExpression");

            if (expression != null)
            {
                var info = expression.DataItem as IDataErrorInfo;

                Debug.Assert(expression.ParentBinding != null, "Expression's ParentBinding is null.");

                if (expression.ParentBinding != null)
                {
                    // determine the binding path
                    var boundProperty = expression.ParentBinding.Path.Path;

                    Debug.Assert(info != null, "Why DataContext doesn't implement IDataErrorInfo.");
                    Debug.Assert(!String.IsNullOrWhiteSpace(boundProperty), "binding path is null or an empty string.");

                    if (info != null
                        && String.IsNullOrWhiteSpace(boundProperty) == false)
                    {
                        // obtain any errors relating to this bound property
                        var error = info[boundProperty];
                        if (string.IsNullOrWhiteSpace(error) == false)
                        {
                            return new ValidationResult(false, error);
                        }
                        return ValidationResult.ValidResult;
                    }
                }
            }
            return new ValidationResult(false, String.Empty);
        }
    }
}
