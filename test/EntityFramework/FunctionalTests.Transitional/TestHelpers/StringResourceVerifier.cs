// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Helpers for verifying strings that are pulled from a localized resources
    /// </summary>
    public class StringResourceVerifier
    {
        private readonly AssemblyResourceLookup _lookup;

        /// <summary>
        /// Initializes a new instance of the StringResourceVerifier class.
        /// </summary>
        /// <param name="lookup"> Lookup to be used for locating string resources </param>
        public StringResourceVerifier(AssemblyResourceLookup lookup)
        {
            _lookup = lookup;
        }

        /// <summary>
        /// Determines if the supplied string is an instance of the string defined in localized resources
        /// </summary>
        /// <param name="expectedResourceKey"> The key of the resource string to match against </param>
        /// <param name="actualMessage"> String value to be verified </param>
        /// <param name="stringParameters"> Expected values for string.Format placeholders in the resource string If none are supplied then any values for placeholders in the resource string will count as a match </param>
        /// <returns> True if the string matches, false otherwise </returns>
        public bool IsMatch(string expectedResourceKey, string actualMessage, params object[] stringParameters)
        {
            return IsMatch(expectedResourceKey, actualMessage, true, stringParameters);
        }

        /// <summary>
        /// Determines if the supplied string is an instance of the string defined in localized resources
        /// </summary>
        /// <param name="expectedResourceKey"> The key of the resource string to match against </param>
        /// <param name="actualMessage"> String value to be verified </param>
        /// <param name="isExactMatch"> Determines whether the exception message must be exact match of the message in the resource file, or just contain it. </param>
        /// <param name="stringParameters"> Expected values for string.Format placeholders in the resource string If none are supplied then any values for placeholders in the resource string will count as a match </param>
        /// <returns> True if the string matches, false otherwise </returns>
        public bool IsMatch(string expectedResourceKey, string actualMessage, bool isExactMatch, params object[] stringParameters)
        {
            string messageFromResources;
            return IsMatch(expectedResourceKey, actualMessage, isExactMatch, stringParameters, out messageFromResources);
        }

        /// <summary>
        /// Verified the supplied string is an instance of the string defined in resources
        /// </summary>
        /// <param name="expectedResourceKey"> The key of the resource string to match against </param>
        /// <param name="actualMessage"> String value to be verified </param>
        /// <param name="stringParameters"> Expected values for string.Format placeholders in the resource string If none are supplied then any values for placeholders in the resource string will count as a match </param>
        public void VerifyMatch(string expectedResourceKey, string actualMessage, params object[] stringParameters)
        {
            VerifyMatch(expectedResourceKey, actualMessage, true, stringParameters);
        }

        /// <summary>
        /// Determines if the supplied string is an instance of the string defined in localized resources
        /// If the string in the resource file contains string.Format place holders then the actual message can contain any values for these
        /// </summary>
        /// <param name="expectedResourceKey"> The key of the resource string to match against </param>
        /// <param name="actualMessage"> String value to be verified </param>
        /// <param name="isExactMatch"> Determines whether the exception message must be exact match of the message in the resource file, or just contain it. </param>
        /// <param name="stringParameters"> Expected values for string.Format placeholders in the resource string If none are supplied then any values for placeholders in the resource string will count as a match </param>
        public void VerifyMatch(string expectedResourceKey, string actualMessage, bool isExactMatch, params object[] stringParameters)
        {
            string messageFromResources;
            Assert.True(
                IsMatch(
                    expectedResourceKey, actualMessage, isExactMatch, stringParameters,
                    out messageFromResources),
                string.Format(
                    @"Actual string does not match the message defined in resources.
Expected:'{0}',
Actual:'{1}'",
                    messageFromResources, actualMessage));
        }

        private static bool IsMatchWithAnyPlaceholderValues(string expectedMessage, string actualMessage, bool isExactMatch)
        {
            // Find the sections of the Exception message seperated by {x} tags
            var sections = FindMessageSections(expectedMessage);

            // Check that each section is present in the actual message in correct order
            var indexToCheckFrom = 0;
            foreach (var section in sections)
            {
                // Check if it is present in the actual message
                var index = actualMessage.IndexOf(section, indexToCheckFrom, StringComparison.Ordinal);
                if (index < 0)
                {
                    return false;
                }

                if (section.Length == 0
                    && section == sections.Last())
                {
                    // If the last section is a zero-length string
                    // then this indicates that the placeholder is the
                    // last thing in the resource. Thus every section
                    // matched and the placeholder takes up the rest of string
                    // from the actual message.
                    return true;
                }
                else
                {
                    // continue checking from the end of this section
                    // (Ensures sections are in the correct order)
                    indexToCheckFrom = index + section.Length;
                }
            }

            // If we reach the end then everything is a match
            return isExactMatch ? indexToCheckFrom == actualMessage.Length : indexToCheckFrom <= actualMessage.Length;
        }

        private static IEnumerable<string> FindMessageSections(string messageFromResources)
        {
            // Find the start and end index of each section of the string
            var sections = new List<StringSection>();

            // Start with a section that spans the whole string
            // While there are still place holders shorten the previous section to end at the next { and start a new section from the following }
            sections.Add(
                new StringSection
                    {
                        StartIndex = 0,
                        EndIndex = messageFromResources.Length
                    });
            var indexToCheckFrom = 0;
            while (messageFromResources.IndexOf("{", indexToCheckFrom, StringComparison.Ordinal) >= 0)
            {
                // Find the indexes to split the new section around
                var previous = sections.Last();
                var previousEndIndex = messageFromResources.IndexOf("{", indexToCheckFrom, StringComparison.Ordinal);
                var nextStartIndex = messageFromResources.IndexOf("}", previousEndIndex, StringComparison.Ordinal) + 1;

                // If there are no remaining closing tags then we are done
                if (nextStartIndex == 0)
                {
                    break;
                }
                else
                {
                    // Contents of place holder must be integer
                    int temp;
                    var intContents =
                        int.TryParse(messageFromResources.Substring(previousEndIndex + 1, nextStartIndex - previousEndIndex - 2), out temp);

                    // Place holder must not be escaped (i.e. {{0}})
                    var escaped = messageFromResources[previousEndIndex] == '{'
                                  && nextStartIndex < messageFromResources.Length
                                  && messageFromResources[nextStartIndex] == '}';

                    if (!intContents || escaped)
                    {
                        indexToCheckFrom++;
                        continue;
                    }
                }

                // Shorten the previous section to end at the {
                previous.EndIndex = previousEndIndex;

                // Add the remaining string after the } into a new section,
                // even if the '}' is the last character in the string. This
                // helps verification ensure that there is actually a wildcard
                // at this point in the string instead of the string ending
                // without a wildcard.
                sections.Add(
                    new StringSection
                        {
                            StartIndex = nextStartIndex,
                            EndIndex = messageFromResources.Length
                        });
                indexToCheckFrom = nextStartIndex;
            }

            // Pull out the sections
            return
                sections.Select(
                    s => messageFromResources.Substring(s.StartIndex, s.EndIndex - s.StartIndex).Replace("{{", "{").Replace("}}", "}"));
        }

        private bool IsMatch(
            string expectedResourceKey, string actualMessage, bool isExactMatch, object[] stringParameters, out string messageFromResources)
        {
            ExceptionHelpers.CheckStringArgumentIsNotNullOrEmpty(expectedResourceKey, "expectedResourceKey");
            ExceptionHelpers.CheckArgumentNotNull(actualMessage, "actualMessage");

            messageFromResources = _lookup.LookupString(expectedResourceKey);

            if (stringParameters.Length == 0)
            {
                return IsMatchWithAnyPlaceholderValues(messageFromResources, actualMessage, isExactMatch);
            }
            else
            {
                Assert.True(stringParameters.Count(p => p is AnyValueParameter) <= 1, "Only one 'AnyValueParameter' allowed.");

                messageFromResources = string.Format(CultureInfo.CurrentCulture, messageFromResources, stringParameters);

                var anyValueParameter = stringParameters.OfType<AnyValueParameter>().SingleOrDefault();
                if (anyValueParameter != null)
                {
                    var parts = messageFromResources.Split(new[] { anyValueParameter.ToString() }, StringSplitOptions.None);
                    Assert.Equal(2, parts.Length);

                    return parts.Length == 2 && actualMessage.StartsWith(parts[0]) && actualMessage.EndsWith(parts[1]);
                }

                return isExactMatch ? actualMessage == messageFromResources : actualMessage.Contains(messageFromResources);
            }
        }

        /// <summary>
        /// Represents a section of a string
        /// </summary>
        private class StringSection
        {
            /// <summary>
            /// Gets or sets the index the section starts at
            /// </summary>
            public int StartIndex { get; set; }

            /// <summary>
            /// Gets or sets the index the section ends at
            /// </summary>
            public int EndIndex { get; set; }
        }
    }
}
