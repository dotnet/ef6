// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Pluralization
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal static class PluralizationServiceUtil
    {
        internal static bool DoesWordContainSuffix(string word, IEnumerable<string> suffixes, CultureInfo culture)
        {
            return suffixes.Any(s => word.EndsWith(s, true, culture));
        }

        internal static bool TryGetMatchedSuffixForWord(
            string word, IEnumerable<string> suffixes, CultureInfo culture, out string matchedSuffix)
        {
            matchedSuffix = null;
            if (DoesWordContainSuffix(word, suffixes, culture))
            {
                matchedSuffix = suffixes.First(s => word.EndsWith(s, true, culture));
                return true;
            }
            return false;
        }

        internal static bool TryInflectOnSuffixInWord(
            string word, IEnumerable<string> suffixes, Func<string, string> operationOnWord, CultureInfo culture,
            out string newWord)
        {
            newWord = null;
            string matchedSuffixString;

            if (TryGetMatchedSuffixForWord(
                word,
                suffixes,
                culture,
                out matchedSuffixString))
            {
                newWord = operationOnWord(word);
                return true;
            }
            return false;
        }
    }
}
