// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Xml;

    internal class FunctionImportReturnTypeStructuralTypeColumnRenameMapping
    {
        private readonly Collection<FunctionImportReturnTypeStructuralTypeColumn> _columnListForType;
        private readonly Collection<FunctionImportReturnTypeStructuralTypeColumn> _columnListForIsTypeOfType;

        // <summary>
        // Null if default mapping is not allowed.
        // </summary>
        private readonly string _defaultMemberName;

        private readonly Memoizer<StructuralType, FunctionImportReturnTypeStructuralTypeColumn> _renameCache;

        internal FunctionImportReturnTypeStructuralTypeColumnRenameMapping(string defaultMemberName)
        {
            _defaultMemberName = defaultMemberName;
            _columnListForType = new Collection<FunctionImportReturnTypeStructuralTypeColumn>();
            _columnListForIsTypeOfType = new Collection<FunctionImportReturnTypeStructuralTypeColumn>();
            _renameCache = new Memoizer<StructuralType, FunctionImportReturnTypeStructuralTypeColumn>(
                GetRename, EqualityComparer<StructuralType>.Default);
        }

        // <summary>
        // <see cref="GetRename(EdmType, out IXmlLineInfo)" /> for more info.
        // </summary>
        internal string GetRename(EdmType type)
        {
            IXmlLineInfo lineInfo;
            return GetRename(type, out lineInfo);
        }

        // <summary>
        // A default mapping (property "Xyz" maps by convention to column "Xyz"), if allowed, has the lowest precedence.
        // A mapping for a specific type (EntityType="Abc") takes precedence over a mapping for a hierarchy (EntityType="IsTypeOf(Abc)"))
        // If there are two hierarchy mappings, the most specific mapping takes precedence.
        // For instance, given the types Base, Derived1 : Base, and Derived2 : Derived1,
        // w.r.t. Derived1 "IsTypeOf(Derived1)" takes precedence over "IsTypeOf(Base)" when you ask for the rename of Derived1
        // </summary>
        // <param name="lineInfo"> Empty for default rename mapping. </param>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Only used in debug mode.")]
        internal string GetRename(EdmType type, out IXmlLineInfo lineInfo)
        {
            DebugCheck.NotNull(type);

            Debug.Assert(type is StructuralType, "we can only rename structural type");

            var rename = _renameCache.Evaluate(type as StructuralType);
            lineInfo = rename.LineInfo;
            return rename.ColumnName;
        }

        private FunctionImportReturnTypeStructuralTypeColumn GetRename(StructuralType typeForRename)
        {
            var ofTypecolumn = _columnListForType.FirstOrDefault(t => t.Type == typeForRename);
            if (null != ofTypecolumn)
            {
                return ofTypecolumn;
            }

            // if there are duplicate istypeof mapping defined rename for the same column, the last one wins
            var isOfTypeColumn = _columnListForIsTypeOfType.Where(t => t.Type == typeForRename).LastOrDefault();

            if (null != isOfTypeColumn)
            {
                return isOfTypeColumn;
            }
            else
            {
                // find out all the tyes that is isparent type of this lookup type
                var nodesInBaseHierarchy =
                    _columnListForIsTypeOfType.Where(t => t.Type.IsAssignableFrom(typeForRename));

                if (nodesInBaseHierarchy.Count() == 0)
                {
                    // non of its parent is renamed, so it will take the default one
                    return new FunctionImportReturnTypeStructuralTypeColumn(_defaultMemberName, typeForRename, false, null);
                }
                else
                {
                    // we will guarantee that there will be some mapping for us on this column
                    // find out which one is lowest on the link
                    return GetLowestParentInHierarchy(nodesInBaseHierarchy);
                }
            }
        }

        private static FunctionImportReturnTypeStructuralTypeColumn GetLowestParentInHierarchy(
            IEnumerable<FunctionImportReturnTypeStructuralTypeColumn> nodesInHierarchy)
        {
            FunctionImportReturnTypeStructuralTypeColumn lowestParent = null;
            foreach (var node in nodesInHierarchy)
            {
                if (lowestParent == null)
                {
                    lowestParent = node;
                }
                else if (lowestParent.Type.IsAssignableFrom(node.Type))
                {
                    lowestParent = node;
                }
            }
            Debug.Assert(null != lowestParent, "We should have the lowest parent");
            return lowestParent;
        }

        internal void AddRename(FunctionImportReturnTypeStructuralTypeColumn renamedColumn)
        {
            DebugCheck.NotNull(renamedColumn);

            if (!renamedColumn.IsTypeOf)
            {
                // add to collection if the mapping is for specific type
                _columnListForType.Add(renamedColumn);
            }
            else
            {
                _columnListForIsTypeOfType.Add(renamedColumn);
            }
        }
    }
}
