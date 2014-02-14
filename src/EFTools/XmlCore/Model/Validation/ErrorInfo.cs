// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Validation
{
    using Microsoft.Data.Tools.XmlDesignerBase;
    using System;
    using System.Diagnostics;
    using System.Globalization;

    [DebuggerDisplay("{_errorClass.ToString()} | {_message}")]
    internal class ErrorInfo
    {
        private readonly Severity _severity;
        private readonly string _message;
        private readonly EFObject _item;
        private readonly string _itemPath;
        private readonly int _errorCode;
        private readonly ErrorClass _errorClass;


        // should be used for edmx errors 
        public ErrorInfo(Severity severity, string message, EFObject item, int errorCode, ErrorClass errorClass)
            : this(severity, message, item, null, errorCode, errorClass)
        {
            Debug.Assert(item != null, "item is null");
        }

        // should be used for code first errors
        public ErrorInfo(Severity severity, string message, string itemPath, int errorCode, ErrorClass errorClass)
            : this(severity, message, null, itemPath, errorCode, errorClass)
        {
            Debug.Assert(!string.IsNullOrEmpty(itemPath), "invalid item path");
        }

        private ErrorInfo(Severity severity, string message, EFObject item, string itemPath, int errorCode, ErrorClass errorClass)
        {
            Debug.Assert(item == null ^ itemPath == null, "item and itemPath are mutually exclusive");

            _severity = severity;
            _item = item;
            _itemPath = itemPath;
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
            return _item == null ? 0 : _item.GetLineNumber();
        }

        internal int GetColumnNumber()
        {
            return _item == null ? 0 : _item.GetColumnNumber();
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

        public string ItemPath
        {
            get { return _item != null ? _item.Uri.LocalPath : _itemPath; }
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
