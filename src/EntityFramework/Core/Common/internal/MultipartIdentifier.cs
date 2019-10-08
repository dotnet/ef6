// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    // <summary>
    // Copied from System.Data.dll
    // </summary>
    internal static class MultipartIdentifier
    {
        private const int MaxParts = 4;
        internal const int ServerIndex = 0;
        internal const int CatalogIndex = 1;
        internal const int SchemaIndex = 2;
        internal const int TableIndex = 3;

        private enum MPIState
        {
            MPI_Value,
            MPI_ParseNonQuote,
            MPI_LookForSeparator,
            MPI_LookForNextCharOrSeparator,
            MPI_ParseQuote,
            MPI_RightQuote,
        }

        private static void IncrementStringCount(List<string> ary, ref int position)
        {
            ++position;
            ary.Add(string.Empty);
        }

        private static bool IsWhitespace(char ch)
        {
            return Char.IsWhiteSpace(ch);
        }

        // <summary>
        // Core function  for parsing the multipart identifer string.
        // Note:  Left quote strings need to correspond 1 to 1 with the right quote strings
        // example: "ab" "cd",  passed in for the left and the right quote
        // would set a or b as a starting quote character.
        // If a is the starting quote char then c would be the ending quote char
        // otherwise if b is the starting quote char then d would be the ending quote character.
        // </summary>
        // <param name="name"> string to parse </param>
        // <param name="leftQuote"> set of characters which are valid quoteing characters to initiate a quote </param>
        // <param name="rightQuote"> set of characters which are valid to stop a quote, array index's correspond to the leftquote array. </param>
        // <param name="separator"> separator to use </param>
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal static List<string> ParseMultipartIdentifier(string name, string leftQuote, string rightQuote, char separator)
        {
            Debug.Assert(
                -1 == leftQuote.IndexOf(separator) && -1 == rightQuote.IndexOf(separator) && leftQuote.Length == rightQuote.Length,
                "Incorrect usage of quotes");

            var parsedNames = new List<string>();
            parsedNames.Add(null);
            var stringCount = 0; // index of current string in the list
            var state = MPIState.MPI_Value; // Initialize the starting state

            var sb = new StringBuilder(name.Length);
            // String buffer to hold the string being currently built, init the string builder so it will never be resized
            StringBuilder whitespaceSB = null;
            // String buffer to hold white space used when parsing nonquoted strings  'a b .  c d' = 'a b' and 'c d'
            var rightQuoteChar = ' '; // Right quote character to use given the left quote character found.
            for (var index = 0; index < name.Length; ++index)
            {
                var testchar = name[index];
                switch (state)
                {
                    case MPIState.MPI_Value:
                        {
                            int quoteIndex;
                            if (IsWhitespace(testchar))
                            {
                                // Is White Space then skip the whitespace
                                continue;
                            }
                            else if (testchar == separator)
                            {
                                // If we found a separator, no string was found, initialize the string we are parsing to Empty and the next one to Empty.
                                // This is NOT a redundant setting of string.Empty it solves the case where we are parsing ".xyz" and we should be returning null, null, empty, xyz
                                parsedNames[stringCount] = string.Empty;
                                IncrementStringCount(parsedNames, ref stringCount);
                            }
                            else if (-1 != (quoteIndex = leftQuote.IndexOf(testchar)))
                            {
                                // If we are a left quote                                                                                                                          
                                rightQuoteChar = rightQuote[quoteIndex]; // record the corresponding right quote for the left quote
                                sb.Length = 0;
                                state = MPIState.MPI_ParseQuote;
                            }
                            else if (-1 != rightQuote.IndexOf(testchar))
                            {
                                // If we shouldn't see a right quote
                                throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
                            }
                            else
                            {
                                sb.Length = 0;
                                sb.Append(testchar);
                                state = MPIState.MPI_ParseNonQuote;
                            }
                            break;
                        }

                    case MPIState.MPI_ParseNonQuote:
                        {
                            if (testchar == separator)
                            {
                                parsedNames[stringCount] = sb.ToString(); // set the currently parsed string
                                IncrementStringCount(parsedNames, ref stringCount);
                                state = MPIState.MPI_Value;
                            }
                            else // Quotes are not valid inside a non-quoted name
                                if (-1 != rightQuote.IndexOf(testchar))
                                {
                                    throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
                                }
                                else if (-1 != leftQuote.IndexOf(testchar))
                                {
                                    throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
                                }
                                else if (IsWhitespace(testchar))
                                {
                                    // If it is Whitespace 
                                    parsedNames[stringCount] = sb.ToString(); // Set the currently parsed string
                                    if (null == whitespaceSB)
                                    {
                                        whitespaceSB = new StringBuilder();
                                    }
                                    whitespaceSB.Length = 0;
                                    whitespaceSB.Append(testchar);
                                    // start to record the white space, if we are parsing a name like "name with space" we should return "name with space"
                                    state = MPIState.MPI_LookForNextCharOrSeparator;
                                }
                                else
                                {
                                    sb.Append(testchar);
                                }
                            break;
                        }

                    case MPIState.MPI_LookForNextCharOrSeparator:
                        {
                            if (!IsWhitespace(testchar))
                            {
                                // If it is not whitespace
                                if (testchar == separator)
                                {
                                    IncrementStringCount(parsedNames, ref stringCount);
                                    state = MPIState.MPI_Value;
                                }
                                else
                                {
                                    // If its not a separator and not whitespace
                                    sb.Append(whitespaceSB);
                                    sb.Append(testchar);
                                    parsedNames[stringCount] = sb.ToString(); // Need to set the name here in case the string ends here.
                                    state = MPIState.MPI_ParseNonQuote;
                                }
                            }
                            else
                            {
                                whitespaceSB.Append(testchar);
                            }
                            break;
                        }

                    case MPIState.MPI_ParseQuote:
                        {
                            if (testchar == rightQuoteChar)
                            {
                                // if se are on a right quote see if we are escapeing the right quote or ending the quoted string                            
                                state = MPIState.MPI_RightQuote;
                            }
                            else
                            {
                                sb.Append(testchar); // Append what we are currently parsing
                            }
                            break;
                        }

                    case MPIState.MPI_RightQuote:
                        {
                            if (testchar == rightQuoteChar)
                            {
                                // If the next char is a another right quote then we were escapeing the right quote
                                sb.Append(testchar);
                                state = MPIState.MPI_ParseQuote;
                            }
                            else if (testchar == separator)
                            {
                                // If its a separator then record what we've parsed
                                parsedNames[stringCount] = sb.ToString();
                                IncrementStringCount(parsedNames, ref stringCount);
                                state = MPIState.MPI_Value;
                            }
                            else if (!IsWhitespace(testchar))
                            {
                                // If it is not white space we got problems
                                throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
                            }
                            else
                            {
                                // It is a whitespace character so the following char should be whitespace, separator, or end of string anything else is bad
                                parsedNames[stringCount] = sb.ToString();
                                state = MPIState.MPI_LookForSeparator;
                            }
                            break;
                        }

                    case MPIState.MPI_LookForSeparator:
                        {
                            if (!IsWhitespace(testchar))
                            {
                                // If it is not whitespace
                                if (testchar == separator)
                                {
                                    // If it is a separator 
                                    IncrementStringCount(parsedNames, ref stringCount);
                                    state = MPIState.MPI_Value;
                                }
                                else
                                {
                                    // Otherwise not a separator
                                    throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
                                }
                            }
                            break;
                        }
                }
            }

            // Resolve final states after parsing the string            
            switch (state)
            {
                case MPIState.MPI_Value: // These states require no extra action
                case MPIState.MPI_LookForSeparator:
                case MPIState.MPI_LookForNextCharOrSeparator:
                    break;

                case MPIState.MPI_ParseNonQuote: // Dump what ever was parsed
                case MPIState.MPI_RightQuote:
                    parsedNames[stringCount] = sb.ToString();
                    break;

                case MPIState.MPI_ParseQuote: // Invalid Ending States
                default:
                    throw new ArgumentException(Strings.ADP_InvalidMultipartNameDelimiterUsage, "path");
            }
            return parsedNames;
        }
    }
}
