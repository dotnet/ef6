namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Xml;

    internal abstract class FunctionImportStructuralTypeMapping
    {
        internal readonly LineInfo LineInfo;
        internal readonly Collection<FunctionImportReturnTypePropertyMapping> ColumnsRenameList;

        internal FunctionImportStructuralTypeMapping(
            Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList, LineInfo lineInfo)
        {
            ColumnsRenameList = columnsRenameList;
            LineInfo = lineInfo;
        }
    }

    internal sealed class FunctionImportEntityTypeMapping : FunctionImportStructuralTypeMapping
    {
        internal FunctionImportEntityTypeMapping(
            IEnumerable<EntityType> isOfTypeEntityTypes,
            IEnumerable<EntityType> entityTypes, IEnumerable<FunctionImportEntityTypeMappingCondition> conditions,
            Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList,
            LineInfo lineInfo)
            : base(columnsRenameList, lineInfo)
        {
            Contract.Requires(isOfTypeEntityTypes != null);
            Contract.Requires(entityTypes != null);
            Contract.Requires(conditions != null);

            IsOfTypeEntityTypes = new ReadOnlyCollection<EntityType>(isOfTypeEntityTypes.ToList());
            EntityTypes = new ReadOnlyCollection<EntityType>(entityTypes.ToList());
            Conditions = new ReadOnlyCollection<FunctionImportEntityTypeMappingCondition>(conditions.ToList());
        }

        internal readonly ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> Conditions;
        internal readonly ReadOnlyCollection<EntityType> EntityTypes;
        internal readonly ReadOnlyCollection<EntityType> IsOfTypeEntityTypes;

        /// <summary>
        /// Gets all (concrete) entity types implied by this type mapping.
        /// </summary>
        internal IEnumerable<EntityType> GetMappedEntityTypes(ItemCollection itemCollection)
        {
            const bool includeAbstractTypes = false;
            return EntityTypes.Concat(
                IsOfTypeEntityTypes.SelectMany(
                    entityType =>
                    MetadataHelper.GetTypeAndSubtypesOf(entityType, itemCollection, includeAbstractTypes)
                        .Cast<EntityType>()));
        }

        internal IEnumerable<String> GetDiscriminatorColumns()
        {
            return Conditions.Select(condition => condition.ColumnName);
        }
    }

    internal sealed class FunctionImportComplexTypeMapping : FunctionImportStructuralTypeMapping
    {
        internal readonly ComplexType ReturnType;

        internal FunctionImportComplexTypeMapping(
            ComplexType returnType, Collection<FunctionImportReturnTypePropertyMapping> columnsRenameList, LineInfo lineInfo)
            : base(columnsRenameList, lineInfo)
        {
            ReturnType = returnType;
        }
    }

    internal abstract class FunctionImportReturnTypePropertyMapping
    {
        internal readonly string CMember;
        internal readonly string SColumn;
        internal readonly LineInfo LineInfo;

        internal FunctionImportReturnTypePropertyMapping(string cMember, string sColumn, LineInfo lineInfo)
        {
            CMember = cMember;
            SColumn = sColumn;
            LineInfo = lineInfo;
        }
    }

    internal sealed class FunctionImportReturnTypeScalarPropertyMapping : FunctionImportReturnTypePropertyMapping
    {
        internal FunctionImportReturnTypeScalarPropertyMapping(string cMember, string sColumn, LineInfo lineInfo)
            : base(cMember, sColumn, lineInfo)
        {
        }
    }

    /// <summary>
    /// extract the column rename info from polymorphic entity type mappings
    /// </summary>
    internal sealed class FunctionImportReturnTypeEntityTypeColumnsRenameBuilder
    {
        /// <summary>
        /// CMember -> SMember*
        /// </summary>
        internal Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> ColumnRenameMapping;

        internal FunctionImportReturnTypeEntityTypeColumnsRenameBuilder(
            Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>> isOfTypeEntityTypeColumnsRenameMapping,
            Dictionary<EntityType, Collection<FunctionImportReturnTypePropertyMapping>> entityTypeColumnsRenameMapping)
        {
            Contract.Requires(isOfTypeEntityTypeColumnsRenameMapping != null);
            Contract.Requires(entityTypeColumnsRenameMapping != null);

            ColumnRenameMapping = new Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping>();

            // Assign the columns renameMapping to the result dictionary.
            foreach (var entityType in isOfTypeEntityTypeColumnsRenameMapping.Keys)
            {
                SetStructuralTypeColumnsRename(
                    entityType, isOfTypeEntityTypeColumnsRenameMapping[entityType], true /*isTypeOf*/);
            }

            foreach (var entityType in entityTypeColumnsRenameMapping.Keys)
            {
                SetStructuralTypeColumnsRename(
                    entityType, entityTypeColumnsRenameMapping[entityType], false /*isTypeOf*/);
            }
        }

        /// <summary>
        /// Set the column mappings for each defaultMemberName.
        /// </summary>
        private void SetStructuralTypeColumnsRename(
            EntityType entityType,
            Collection<FunctionImportReturnTypePropertyMapping> columnsRenameMapping,
            bool isTypeOf)
        {
            Contract.Requires(entityType != null);
            Contract.Requires(columnsRenameMapping != null);

            foreach (var mapping in columnsRenameMapping)
            {
                if (!ColumnRenameMapping.Keys.Contains(mapping.CMember))
                {
                    ColumnRenameMapping[mapping.CMember] = new FunctionImportReturnTypeStructuralTypeColumnRenameMapping(mapping.CMember);
                }
                ColumnRenameMapping[mapping.CMember].AddRename(
                    new FunctionImportReturnTypeStructuralTypeColumn(mapping.SColumn, entityType, isTypeOf, mapping.LineInfo));
            }
        }
    }

    internal sealed class FunctionImportReturnTypeStructuralTypeColumn
    {
        internal readonly StructuralType Type;
        internal readonly bool IsTypeOf;
        internal readonly string ColumnName;
        internal readonly LineInfo LineInfo;

        internal FunctionImportReturnTypeStructuralTypeColumn(string columnName, StructuralType type, bool isTypeOf, LineInfo lineInfo)
        {
            ColumnName = columnName;
            IsTypeOf = isTypeOf;
            Type = type;
            LineInfo = lineInfo;
        }
    }

    internal class FunctionImportReturnTypeStructuralTypeColumnRenameMapping
    {
        private readonly Collection<FunctionImportReturnTypeStructuralTypeColumn> _columnListForType;
        private readonly Collection<FunctionImportReturnTypeStructuralTypeColumn> _columnListForIsTypeOfType;

        /// <summary>
        /// Null if default mapping is not allowed.
        /// </summary>
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

        /// <summary>
        /// <see cref="GetRename(EdmType, out IXmlLineInfo)"/> for more info.
        /// </summary>
        internal string GetRename(EdmType type)
        {
            IXmlLineInfo lineInfo;
            return GetRename(type, out lineInfo);
        }

        /// <summary>
        /// A default mapping (property "Foo" maps by convention to column "Foo"), if allowed, has the lowest precedence.
        /// A mapping for a specific type (EntityType="Bar") takes precedence over a mapping for a hierarchy (EntityType="IsTypeOf(Bar)"))
        /// If there are two hierarchy mappings, the most specific mapping takes precedence. 
        /// For instance, given the types Base, Derived1 : Base, and Derived2 : Derived1, 
        /// w.r.t. Derived1 "IsTypeOf(Derived1)" takes precedence over "IsTypeOf(Base)" when you ask for the rename of Derived1
        /// </summary>
        /// <param name="lineInfo">Empty for default rename mapping.</param>
        internal string GetRename(EdmType type, out IXmlLineInfo lineInfo)
        {
            Contract.Requires(type != null);

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
                var nodesInBaseHierachy =
                    _columnListForIsTypeOfType.Where(t => t.Type.IsAssignableFrom(typeForRename));

                if (nodesInBaseHierachy.Count() == 0)
                {
                    // non of its parent is renamed, so it will take the default one
                    return new FunctionImportReturnTypeStructuralTypeColumn(_defaultMemberName, typeForRename, false, null);
                }
                else
                {
                    // we will guarantee that there will be some mapping for us on this column
                    // find out which one is lowest on the link
                    return GetLowestParentInHierachy(nodesInBaseHierachy);
                }
            }
        }

        private static FunctionImportReturnTypeStructuralTypeColumn GetLowestParentInHierachy(
            IEnumerable<FunctionImportReturnTypeStructuralTypeColumn> nodesInHierachy)
        {
            FunctionImportReturnTypeStructuralTypeColumn lowestParent = null;
            foreach (var node in nodesInHierachy)
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
            Contract.Requires(renamedColumn != null);

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
