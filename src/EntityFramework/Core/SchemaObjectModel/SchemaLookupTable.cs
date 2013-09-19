// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Reponsible for keep map from alias to namespace for a given schema.
    /// </summary>
    internal sealed class AliasResolver
    {
        #region Fields

        private readonly Dictionary<string, string> _aliasToNamespaceMap = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly List<UsingElement> _usingElementCollection = new List<UsingElement>();
        private readonly Schema _definingSchema;

        #endregion

        #region Public Methods

        /// <summary>
        /// Construct the LookUp table
        /// </summary>
        public AliasResolver(Schema schema)
        {
            _definingSchema = schema;

            // If there is an alias defined for the defining schema,
            // add it to the look up table
            if (!string.IsNullOrEmpty(schema.Alias))
            {
                _aliasToNamespaceMap.Add(schema.Alias, schema.Namespace);
            }
        }

        /// <summary>
        /// Add a UsingElement to the table
        /// </summary>
        /// <param name="usingElement"> the UsingElement to add </param>
        public void Add(UsingElement usingElement)
        {
            DebugCheck.NotNull(usingElement);

            var newNamespace = usingElement.NamespaceName;
            var newAlias = usingElement.Alias;

            // Check whether the alias is a reserved keyword
            if (CheckForSystemNamespace(usingElement, newAlias, NameKind.Alias))
            {
                newAlias = null;
            }

            //Check whether the namespace is a reserved keyword
            if (CheckForSystemNamespace(usingElement, newNamespace, NameKind.Namespace))
            {
                newNamespace = null;
            }

            // see if the alias has already been used
            if (newAlias != null
                && _aliasToNamespaceMap.ContainsKey(newAlias))
            {
                // it has, issue an error and make sure we don't try to add it
                usingElement.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error, Strings.AliasNameIsAlreadyDefined(newAlias));
                newAlias = null;
            }

            // If there's an alias, add it.
            // Its okay if they add the same namespace twice, until they have different alias
            if (newAlias != null)
            {
                _aliasToNamespaceMap.Add(newAlias, newNamespace);
                _usingElementCollection.Add(usingElement);
            }
        }

        /// <summary>
        /// Get the Schema(s) a namespace or alias might refer to
        /// returned schemas may be null is called before or during Schema Resolution
        /// </summary>
        public bool TryResolveAlias(string alias, out string namespaceName)
        {
            DebugCheck.NotEmpty(alias);

            // Check if there is an alias defined with this name
            return _aliasToNamespaceMap.TryGetValue(alias, out namespaceName);
        }

        /// <summary>
        /// Resolves all the namespace specified in the using elements in this schema
        /// </summary>
        public void ResolveNamespaces()
        {
            foreach (var usingElement in _usingElementCollection)
            {
                if (!_definingSchema.SchemaManager.IsValidNamespaceName(usingElement.NamespaceName))
                {
                    usingElement.AddError(
                        ErrorCode.InvalidNamespaceInUsing, EdmSchemaErrorSeverity.Error,
                        Strings.InvalidNamespaceInUsing(usingElement.NamespaceName));
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if the given name is a reserved keyword. if yes, add appropriate error to the refschema
        /// </summary>
        private bool CheckForSystemNamespace(UsingElement refSchema, string name, NameKind nameKind)
        {
            Debug.Assert(
                _definingSchema.ProviderManifest != null,
                "Since we don't allow using elements in provider manifest, provider manifest can never be null");

            // We need to check for system namespace
            if (EdmItemCollection.IsSystemNamespace(_definingSchema.ProviderManifest, name))
            {
                if (nameKind == NameKind.Alias)
                {
                    refSchema.AddError(
                        ErrorCode.CannotUseSystemNamespaceAsAlias, EdmSchemaErrorSeverity.Error,
                        Strings.CannotUseSystemNamespaceAsAlias(name));
                }
                else
                {
                    refSchema.AddError(
                        ErrorCode.NeedNotUseSystemNamespaceInUsing, EdmSchemaErrorSeverity.Error,
                        Strings.NeedNotUseSystemNamespaceInUsing(name));
                }
                return true;
            }
            return false;
        }

        #endregion

        #region Private Types

        /// <summary>
        /// Kind of Name
        /// </summary>
        private enum NameKind
        {
            /// <summary>
            /// It's an Alias
            /// </summary>
            Alias,

            /// <summary>
            /// It's a namespace
            /// </summary>
            Namespace,
        }

        #endregion
    }
}
