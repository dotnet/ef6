// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    ///     This class represent a single filter entry
    /// </summary>
    internal class EntityStoreSchemaFilterEntry
    {
        private readonly string _catalog;
        private readonly string _schema;
        private readonly string _name;
        private readonly EntityStoreSchemaFilterObjectTypes _types;
        private readonly EntityStoreSchemaFilterEffect _effect;

        /// <summary>
        ///     Creates a EntityStoreSchemaFilterEntry
        /// </summary>
        /// <param name="catalog">The pattern to use to select the appropriate catalog or null to not limit by catalog.</param>
        /// <param name="schema">The pattern to use to select the appropriate schema or null to not limit by schema.</param>
        /// <param name="name">The pattern to use to select the appropriate name or null to not limit by name.</param>
        /// <param name="types">The type of objects to apply this filter to.</param>
        /// <param name="effect">The effect that this filter should have on the results.</param>
        public EntityStoreSchemaFilterEntry(
            string catalog, string schema, string name, EntityStoreSchemaFilterObjectTypes types, EntityStoreSchemaFilterEffect effect)
        {
            if (types == EntityStoreSchemaFilterObjectTypes.None)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Resources_VersioningFacade.InvalidStringArgument,
                        "types"));
            }
            _catalog = catalog;
            _schema = schema;
            _name = name;
            _types = types;
            _effect = effect;
        }

        /// <summary>
        ///     Creates a EntityStoreSchemaFilterEntry
        /// </summary>
        /// <param name="catalog">The pattern to use to select the appropriate catalog or null to not limit by catalog.</param>
        /// <param name="schema">The pattern to use to select the appropriate schema or null to not limit by schema.</param>
        /// <param name="name">The pattern to use to select the appropriate name or null to not limit by name.</param>
        public EntityStoreSchemaFilterEntry(string catalog, string schema, string name)
            : this(catalog, schema, name, EntityStoreSchemaFilterObjectTypes.All, EntityStoreSchemaFilterEffect.Allow)
        {
        }

        /// <summary>
        ///     Gets the pattern that will be used to select the appropriate catalog.
        /// </summary>
        public string Catalog
        {
            [DebuggerStepThrough] get { return _catalog; }
        }

        /// <summary>
        ///     Gets the pattern that will be used to select the appropriate schema.
        /// </summary>
        public string Schema
        {
            [DebuggerStepThrough] get { return _schema; }
        }

        /// <summary>
        ///     Gets the pattern that will be used to select the appropriate name.
        /// </summary>
        public string Name
        {
            [DebuggerStepThrough] get { return _name; }
        }

        /// <summary>
        ///     Gets the types of objects that this filter applies to.
        /// </summary>
        public EntityStoreSchemaFilterObjectTypes Types
        {
            [DebuggerStepThrough] get { return _types; }
        }

        /// <summary>
        ///     Gets the effect that this filter has on results.
        /// </summary>
        public EntityStoreSchemaFilterEffect Effect
        {
            [DebuggerStepThrough] get { return _effect; }
        }
    }
}
