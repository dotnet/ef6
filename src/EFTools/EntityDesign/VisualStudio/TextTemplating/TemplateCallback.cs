// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.TextTemplating
{
    using System;
    using System.Globalization;
    using System.Text;
    using Microsoft.VisualStudio.TextTemplating.VSHost;

    // <summary>
    //     TemplateCallback is used by the TextTemplatingService to handle error messages
    //     NOTE that this class should avoid any dependencies on any instance types (especially types instantiated by the
    //     Entity Designer) in the Microsoft.Data.Entity.Design.* namespace except for
    //     Microsoft.Data.Entity.Design.CreateDatabase.
    //     This class exists in this project because of VS dependencies.
    // </summary>
    internal class TemplateCallback : ITextTemplatingCallback
    {
        internal StringBuilder ErrorStringBuilder { get; private set; }

        internal TemplateCallback()
        {
            ErrorStringBuilder = new StringBuilder();
            ErrorStringBuilder.AppendLine(String.Empty);
        }

        void ITextTemplatingCallback.ErrorCallback(bool warning, string message, int line, int column)
        {
            // Throw an exception which will get caught by the pipeline and displayed to the user
            if (warning == false)
            {
                ErrorStringBuilder.AppendLine(String.Format(CultureInfo.CurrentCulture, Resources.TemplateError, line, column, message));
            }
        }

        void ITextTemplatingCallback.SetFileExtension(string extension)
        {
            // We don't need to respond to the type of extension set in the template
        }

        void ITextTemplatingCallback.SetOutputEncoding(Encoding encoding, bool fromOutputDirective)
        {
            // We don't need to respond to the output encoding set by the template
        }
    }
}
