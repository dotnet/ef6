// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Diagnostics;

    public static class RelationshipMultiplicityConverter
    {
        internal static string MultiplicityToString(RelationshipMultiplicity multiplicity)
        {
            switch (multiplicity)
            {
                case RelationshipMultiplicity.Many:
                    return "*";
                case RelationshipMultiplicity.One:
                    return "1";
                case RelationshipMultiplicity.ZeroOrOne:
                    return "0..1";
                default:
                    Debug.Fail("Did you add a new RelationshipMultiplicity?");
                    return String.Empty;
            }
        }

        /// <summary>
        ///     Gets a <see cref="RelationshipMultiplicity"/> from a string
        /// </summary>
        /// <param name="value"> string containing multiplicity definition </param>
        /// <param name="multiplicity"> multiplicity value (-1 if there were errors) </param>
        /// <returns> <c>true</c> if the string was parsable, <c>false</c> otherwise </returns>
        internal static bool TryParseMultiplicity(string value, out RelationshipMultiplicity multiplicity)
        {
            switch (value)
            {
                case "*":
                    multiplicity = RelationshipMultiplicity.Many;
                    return true;
                case "1":
                    multiplicity = RelationshipMultiplicity.One;
                    return true;
                case "0..1":
                    multiplicity = RelationshipMultiplicity.ZeroOrOne;
                    return true;
                default:
                    multiplicity = (RelationshipMultiplicity)(-1);
                    return false;
            }
        }
    }
}
