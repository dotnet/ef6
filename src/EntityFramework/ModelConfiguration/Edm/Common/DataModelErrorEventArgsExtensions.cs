// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Common
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.Resources;
    using System.Text;

    internal static class DataModelErrorEventArgsExtensions
    {
        public static string ToErrorMessage(this IEnumerable<DataModelErrorEventArgs> validationErrors)
        {
            var errorMessage = new StringBuilder();

            errorMessage.AppendLine(Strings.ValidationHeader);
            errorMessage.AppendLine();

            foreach (var error in validationErrors)
            {
                errorMessage.AppendLine(
                    Strings.ValidationItemFormat(error.Item, error.PropertyName, error.ErrorMessage));
            }

            return errorMessage.ToString();
        }
    }
}
