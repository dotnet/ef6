// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.EntityClient.Internal
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    internal class DbConnectionOptions
    {
        // instances of this class are intended to be immutable, i.e readonly
        // used by pooling classes so it is much easier to verify correctness
        // when not worried about the class being modified during execution
        internal const string DataDirectory = "|datadirectory|";
        private readonly string _usersConnectionString;
        private readonly Dictionary<string, string> _parsetable = new Dictionary<string, string>();
        internal readonly NameValuePair KeyChain;

        /// <summary>
        /// For testing.
        /// </summary>
        internal DbConnectionOptions()
        {
        }

        internal DbConnectionOptions(string connectionString, IList<string> validKeywords)
        {
            DebugCheck.NotNull(validKeywords);

            _usersConnectionString = connectionString ?? "";

            // first pass on parsing, initial syntax check
            if (0 < _usersConnectionString.Length)
            {
                KeyChain = ParseInternal(_parsetable, _usersConnectionString, validKeywords);
            }
        }

        internal string UsersConnectionString
        {
            get { return _usersConnectionString ?? string.Empty; }
        }

        internal bool IsEmpty
        {
            get { return (null == KeyChain); }
        }

        internal Dictionary<string, string> Parsetable
        {
            get { return _parsetable; }
        }

        internal virtual string this[string keyword]
        {
            get
            {
                string value;
                _parsetable.TryGetValue(keyword, out value);
                return value;
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static string GetKeyName(StringBuilder buffer)
        {
            var count = buffer.Length;
            while ((0 < count)
                   && Char.IsWhiteSpace(buffer[count - 1]))
            {
                count--; // trailing whitespace
            }
            return buffer.ToString(0, count).ToLowerInvariant();
        }

        private static string GetKeyValue(StringBuilder buffer, bool trimWhitespace)
        {
            var count = buffer.Length;
            var index = 0;
            if (trimWhitespace)
            {
                while ((index < count)
                       && Char.IsWhiteSpace(buffer[index]))
                {
                    index++; // leading whitespace
                }
                while ((0 < count)
                       && Char.IsWhiteSpace(buffer[count - 1]))
                {
                    count--; // trailing whitespace
                }
            }
            return buffer.ToString(index, count - index);
        }

        // transistion states used for parsing
        private enum ParserState
        {
            NothingYet = 1, //start point
            Key,
            KeyEqual,
            KeyEnd,
            UnquotedValue,
            DoubleQuoteValue,
            DoubleQuoteValueQuote,
            SingleQuoteValue,
            SingleQuoteValueQuote,
            QuotedValueEnd,
            NullTermination,
        };

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static int GetKeyValuePair(
            string connectionString, int currentPosition, StringBuilder buffer, out string keyname, out string keyvalue)
        {
            var startposition = currentPosition;

            buffer.Length = 0;
            keyname = null;
            keyvalue = null;

            var currentChar = '\0';

            var parserState = ParserState.NothingYet;
            var length = connectionString.Length;
            for (; currentPosition < length; ++currentPosition)
            {
                currentChar = connectionString[currentPosition];

                switch (parserState)
                {
                    case ParserState.NothingYet: // [\\s;]*
                        if ((';' == currentChar)
                            || Char.IsWhiteSpace(currentChar))
                        {
                            continue;
                        }
                        if ('\0' == currentChar)
                        {
                            parserState = ParserState.NullTermination;
                            continue;
                        }
                        if (Char.IsControl(currentChar))
                        {
                            throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(startposition));
                        }
                        startposition = currentPosition;
                        if ('=' != currentChar)
                        {
                            parserState = ParserState.Key;
                            break;
                        }
                        else
                        {
                            parserState = ParserState.KeyEqual;
                            continue;
                        }

                    case ParserState.Key: // (?<key>([^=\\s\\p{Cc}]|\\s+[^=\\s\\p{Cc}]|\\s+==|==)+)
                        if ('=' == currentChar)
                        {
                            parserState = ParserState.KeyEqual;
                            continue;
                        }
                        if (Char.IsWhiteSpace(currentChar))
                        {
                            break;
                        }
                        if (Char.IsControl(currentChar))
                        {
                            throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(startposition));
                        }
                        break;

                    case ParserState.KeyEqual: // \\s*=(?!=)\\s*
                        if ('=' == currentChar)
                        {
                            parserState = ParserState.Key;
                            break;
                        }
                        keyname = GetKeyName(buffer);
                        if (string.IsNullOrEmpty(keyname))
                        {
                            throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(startposition));
                        }
                        buffer.Length = 0;
                        parserState = ParserState.KeyEnd;
                        goto case ParserState.KeyEnd;

                    case ParserState.KeyEnd:
                        if (Char.IsWhiteSpace(currentChar))
                        {
                            continue;
                        }
                        if ('\'' == currentChar)
                        {
                            parserState = ParserState.SingleQuoteValue;
                            continue;
                        }
                        if ('"' == currentChar)
                        {
                            parserState = ParserState.DoubleQuoteValue;
                            continue;
                        }

                        if (';' == currentChar)
                        {
                            goto ParserExit;
                        }
                        if ('\0' == currentChar)
                        {
                            goto ParserExit;
                        }
                        if (Char.IsControl(currentChar))
                        {
                            throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(startposition));
                        }
                        parserState = ParserState.UnquotedValue;
                        break;

                    case ParserState.UnquotedValue: // "((?![\"'\\s])" + "([^;\\s\\p{Cc}]|\\s+[^;\\s\\p{Cc}])*" + "(?<![\"']))"
                        if (Char.IsWhiteSpace(currentChar))
                        {
                            break;
                        }
                        if (Char.IsControl(currentChar)
                            || ';' == currentChar)
                        {
                            goto ParserExit;
                        }
                        break;

                    case ParserState.DoubleQuoteValue: // "(\"([^\"\u0000]|\"\")*\")"
                        if ('"' == currentChar)
                        {
                            parserState = ParserState.DoubleQuoteValueQuote;
                            continue;
                        }
                        if ('\0' == currentChar)
                        {
                            throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(startposition));
                        }
                        break;

                    case ParserState.DoubleQuoteValueQuote:
                        if ('"' == currentChar)
                        {
                            parserState = ParserState.DoubleQuoteValue;
                            break;
                        }
                        keyvalue = GetKeyValue(buffer, false);
                        parserState = ParserState.QuotedValueEnd;
                        goto case ParserState.QuotedValueEnd;

                    case ParserState.SingleQuoteValue: // "('([^'\u0000]|'')*')"
                        if ('\'' == currentChar)
                        {
                            parserState = ParserState.SingleQuoteValueQuote;
                            continue;
                        }
                        if ('\0' == currentChar)
                        {
                            throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(startposition));
                        }
                        break;

                    case ParserState.SingleQuoteValueQuote:
                        if ('\'' == currentChar)
                        {
                            parserState = ParserState.SingleQuoteValue;
                            break;
                        }
                        keyvalue = GetKeyValue(buffer, false);
                        parserState = ParserState.QuotedValueEnd;
                        goto case ParserState.QuotedValueEnd;

                    case ParserState.QuotedValueEnd:
                        if (Char.IsWhiteSpace(currentChar))
                        {
                            continue;
                        }
                        if (';' == currentChar)
                        {
                            goto ParserExit;
                        }
                        if ('\0' == currentChar)
                        {
                            parserState = ParserState.NullTermination;
                            continue;
                        }
                        throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(startposition)); // unbalanced single quote

                    case ParserState.NullTermination: // [\\s;\u0000]*
                        if ('\0' == currentChar)
                        {
                            continue;
                        }
                        if (Char.IsWhiteSpace(currentChar))
                        {
                            continue;
                        }
                        throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(currentPosition));

                    default:
                        throw new InvalidOperationException(
                            Strings.ADP_InternalProviderError((int)EntityUtil.InternalErrorCode.InvalidParserState1));
                }
                buffer.Append(currentChar);
            }
            ParserExit:
            switch (parserState)
            {
                case ParserState.Key:
                case ParserState.DoubleQuoteValue:
                case ParserState.SingleQuoteValue:
                    // keyword not found/unbalanced double/single quote
                    throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(startposition));

                case ParserState.KeyEqual:
                    // equal sign at end of line
                    keyname = GetKeyName(buffer);
                    if (string.IsNullOrEmpty(keyname))
                    {
                        throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(startposition));
                    }
                    break;

                case ParserState.UnquotedValue:
                    // unquoted value at end of line
                    keyvalue = GetKeyValue(buffer, true);

                    var tmpChar = keyvalue[keyvalue.Length - 1];
                    if (('\'' == tmpChar)
                        || ('"' == tmpChar))
                    {
                        throw new ArgumentException(Strings.ADP_ConnectionStringSyntax(startposition));
                        // unquoted value must not end in quote
                    }
                    break;

                case ParserState.DoubleQuoteValueQuote:
                case ParserState.SingleQuoteValueQuote:
                case ParserState.QuotedValueEnd:
                    // quoted value at end of line
                    keyvalue = GetKeyValue(buffer, false);
                    break;

                case ParserState.NothingYet:
                case ParserState.KeyEnd:
                case ParserState.NullTermination:
                    // do nothing
                    break;

                default:
                    throw new InvalidOperationException(
                        Strings.ADP_InternalProviderError((int)EntityUtil.InternalErrorCode.InvalidParserState2));
            }
            if ((';' == currentChar)
                && (currentPosition < connectionString.Length))
            {
                currentPosition++;
            }
            return currentPosition;
        }

        private static NameValuePair ParseInternal(IDictionary<string, string> parsetable, string connectionString, IList<string> validKeywords)
        {
            DebugCheck.NotNull(connectionString);
            DebugCheck.NotNull(validKeywords);

            var buffer = new StringBuilder();
            NameValuePair localKeychain = null, keychain = null;
            var nextStartPosition = 0;
            var endPosition = connectionString.Length;
            while (nextStartPosition < endPosition)
            {
                var startPosition = nextStartPosition;

                string keyname, keyvalue;
                nextStartPosition = GetKeyValuePair(connectionString, startPosition, buffer, out keyname, out keyvalue);
                if (string.IsNullOrEmpty(keyname))
                {
                    break;
                }

                if (!validKeywords.Contains(keyname))
                {
                    throw new ArgumentException(Strings.ADP_KeywordNotSupported(keyname));
                }
                parsetable[keyname] = keyvalue; // last key-value pair wins (or first)

                if (null != localKeychain)
                {
                    localKeychain = localKeychain.Next = new NameValuePair();
                }
                else
                {
                    // first time only - don't contain modified chain from UDL file
                    keychain = localKeychain = new NameValuePair();
                }
            }

            return keychain;
        }
    }
}
