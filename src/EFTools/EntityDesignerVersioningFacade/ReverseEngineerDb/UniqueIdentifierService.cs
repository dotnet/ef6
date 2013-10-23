// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    ///     Service making names within a scope unique. Initialize a new instance for every scope.
    /// </summary>
    internal class UniqueIdentifierService
    {
        private static readonly Func<string, string> IdentityTransform = s => s;

        private readonly Dictionary<string, bool> _knownIdentifiers;
        private readonly Dictionary<object, string> _identifierToAdjustedIdentifier;
        private readonly Func<string, string> _transform;

        /// <summary>
        ///     Creates a new instance of this class using the OrdinalIgnoreCase string equality comparer,
        ///     and the identity transform function.
        /// </summary>
        public UniqueIdentifierService()
            : this(StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        ///     Creates a new instance of this class using the specified string comparer and transform function.
        /// </summary>
        /// <param name="comparer">A string equality comparer. null specifies the default comparer.</param>
        /// <param name="transform">A transform function. null specifies the identity transform.</param>
        public UniqueIdentifierService(StringComparer comparer, Func<string, string> transform = null)
        {
            _knownIdentifiers = new Dictionary<string, bool>(comparer);
            _identifierToAdjustedIdentifier = new Dictionary<object, string>();
            _transform = transform ?? IdentityTransform;
        }

        /// <summary>
        ///     This method can be used in when you have an identifier that you know can't be used,
        ///     and you don't want an adjusted version of it.
        /// </summary>
        /// <param name="identifier">
        ///     An identifier. Must not be null or empty, and must not be
        ///     already registered.
        /// </param>
        public void RegisterUsedIdentifier(string identifier)
        {
            _knownIdentifiers.Add(identifier, true);
        }

        /// <summary>
        ///     Removes an identifier from the register of known identifiers.
        /// </summary>
        /// <param name="identifier">The identifier to be unregistered.</param>
        public void UnregisterIdentifier(string identifier)
        {
            _knownIdentifiers.Remove(identifier);
        }

        /// <summary>
        ///     Given an identifier, makes it unique within the scope by adding a suffix (1, 2, 3, ...),
        ///     and returns the adjusted identifier.
        /// </summary>
        /// <param name="identifier">An identifier. Must not be null or empty.</param>
        /// <param name="value">
        ///     An object associated with this identifier in case it is required to
        ///     retrieve the adjusted identifier. If not null, must not exist in the current scope already.
        /// </param>
        /// <returns>Identifier adjusted to be unique within the scope.</returns>
        public string AdjustIdentifier(string identifier, object value = null)
        {
            // find a unique name by adding suffix as necessary
            var numberOfConflicts = 0;
            var adjustedIdentifier = _transform(identifier);
            while (_knownIdentifiers.ContainsKey(adjustedIdentifier))
            {
                ++numberOfConflicts;
                adjustedIdentifier = _transform(identifier) + numberOfConflicts.ToString(CultureInfo.InvariantCulture);
            }

            // remember the identifier in this scope
            _knownIdentifiers.Add(adjustedIdentifier, true);

            if (value != null)
            {
                _identifierToAdjustedIdentifier.Add(value, adjustedIdentifier);
            }

            return adjustedIdentifier;
        }

        /// <summary>
        ///     Determines the adjusted name for an identifier if it has been registered in this scope.
        /// </summary>
        /// <param name="value">An object associated with the identifier to be retrieved.</param>
        /// <param name="adjustedIdentifier">The adjusted identifier.</param>
        /// <returns>true if the retrieval was successful, false otherwise.</returns>
        public bool TryGetAdjustedName(object value, out string adjustedIdentifier)
        {
            return _identifierToAdjustedIdentifier.TryGetValue(value, out adjustedIdentifier);
        }
    }
}
