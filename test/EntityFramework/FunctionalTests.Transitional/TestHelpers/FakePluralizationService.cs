// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.TestHelpers
{
    using System;
    using System.Data.Entity.Infrastructure.Pluralization;

    public class FakePluralizationService : IPluralizationService
    {
        public string Pluralize(string word)
        {
            if (!word.EndsWith("z"))
            {
                return string.Format("{0}z", word);
            }
            return word;
        }

        public string Singularize(string word)
        {
            return word;
        }
    }
}
