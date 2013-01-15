// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Pluralization
{
    using System.Data.Entity.Config;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Pluralization services to be used by the EF runtime implement this interface.
    ///     By default the <see cref="EnglishPluralizationService" /> is used, but the pluralization service to use
    ///     can be set in a class derived from <see cref="DbConfiguration" />.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pluralization")]
    public interface IPluralizationService
    {
        /// <summary>
        ///     Check if a word is plural.
        /// </summary>
        /// <param name="word">The word to check.</param>
        /// <returns>True if word is plural; false otherwise..</returns>
        bool IsPlural(string word);

        /// <summary>
        ///     Check if a word is singular.
        /// </summary>
        /// <param name="word">The word to check.</param>
        /// <returns>True if word is singular; false otherwise.</returns>
        bool IsSingular(string word);

        /// <summary>
        ///     Pluralize a word using the service.
        /// </summary>
        /// <param name="word">The word to pluralize.</param>
        /// <returns>The pluralized word </returns>
        string Pluralize(string word);

        /// <summary>
        ///     Singularize a word using the service.
        /// </summary>
        /// <param name="word">The word to singularize.</param>
        /// <returns>The singularized word.</returns>
        string Singularize(string word);
    }
}
