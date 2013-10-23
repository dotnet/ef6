// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Tools.XmlDesignerBase;

    [DebuggerDisplay("{_errorClass.ToString()} | {_message}")]
    internal class ErrorInfo
    {
        private readonly Severity _severity;
        private readonly string _message;
        private readonly EFObject _item;
        private readonly int _errorCode;
        private readonly ErrorClass _errorClass;

        internal ErrorInfo(Severity severity, string message, EFObject item, int errorCode, ErrorClass errorClass)
        {
            Debug.Assert(item != null, "item != null");
            _severity = severity;

            // prefix the error code in front of the error message.  This is here to help identify runtime errors that cause safe-mode
            _message = String.Format(CultureInfo.CurrentCulture, Resources.Error_Message_With_Error_Code_Prefix, errorCode, message);
            _item = item;
            _errorCode = errorCode;
            _errorClass = errorClass;
        }

        internal enum Severity
        {
            NONE = 0,
            INFO = 1,
            WARNING = 2,
            ERROR = 4
        };

        internal int GetLineNumber()
        {
            return _item.GetLineNumber();
        }

        internal int GetColumnNumber()
        {
            return _item.GetColumnNumber();
        }

        internal bool IsError()
        {
            return _severity == Severity.ERROR;
        }

        internal bool IsWarning()
        {
            return _severity == Severity.WARNING;
        }

        internal bool IsInfo()
        {
            return _severity == Severity.INFO;
        }

        internal Severity Level
        {
            get { return _severity; }
        }

        internal string Message
        {
            get { return _message; }
        }

        internal EFObject Item
        {
            get { return _item; }
        }

        internal int ErrorCode
        {
            get { return _errorCode; }
        }

        internal ErrorClass ErrorClass
        {
            get { return _errorClass; }
        }
    }
}
