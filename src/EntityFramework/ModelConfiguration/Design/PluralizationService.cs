// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Design.PluralizationServices
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pluralization")]
    internal abstract class PluralizationService
    {
        public CultureInfo Culture { get; protected set; }

        public abstract bool IsPlural(string word);
        public abstract bool IsSingular(string word);
        public abstract string Pluralize(string word);
        public abstract string Singularize(string word);

        /// <summary>
        ///     Factory method for PluralizationService. Only support english pluralization.
        ///     Please set the PluralizationService on the System.Data.Entity.Design.EntityModelSchemaGenerator
        ///     to extend the service to other locales.
        /// </summary>
        /// <param name="culture"> CultureInfo </param>
        /// <returns> PluralizationService </returns>
        public static PluralizationService CreateService(CultureInfo culture)
        {
            Contract.Requires(culture != null);

            if (culture.TwoLetterISOLanguageName == "en")
            {
                return new EnglishPluralizationService();
            }
            else
            {
                throw new NotImplementedException("We don't support locales other than english yet");
            }
        }
    }
}
