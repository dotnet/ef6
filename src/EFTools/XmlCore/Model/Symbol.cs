// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;
    using System.Text;

    /// <summary>
    ///     Represents a name consisting of parts.  A symbol uniquely identifies a normalizeable item in the model.
    /// </summary>
    [DebuggerDisplay("{ToDisplayString()}")]
    [Serializable]
    // Need to be serializable because we are going to pass the instance of this class in the clipboard for drag and drop operation.
    internal class Symbol
    {
        internal static readonly char NORMALIZED_NAME_SEPARATOR_FOR_DISPLAY = '.';
        internal static readonly char VALID_RUNTIME_SEPARATOR = '.';

        internal static Symbol EmptySymbol = new Symbol(string.Empty);

        private readonly string[] _parts;

        /// <summary>
        ///     Create a new Symbol with the passed in Symbol's parts as the prefix parts of the new symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="parts"></param>
        internal Symbol(Symbol symbol, params string[] parts)
        {
            _parts = new String[symbol._parts.Length + parts.Length];

            var i = 0;
            for (; i < symbol._parts.Length; i++)
            {
                _parts[i] = symbol._parts[i];
            }

            for (var j = 0; j < parts.Length; j++)
            {
                _parts[i + j] = parts[j];
            }
        }

        /// <summary>
        ///     Create a new symbol with the specified name parts
        /// </summary>
        /// <param name="parts"></param>
        internal Symbol(params string[] parts)
        {
            _parts = parts;
        }

        // returns the Last part of the symbol
        internal String GetLocalName()
        {
            if (_parts == null
                || _parts.Length == 0)
            {
                return String.Empty;
            }
            else
            {
                return _parts[_parts.Length - 1];
            }
        }

        // returns the First part of the symbol
        internal String GetFirstPart()
        {
            if (_parts == null
                || _parts.Length == 0)
            {
                return null;
            }
            else
            {
                return _parts[0];
            }
        }

        public override bool Equals(object obj)
        {
            var eq = false;
            var other = obj as Symbol;
            if (other != null)
            {
                if (other._parts == null
                    && _parts == null)
                {
                    eq = true;
                }
                else if (other._parts != null
                         && _parts != null)
                {
                    if (other._parts.Length == _parts.Length)
                    {
                        eq = true;
                        for (var i = 0; i < _parts.Length; i++)
                        {
                            if (!other._parts[i].Equals(_parts[i]))
                            {
                                eq = false;
                                break;
                            }
                        }
                    }
                }
            }
            return eq;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            if (_parts != null)
            {
                foreach (var s in _parts)
                {
                    hashCode ^= s.GetHashCode();
                }
            }
            return hashCode;
        }

        internal string ToExternalString()
        {
            return ToString(NORMALIZED_NAME_SEPARATOR_FOR_DISPLAY);
        }

        internal string ToDisplayString()
        {
            return ToString(VALID_RUNTIME_SEPARATOR);
        }

        internal string ToDebugString()
        {
            return ToString(NORMALIZED_NAME_SEPARATOR_FOR_DISPLAY);
        }

        public string ToString(char separatorChar)
        {
            if (_parts == null)
            {
                return "null";
            }
            else
            {
                var sb = new StringBuilder();
                for (var i = 0; i < _parts.Length; i++)
                {
                    sb.Append(_parts[i]);

                    // append "dot" for all but the last part.
                    if (i != _parts.Length - 1)
                    {
                        sb.Append(separatorChar);
                    }
                }
                return sb.ToString();
            }
        }
    }
}
