// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.Properties;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VersioningFacade.Metadata;

    /// <summary>
    ///     Provides helper methods to classes in the Microsoft.Data.Entity.Design.DatabaseGeneration,
    ///     Microsoft.Data.Entity.Design.DatabaseGeneration.Activities, and Microsoft.Data.Entity.Design.DatabaseGeneration.OutputGenerators
    ///     namespaces for generating and validating ItemCollections.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    public static class EdmExtension
    {
        private const string SsdlErrorExDataKey = "ssdlErrors";

        /// <summary>
        ///     Returns a localized exception from the database generation process if the name supplied in the store schema definition language (SSDL) contains invalid characters for the target database.
        /// </summary>
        /// <param name="userInput">The object name that contains invalid characters.</param>
        /// <returns>A localized exception from the database generation process if the name supplied in the store schema definition language (SSDL) contains invalid characters for the target database</returns>
        public static string GetInvalidCharsException(string userInput)
        {
            return String.Format(CultureInfo.CurrentCulture, Resources.ErrorInvalidCharsException, userInput);
        }

        /// <summary>
        ///     Converts a string representation of conceptual schema definition language (CSDL) to an
        ///     <see
        ///         cref="EdmItemCollection" />
        ///     and validates it.
        /// </summary>
        /// <param name="csdl">Conceptual model metadata as a string.</param>
        /// <param name="targetFrameworkVersion">The targeted version of the Entity Framework.</param>
        /// <returns>
        ///     CSDL as an <see cref="EdmItemCollection" />.
        /// </returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "csdl")]
        public static EdmItemCollection CreateAndValidateEdmItemCollection(string csdl, Version targetFrameworkVersion)
        {
            if (csdl == null)
            {
                throw new ArgumentNullException("csdl");
            }

            if (targetFrameworkVersion == null)
            {
                throw new ArgumentNullException("targetFrameworkVersion");
            }

            if (!EntityFrameworkVersion.IsValidVersion(targetFrameworkVersion))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidTargetVersion, targetFrameworkVersion),
                    "targetFrameworkVersion");
            }

            IList<EdmSchemaError> schemaErrors;
            EdmItemCollection edmItemCollection;
            using (var textReader = new StringReader(csdl))
            {
                using (var xmlReader = XmlReader.Create(textReader))
                {
                    edmItemCollection = EdmItemCollection.Create(new[] { xmlReader }, null, out schemaErrors);
                }
            }

            if (schemaErrors.Count > 0)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.ErrorCsdlNotValid,
                        string.Join(Environment.NewLine, schemaErrors.Select(e => e.Message))));
            }

            if (edmItemCollection.CsdlVersion() > targetFrameworkVersion)
            {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        Resources.TargetVersionSchemaVersionMismatch,
                        edmItemCollection.CsdlVersion(),
                        targetFrameworkVersion));
            }

            return edmItemCollection;
        }

        /// <summary>
        ///     Converts a string representation of store schema definition language (SSDL) to a <see cref="StoreItemCollection" />.
        /// </summary>
        /// <param name="ssdl">SSDL as a string.</param>
        /// <param name="targetFrameworkVersion">The targeted version of the Entity Framework.</param>
        /// <param name="resolver">The dependency resolver to use for loading required dependencies.</param>
        /// <param name="edmErrors">An output parameter that contains a list of errors that occurred during the generation of the StoreItemCollection.</param>
        /// <returns>
        ///     SSDL as a <see cref="StoreItemCollection" />.
        /// </returns>
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "3#")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ssdl")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "edm")]
        public static StoreItemCollection CreateStoreItemCollection(
            string ssdl, Version targetFrameworkVersion, IDbDependencyResolver resolver, out IList<EdmSchemaError> edmErrors)
        {
            if (ssdl == null)
            {
                throw new ArgumentNullException("ssdl");
            }

            if (targetFrameworkVersion == null)
            {
                throw new ArgumentNullException("targetFrameworkVersion");
            }

            if (!EntityFrameworkVersion.IsValidVersion(targetFrameworkVersion))
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, Resources.ErrorNonValidTargetVersion, targetFrameworkVersion),
                    "targetFrameworkVersion");
            }

            using (var textReader = new StringReader(ssdl))
            {
                using (var ssdlReader = XmlReader.Create(textReader))
                {
                    return StoreItemCollection.Create(new[] { ssdlReader }, null, resolver, out edmErrors);
                }
            }
        }

        /// <summary>
        ///     Converts a string representation of store schema definition language (SSDL) to a <see cref="StoreItemCollection" /> and validates it.
        /// </summary>
        /// <param name="ssdl">SSDL as a string.</param>
        /// <param name="targetFrameworkVersion">The targeted version of the Entity Framework.</param>
        /// <param name="resolver">The dependency resolver to use for loading required dependencies.</param>
        /// <param name="catchThrowNamingConflicts">
        ///     Determines if exceptions should be thrown if a naming conflict exists in the generated
        ///     <see
        ///         cref="StoreItemCollection" />
        ///     .
        /// </param>
        /// <returns>
        ///     SSDL as a <see cref="StoreItemCollection" />.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "ssdl")]
        public static StoreItemCollection CreateAndValidateStoreItemCollection(
            string ssdl, Version targetFrameworkVersion, IDbDependencyResolver resolver, bool catchThrowNamingConflicts)
        {
            // Make sure the StoreItemCollection was created (validate it) otherwise the next stage will not proceed
            IList<EdmSchemaError> ssdlErrors;
            var storeItemCollection = CreateStoreItemCollection(ssdl, targetFrameworkVersion, resolver, out ssdlErrors);
            if (ssdlErrors != null
                && ssdlErrors.Count > 0)
            {
                if (catchThrowNamingConflicts)
                {
                    var namingError = ssdlErrors.FirstOrDefault(e => e.ErrorCode == 19);
                    if (namingError != null)
                    {
                        // having the caller catch, parse the SSDL errors, and rethrow an exception is expensive; 
                        // we'll just throw a special one here instead
                        var namingErrorException =
                            new InvalidOperationException(
                                String.Format(CultureInfo.CurrentCulture, Resources.ErrorNameCollision, namingError.Message));
                        namingErrorException.Data.Add(SsdlErrorExDataKey, ssdlErrors);

                        throw namingErrorException;
                    }
                }

                var invalidSsdlException =
                    new InvalidOperationException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            Resources.ErrorNonValidSsdl,
                            string.Join(Environment.NewLine, ssdlErrors.Select(e => e.Message))));

                invalidSsdlException.Data.Add(SsdlErrorExDataKey, ssdlErrors);

                throw invalidSsdlException;
            }

            return storeItemCollection;
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        internal static StorageMappingItemCollection CreateStorageMappingItemCollection(
            EdmItemCollection edm, StoreItemCollection store, string msl, out IList<EdmSchemaError> edmErrors)
        {
            Debug.Assert(edm != null, "edm != null");
            Debug.Assert(store != null, "store != null");
            Debug.Assert(!string.IsNullOrWhiteSpace(msl), "msl cannot be null or whitespace");

            edmErrors = null;
            using (var textReader = new StringReader(msl))
            {
                using (var mslReader = XmlReader.Create(textReader))
                {
                    return StorageMappingItemCollection.Create(edm, store, new[] { mslReader }, null, out edmErrors);
                }
            }
        }

        #region Activity Helpers

        internal static string SerializeXElement(XElement xelement)
        {
            var sb = new StringBuilder();
            using (TextWriter textWriter = new StringWriter(sb, CultureInfo.CurrentCulture))
            {
                xelement.Save(textWriter);
            }

            return sb.ToString();
        }

        #endregion
    }
}
