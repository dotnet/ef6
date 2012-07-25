// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Design.PluralizationServices
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     This class provide service for both the singularization and pluralization, it takes the word pairs
    ///     in the ctor following the rules that the first one is singular and the second one is plural.
    /// </summary>
    internal class BidirectionalDictionary<TFirst, TSecond>
    {
        internal Dictionary<TFirst, TSecond> FirstToSecondDictionary { get; set; }
        internal Dictionary<TSecond, TFirst> SecondToFirstDictionary { get; set; }

        internal BidirectionalDictionary()
        {
            FirstToSecondDictionary = new Dictionary<TFirst, TSecond>();
            SecondToFirstDictionary = new Dictionary<TSecond, TFirst>();
        }

        internal BidirectionalDictionary(Dictionary<TFirst, TSecond> firstToSecondDictionary)
            : this()
        {
            foreach (var key in firstToSecondDictionary.Keys)
            {
                AddValue(key, firstToSecondDictionary[key]);
            }
        }

        internal virtual bool ExistsInFirst(TFirst value)
        {
            if (FirstToSecondDictionary.ContainsKey(value))
            {
                return true;
            }
            return false;
        }

        internal virtual bool ExistsInSecond(TSecond value)
        {
            if (SecondToFirstDictionary.ContainsKey(value))
            {
                return true;
            }
            return false;
        }

        internal virtual TSecond GetSecondValue(TFirst value)
        {
            if (ExistsInFirst(value))
            {
                return FirstToSecondDictionary[value];
            }
            else
            {
                return default(TSecond);
            }
        }

        internal virtual TFirst GetFirstValue(TSecond value)
        {
            if (ExistsInSecond(value))
            {
                return SecondToFirstDictionary[value];
            }
            else
            {
                return default(TFirst);
            }
        }

        internal void AddValue(TFirst firstValue, TSecond secondValue)
        {
            FirstToSecondDictionary.Add(firstValue, secondValue);

            if (!SecondToFirstDictionary.ContainsKey(secondValue))
            {
                SecondToFirstDictionary.Add(secondValue, firstValue);
            }
        }
    }

    internal class StringBidirectionalDictionary : BidirectionalDictionary<string, string>
    {
        internal StringBidirectionalDictionary()
        {
        }

        internal StringBidirectionalDictionary(Dictionary<string, string> firstToSecondDictionary)
            : base(firstToSecondDictionary)
        {
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        internal override bool ExistsInFirst(string value)
        {
            return base.ExistsInFirst(value.ToLowerInvariant());
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        internal override bool ExistsInSecond(string value)
        {
            return base.ExistsInSecond(value.ToLowerInvariant());
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        internal override string GetFirstValue(string value)
        {
            return base.GetFirstValue(value.ToLowerInvariant());
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        internal override string GetSecondValue(string value)
        {
            return base.GetSecondValue(value.ToLowerInvariant());
        }
    }
}
