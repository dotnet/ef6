// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     This class represents a set of artifacts that are validated and resolved with respect to one another.
    ///     This class contains resolve information (symbols & bindings) and dep & anti-dep info
    /// </summary>
    internal abstract class EFArtifactSet
    {
        /// <summary>
        ///     a Map of ( EFArtifact -> Map of( ErrorClass -> ICollection of ErrorInfo).
        ///     The ErrorClasses should be either resolve or runtime validation errors.
        ///     These are stored this way so that we can quickly "drop" the appropriate errors when re-validating, and not lose
        ///     errors that are still present.  For example, we can drop all of the runtime validation errors, and still keep
        ///     the resolve errors.
        /// </summary>
        private readonly Dictionary<EFArtifact, Dictionary<ErrorClass, ICollection<ErrorInfo>>> _artifacts2Errors =
            new Dictionary<EFArtifact, Dictionary<ErrorClass, ICollection<ErrorInfo>>>();

        private readonly Dictionary<Symbol, List<EFElement>> _symbolTable = new Dictionary<Symbol, List<EFElement>>();
        private readonly EFDependencyGraph _dependencyGraph = new EFDependencyGraph();

        internal EFArtifactSet(EFArtifact artifact)
        {
            Add(artifact);
        }

        internal void Add(EFArtifact artifact)
        {
            _artifacts2Errors.Add(artifact, new Dictionary<ErrorClass, ICollection<ErrorInfo>>());
        }

        internal ICollection<EFArtifact> Artifacts
        {
            get { return new ReadOnlyCollection<EFArtifact>(_artifacts2Errors.Keys); }
        }

        /// <summary>
        ///     Add a symbol to the global symbol table that maps to the passed in item.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="item"></param>
        internal void AddSymbol(Symbol symbol, EFElement item)
        {
            if (!_symbolTable.ContainsKey(symbol))
            {
                var list = new List<EFElement>(1);
                list.Add(item);
                _symbolTable.Add(symbol, list);
            }
            else
            {
                var current = LookupSymbol(symbol);
                if (current.Identity != item.Identity)
                {
                    // duplicate symbol.  Add the new item to the list 
                    _symbolTable[symbol].Add(item);

                    // make it ready to display to the user
                    var displayableSymbol = EFNormalizableItem.ConvertSymbolToExternal(symbol);

                    // add an duplicate symbol error
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.NORMALIZE_DUPLICATE_SYMBOL_DEFINED, displayableSymbol);
                    var errorInfo = new ErrorInfo(
                        ErrorInfo.Severity.ERROR, msg, item, ErrorCodes.NORMALIZE_DUPLICATE_SYMBOL_DEFINED, ErrorClass.ResolveError);
                    AddError(errorInfo);
                }
            }
        }

        /// <summary>
        ///     Removes the passed in symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="item"></param>
        internal void RemoveSymbol(Symbol symbol, EFElement item)
        {
            if (_symbolTable.ContainsKey(symbol))
            {
                var items = _symbolTable[symbol];
                if (items.Count == 1)
                {
                    _symbolTable.Remove(symbol);
                }
                else
                {
                    items.Remove(item);

                    if (items.Count == 1)
                    {
                        // remove any duplicate symbol errors on the last remaining item with this symbol
                        var otherItem = items[0];
                        RemoveErrorsForEFObject(otherItem, ErrorClass.ResolveError, ErrorCodes.NORMALIZE_DUPLICATE_SYMBOL_DEFINED);
                    }
                }

                // remove any existing duplicate symbol errors on this item
                RemoveErrorsForEFObject(item, ErrorClass.ResolveError, ErrorCodes.NORMALIZE_DUPLICATE_SYMBOL_DEFINED);
            }
        }

        /// <summary>
        ///     Looks up the symbol and returns the item (if any)
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>Returns null if the symbol isn't found</returns>
        internal EFElement LookupSymbol(Symbol symbol)
        {
            if (!_symbolTable.ContainsKey(symbol))
            {
                return null;
            }
            // just return first entry in the list
            return _symbolTable[symbol][0];
        }

        /// <summary>
        ///     Returns the complete list of items associated with the passed in symbol.
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns>The list of items with the symbol, the list may be empty</returns>
        internal IList<EFElement> GetSymbolList(Symbol symbol)
        {
            List<EFElement> symbolList;

            return _symbolTable.TryGetValue(symbol, out symbolList)
                       ? symbolList
                       : new List<EFElement>();
        }

        /// <summary>
        ///     Returns a set of EFElement's whose symbol's first part is
        ///     equal to the passed in parameter
        /// </summary>
        /// <param name="firstPart">first part to match</param>
        /// <returns></returns>
        internal HashSet<EFElement> GetElementsContainingFirstSymbolPart(string firstPart)
        {
            var elements = new HashSet<EFElement>();

            if (string.IsNullOrEmpty(firstPart))
            {
                Debug.Assert(false, "GetElementsContainingFirstSymbolPart() requires non-null, non-empty firstPart parameter");
                return elements;
            }

            foreach (var entry in _symbolTable)
            {
                var symbol = entry.Key;
                if (firstPart == symbol.GetFirstPart())
                {
                    foreach (var element in entry.Value)
                    {
                        elements.Add(element);
                    }
                }
            }

            return elements;
        }

        /// <summary>
        ///     Remove an artifact from this artifact set, and clear all symbols defined by the artifact
        /// </summary>
        /// <param name="artifact"></param>
        internal void RemoveArtifact(EFArtifact artifact)
        {
            // need to also remove the symbols for this artifact
            var keysToRemove = new List<Symbol>();

            foreach (var key in _symbolTable.Keys)
            {
                var symbolValues = _symbolTable[key];
                for (var i = symbolValues.Count - 1; i >= 0; i--)
                {
                    var item = symbolValues[i];
                    if (item.Artifact == artifact)
                    {
                        symbolValues.RemoveAt(i);
                    }
                }

                if (symbolValues.Count == 0)
                {
                    keysToRemove.Add(key);
                }
            }

            foreach (var keyToRemove in keysToRemove)
            {
                _symbolTable.Remove(keyToRemove);
            }

            _artifacts2Errors.Remove(artifact);
        }

        /// <summary>
        ///     Adds an dependency to this Element.  Also updates element's Anti-dependency list
        /// </summary>
        /// <param name="element"></param>
        internal void AddDependency(EFObject item, EFObject dependency)
        {
            _dependencyGraph.AddDependency(item, dependency);
        }

        /// <summary>
        ///     Removes a dependency from this element.  Also updates element's anti-dependency list.
        /// </summary>
        /// <param name="element"></param>
        internal void RemoveDependency(EFObject item, EFObject dependency)
        {
            _dependencyGraph.RemoveDependency(item, dependency);
        }

        internal ICollection<EFObject> GetAntiDependencies(EFObject item)
        {
            return _dependencyGraph.GetAntiDependencies(item);
        }

        internal bool IsValidityDirtyForErrorClass(ErrorClass errorClassMask)
        {
            return Artifacts.Any(artifact => artifact.IsValidityDirtyForErrorClass(errorClassMask));
        }

        /// <summary>
        ///     use this to add a resolve or semantic validation error.  These are added to the EFArtifactSet, and not the EFArtifact
        ///     because they occur in the context of other files in the set.
        ///     Note that the ErrorInfo cannot contain an ErrorClass mask made up of multiple ErrorClass flags.
        /// </summary>
        internal virtual void AddError(ErrorInfo errorInfo)
        {
            Debug.Assert(errorInfo != null, "Unexpected null value passed into AddError()");
            Debug.Assert(errorInfo.Item != null, "errorInfo had null Item");
            Debug.Assert(errorInfo.Item.Artifact != null, "errorInfo.Item.Artifact was null");
            Debug.Assert(
                Enum.GetName(typeof(ErrorClass), errorInfo.ErrorClass) != null,
                "The specified error class is not a value within the ErrorClass type");
            Debug.Assert(
                Enum.GetValues(typeof(ErrorClass))
                    .Cast<uint>()
                    .Where(v => !IsCompositeErrorClass(v) && v != 0)
                    .Contains((uint)errorInfo.ErrorClass),
                "Unexpected ErrorClass of ErrorInfo; it should be a non-zero, non-composite (one and only 1 bit should be set)");

            var errorClass2ErrorInfo = _artifacts2Errors[errorInfo.Item.Artifact];
            ICollection<ErrorInfo> errors;
            if (!errorClass2ErrorInfo.TryGetValue(errorInfo.ErrorClass, out errors))
            {
                errors = new List<ErrorInfo>();
                errorClass2ErrorInfo[errorInfo.ErrorClass] = errors;
            }

            errors.Add(errorInfo);
        }

        internal void RemoveErrorsForEFObject(EFObject item)
        {
            Debug.Assert(item.Artifact != null);
            if (item.Artifact == null)
            {
                return;
            }

            if (_artifacts2Errors.ContainsKey(item.Artifact))
            {
                var errorClass2ErrorInfo = _artifacts2Errors[item.Artifact];
                foreach (var errorsForClass in errorClass2ErrorInfo.Values)
                {
                    // store these off because we can't remove while iterating
                    var errorsToRemove = new List<ErrorInfo>();

                    // find any errors that reference the passed in item
                    foreach (var errorInfo in errorsForClass)
                    {
                        if (errorInfo.Item == item)
                        {
                            errorsToRemove.Add(errorInfo);
                        }
                    }

                    // go remove the ones we found
                    foreach (var removeMe in errorsToRemove)
                    {
                        errorsForClass.Remove(removeMe);
                    }
                }
            }
        }

        /// <summary>
        ///     This will remove all errors that are tied to a specified artifact.
        ///     This is necessary for cases such as rename, where we don't want validation post-rename to contain
        ///     the errors for the old artifact (resulting in duplicate errors)
        /// </summary>
        /// <param name="artifact"></param>
        internal void RemoveErrorsForArtifact(EFArtifact artifact)
        {
            if (_artifacts2Errors.ContainsKey(artifact))
            {
                var errorClass2ErrorInfo = _artifacts2Errors[artifact];
                foreach (var errorsForClass in errorClass2ErrorInfo.Values)
                {
                    // store these off because we can't remove while iterating
                    var errorsToRemove = new List<ErrorInfo>();

                    // find any errors that reference the passed in item
                    foreach (var errorInfo in errorsForClass)
                    {
                        if (errorInfo.Item.Artifact == artifact)
                        {
                            errorsToRemove.Add(errorInfo);
                        }
                    }

                    // go remove the ones we found
                    foreach (var removeMe in errorsToRemove)
                    {
                        errorsForClass.Remove(removeMe);
                    }
                }
            }
        }

        internal void RemoveErrorsForArtifact(EFArtifact artifact, ErrorClass errorClass)
        {
            if (_artifacts2Errors.ContainsKey(artifact))
            {
                var errorClass2ErrorInfo = _artifacts2Errors[artifact];
                foreach (var errorInfos in GetErrorInfosUsingMask(errorClass2ErrorInfo, errorClass))
                {
                    errorInfos.Clear();
                }
            }
        }

        internal void RemoveErrorsForEFObject(EFObject efobject, ErrorClass errorClass, int errorCodeToRemove)
        {
            Dictionary<ErrorClass, ICollection<ErrorInfo>> errorClass2ErrorInfo = null;
            _artifacts2Errors.TryGetValue(efobject.Artifact, out errorClass2ErrorInfo);
            if (errorClass2ErrorInfo != null)
            {
                foreach (var errors in GetErrorInfosUsingMask(errorClass2ErrorInfo, errorClass))
                {
                    var errorsToRemove = new List<ErrorInfo>();
                    foreach (var errorInfo in errors)
                    {
                        if (errorInfo.ErrorCode == errorCodeToRemove)
                        {
                            errorsToRemove.Add(errorInfo);
                        }
                    }

                    foreach (var errorInfo in errorsToRemove)
                    {
                        errors.Remove(errorInfo);
                    }
                }
            }
        }

        /// <summary>
        ///     This will return a collection of errors for all errors in the artifact.  This includes parse errors on the artifact,
        ///     as well as resolve and validation errors that occurred on this artifact in this artifact set.
        /// </summary>
        /// <param name="artifact"></param>
        internal ICollection<ErrorInfo> GetAllErrorsForArtifact(EFArtifact artifact)
        {
            var allErrors = new List<ErrorInfo>();
            GetAllErrorsForArtifact(artifact, allErrors);
            return allErrors;
        }

        /// <summary>
        ///     This will return a collection of errors for the artifact only (without traversing children).
        /// </summary>
        /// <param name="artifact"></param>
        internal ICollection<ErrorInfo> GetArtifactOnlyErrors(EFArtifact artifact)
        {
            var errors = new List<ErrorInfo>();
            var errorClass2ErrorInfo = _artifacts2Errors[artifact];
            foreach (var errorsForClass in errorClass2ErrorInfo.Values)
            {
                errors.AddRange(errorsForClass);
            }
            return errors;
        }

        private void GetAllErrorsForArtifact(EFArtifact artifact, List<ErrorInfo> allErrors)
        {
            var errorClass2ErrorInfo = _artifacts2Errors[artifact];
            foreach (var errorsForClass in errorClass2ErrorInfo.Values)
            {
                allErrors.AddRange(errorsForClass);
            }

            allErrors.AddRange(artifact.GetAllParseErrorsForArtifact());
        }

        /// <summary>
        ///     Clear all errors of the given class for all Artifacts in this ArtifactSet.
        /// </summary>
        /// <param name="errorClass"></param>
        internal void ClearErrors(ErrorClass errorClass)
        {
            foreach (var errorClass2ErrorInfo in _artifacts2Errors.Values)
            {
                foreach (var errorList in GetErrorInfosUsingMask(errorClass2ErrorInfo, errorClass))
                {
                    errorList.Clear();
                }
            }
        }

        internal virtual Version SchemaVersion
        {
            get
            {
                Debug.Assert(true, "EFArtifactSet's SchemaVersion is called");
                return null;
            }
        }

        internal ICollection<ErrorInfo> GetAllErrors()
        {
            var allErrors = new List<ErrorInfo>();
            foreach (var artifact in Artifacts)
            {
                GetAllErrorsForArtifact(artifact, allErrors);
            }
            return allErrors;
        }

        internal ICollection<ErrorInfo> GetErrors(ErrorClass errorClass)
        {
            var allErrors = new List<ErrorInfo>();
            foreach (var artifact in Artifacts)
            {
                GetErrorsForArtifact(artifact, allErrors, errorClass);
            }
            return allErrors;
        }

        internal void GetErrorsForArtifact(EFArtifact artifact, List<ErrorInfo> errors, ErrorClass errorClass)
        {
            Dictionary<ErrorClass, ICollection<ErrorInfo>> errorClass2ErrorInfo = null;

            _artifacts2Errors.TryGetValue(artifact, out errorClass2ErrorInfo);

            if (errorClass2ErrorInfo != null)
            {
                foreach (var e in GetErrorInfosUsingMask(errorClass2ErrorInfo, errorClass))
                {
                    errors.AddRange(e);
                }
            }
        }

        /// <summary>
        ///     Checks to see if there is one and only one bit set for the specified error class (it is a power of 2)
        /// </summary>
        private static bool IsCompositeErrorClass(uint errorClass)
        {
            return ((errorClass & (errorClass - 1)) != 0);
        }

        private static IEnumerable<uint> _errorClassValues;

        private static IEnumerable<uint> ErrorClassValues
        {
            get
            {
                return _errorClassValues ??
                       (_errorClassValues = Enum.GetValues(typeof(ErrorClass)).Cast<uint>().ToList());
            }
        }

        /// <summary>
        ///     This will mask the dictionary using an error class, returning back a list of error lists
        /// </summary>
        private IEnumerable<ICollection<ErrorInfo>> GetErrorInfosUsingMask(
            Dictionary<ErrorClass, ICollection<ErrorInfo>> errorClass2ErrorInfos, ErrorClass errorClassMask)
        {
            var nonCompositeEnumValues = ErrorClassValues.Where(v => !IsCompositeErrorClass(v) && v != 0);

            foreach (var errorClass in nonCompositeEnumValues.Cast<ErrorClass>())
            {
                if ((errorClass & errorClassMask) != 0)
                {
                    if (errorClass2ErrorInfos.ContainsKey(errorClass))
                    {
                        yield return errorClass2ErrorInfos[errorClass];
                    }
                }
            }
        }

        #region Test only code

        internal List<string> GetSymbols()
        {
            var symbols = new List<string>();
            foreach (var key in _symbolTable.Keys)
            {
                var symbolValues = _symbolTable[key];
                var sb = new StringBuilder();
                sb.Append(key.ToDebugString() + "|");
                for (var i = 0; i < symbolValues.Count; i++)
                {
                    sb.Append(symbolValues[i].GetType().Name);
                    if (i < symbolValues.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }

                symbols.Add(sb.ToString());
            }
            return symbols;
        }

        internal String GetDependencyGraphAsString()
        {
            return _dependencyGraph.ToPrettyString();
        }

        internal ICollection<EFObject> GetAntiDependenciesClosure(EFObject item)
        {
            return _dependencyGraph.GetAntiDependenciesClosure(item);
        }

        #endregion
    }
}
