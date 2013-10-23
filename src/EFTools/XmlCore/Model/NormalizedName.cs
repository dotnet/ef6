// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;

    /// <summary>
    ///     Represents a reference from one part of the model to a named object
    ///     elsewhere. The reference can be of the form:
    ///     (1) 'name_of_object'
    ///     (2) 'alias_name.name_of_object'
    ///     (3) 'namespace_name.name_of_object'
    ///     This class splits out the alias or namespace so that it can be changed
    ///     if necessary.
    /// </summary>
    internal class NormalizedName
    {
        // the symbol for the target of this reference in the symbol table
        private readonly Symbol _referenceAsSymbol;

        // the part of the original reference representing an alias name, if any
        private readonly string _aliasPart;

        // the part of the original reference representing a namespace name, if any
        private readonly string _namespacePart;

        // the remaining part of the original reference after any alias or namespace has been removed
        private readonly string _remainingPart;

        /// <summary>
        ///     Create a new NormalizedName with the passed in Symbol's parts as the prefix parts of the new symbol
        /// </summary>
        internal NormalizedName(
            Symbol originalReferenceAsSymbol,
            string aliasPart, string namespacePart, string remainingPart)
        {
            Debug.Assert(
                (null == aliasPart || null == namespacePart),
                "One or both of aliasPart or namespacePart should be null. Received aliasPart " + aliasPart + ", namespacePart "
                + namespacePart);
            _referenceAsSymbol = originalReferenceAsSymbol;
            _aliasPart = aliasPart;
            _namespacePart = namespacePart;
            _remainingPart = remainingPart;
        }

        // the Symbol representing the target of the original reference in the symbol table
        internal Symbol Symbol
        {
            get { return _referenceAsSymbol; }
        }

        // the alias part of the original reference if it had one, otherwise null
        internal string AliasPart
        {
            get { return _aliasPart; }
        }

        // the namespace part of the original reference if it had one, otherwise null
        internal string NamespacePart
        {
            get { return _namespacePart; }
        }

        // the remaining part of the original reference after removal of alias or namespace if it had one
        internal string RemainingPart
        {
            get { return _remainingPart; }
        }

        internal string ToBindingString()
        {
            if (null != _aliasPart)
            {
                return _aliasPart + Symbol.VALID_RUNTIME_SEPARATOR + _remainingPart;
            }
            else if (null != _namespacePart)
            {
                return _namespacePart + Symbol.VALID_RUNTIME_SEPARATOR + _remainingPart;
            }
            else
            {
                return _remainingPart;
            }
        }

        /// <summary>
        ///     Looks in this NameReference for a namespace part matching
        ///     oldNamespace. If it finds it replaces oldNamespace with newNamespace
        ///     in the newBindingString out parameter and returns true, otherwise
        ///     newBindingString contains the reference as it was originally
        ///     and returns false.
        /// </summary>
        /// <returns>true if a replacement is made, otherwise false</returns>
        internal bool ConstructBindingStringWithReplacedNamespace(
            string oldNamespace, string newNamespace, out string newBindingString)
        {
            Debug.Assert(!string.IsNullOrEmpty(oldNamespace), "ConstructBindingStringWithReplacedNamespace(): null or empty oldNamespace");
            Debug.Assert(!string.IsNullOrEmpty(newNamespace), "ConstructBindingStringWithReplacedNamespace(): null or empty newNamespace");
            if (string.IsNullOrEmpty(oldNamespace)
                ||
                string.IsNullOrEmpty(newNamespace))
            {
                newBindingString = null;
                return false;
            }

            // now compare the namespace part of oldBindingStringNameRef to oldNamespace 
            if (null == _namespacePart)
            {
                // no namespace part to replace - just return reference as it was originally
                newBindingString = ToBindingString();
                return false;
            }

            if (!_namespacePart.Equals(oldNamespace, StringComparison.CurrentCulture))
            {
                // the reference has a namespace but it doesn't match oldNamespace
                // so just return the reference as it was originally
                newBindingString = ToBindingString();
                return false;
            }

            // the reference has a namespace and it matches oldNamespace
            // so return set newBindingString to 'newNamespace.remaining_part_of_old_reference'
            if (null == _remainingPart)
            {
                newBindingString = newNamespace;
            }
            else
            {
                newBindingString = newNamespace + Symbol.VALID_RUNTIME_SEPARATOR + _remainingPart;
            }

            return true;
        }

        /// <summary>
        ///     Looks in this NameReference for an alias part matching
        ///     oldAlias. If it finds it replaces oldAlias with newAlias
        ///     in the newBindingString out parameter and returns true, otherwise
        ///     newBindingString contains the reference as it was originally
        ///     and returns false.
        /// </summary>
        /// <returns>true if a replacement is made, otherwise false</returns>
        internal bool ConstructBindingStringWithReplacedAlias(
            string oldAlias, string newAlias, out string newBindingString)
        {
            Debug.Assert(!string.IsNullOrEmpty(oldAlias), "ConstructBindingStringWithReplacedAlias(): null or empty oldAlias");
            Debug.Assert(!string.IsNullOrEmpty(newAlias), "ConstructBindingStringWithReplacedAlias(): null or empty newAlias");
            if (string.IsNullOrEmpty(oldAlias)
                ||
                string.IsNullOrEmpty(newAlias))
            {
                newBindingString = null;
                return false;
            }

            // now compare the alias part of oldBindingStringNameRef to oldNamespace 
            if (null == _aliasPart)
            {
                // no alias part to replace - just return reference as it was originally
                newBindingString = ToBindingString();
                return false;
            }

            if (!_aliasPart.Equals(oldAlias, StringComparison.CurrentCulture))
            {
                // the reference has an alias but it doesn't match oldAlias
                // so just return the reference as it was originally
                newBindingString = ToBindingString();
                return false;
            }

            // the reference has an alias and it matches oldAlias
            // so return set newBindingString to 'newAlias.remaining_part_of_old_reference'
            if (null == _remainingPart)
            {
                newBindingString = newAlias;
            }
            else
            {
                newBindingString = newAlias + Symbol.VALID_RUNTIME_SEPARATOR + _remainingPart;
            }

            return true;
        }

        /// <summary>
        ///     Looks in this NameReference for a name part matching
        ///     oldName. If it finds it replaces oldName with newName
        ///     in the newBindingString out parameter and returns true, otherwise
        ///     newBindingString contains the reference as it was originally
        ///     and returns false.
        /// </summary>
        /// <returns>true if a replacement is made, otherwise false</returns>
        internal bool ConstructBindingStringWithReplacedNamePart(
            string oldName, string newName, out string newBindingString)
        {
            Debug.Assert(!string.IsNullOrEmpty(oldName), "ConstructBindingStringWithReplacedNamePart(): null or empty oldName");
            Debug.Assert(!string.IsNullOrEmpty(newName), "ConstructBindingStringWithReplacedNamePart(): null or empty newName");
            if (string.IsNullOrEmpty(oldName)
                ||
                string.IsNullOrEmpty(newName))
            {
                newBindingString = null;
                return false;
            }

            if (string.IsNullOrEmpty(_remainingPart)
                ||
                !_remainingPart.Equals(oldName, StringComparison.CurrentCulture))
            {
                // the reference has no name part or it does not match oldName
                // so no replacement
                newBindingString = ToBindingString();
                return false;
            }
            else
            {
                // the reference has a name part which matches oldName
                // so replace with newName
                if (null != _aliasPart)
                {
                    newBindingString = _aliasPart + Symbol.VALID_RUNTIME_SEPARATOR + newName;
                }
                else if (null != _namespacePart)
                {
                    newBindingString = _namespacePart + Symbol.VALID_RUNTIME_SEPARATOR + newName;
                }
                else
                {
                    newBindingString = newName;
                }

                return true;
            }
        }
    }
}
