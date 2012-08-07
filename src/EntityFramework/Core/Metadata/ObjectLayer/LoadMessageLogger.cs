// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Text;

    internal class LoadMessageLogger
    {
        private readonly Action<String> _logLoadMessage;
        private readonly Dictionary<EdmType, StringBuilder> _messages = new Dictionary<EdmType, StringBuilder>();

        internal LoadMessageLogger(Action<String> logLoadMessage)
        {
            _logLoadMessage = logLoadMessage;
        }

        internal void LogLoadMessage(string message, EdmType relatedType)
        {
            if (_logLoadMessage != null)
            {
                _logLoadMessage(message);
            }

            LogMessagesWithTypeInfo(message, relatedType);
        }

        internal string CreateErrorMessageWithTypeSpecificLoadLogs(string errorMessage, EdmType relatedType)
        {
            return new StringBuilder(errorMessage)
                .AppendLine(GetTypeRelatedLogMessage(relatedType)).ToString();
        }

        private string GetTypeRelatedLogMessage(EdmType relatedType)
        {
            Debug.Assert(relatedType != null, "have to pass in a type to get the message");

            if (_messages.ContainsKey(relatedType))
            {
                return new StringBuilder()
                    .AppendLine()
                    .AppendLine(Strings.ExtraInfo)
                    .AppendLine(_messages[relatedType].ToString()).ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private void LogMessagesWithTypeInfo(string message, EdmType relatedType)
        {
            Debug.Assert(relatedType != null, "have to have a type with this message");

            if (_messages.ContainsKey(relatedType))
            {
                // if this type already contains loading message, append the new message to the end
                _messages[relatedType].AppendLine(message);
            }
            else
            {
                _messages.Add(relatedType, new StringBuilder(message));
            }
        }
    }
}
