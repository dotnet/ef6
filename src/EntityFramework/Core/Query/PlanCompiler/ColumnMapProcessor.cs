using md = System.Data.Entity.Core.Metadata.Edm;
//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    internal class ColumnMapProcessor
    {
        #region "public" methods

        internal ColumnMap ExpandColumnMap()
        {
            // special handling for the case when the top-level var is a collection. 
            // The element type of the collection may have changed, and consequently a new
            // var will have been created. We simply create a columnmap with that var.
            if (m_varInfo.Kind
                == VarInfoKind.CollectionVarInfo)
            {
                return new VarRefColumnMap(m_columnMap.Var.Type, m_columnMap.Name, ((CollectionVarInfo)m_varInfo).NewVar);
            }
            else if (m_varInfo.Kind
                     == VarInfoKind.PrimitiveTypeVarInfo)
            {
                return new VarRefColumnMap(m_columnMap.Var.Type, m_columnMap.Name, ((PrimitiveTypeVarInfo)m_varInfo).NewVar);
            }
            else
            {
                return CreateColumnMap(m_columnMap.Var.Type, m_columnMap.Name);
            }
        }

        #endregion

        #region Constructors

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Vars")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal ColumnMapProcessor(VarRefColumnMap columnMap, VarInfo varInfo, StructuredTypeInfo typeInfo)
        {
            m_columnMap = columnMap;
            m_varInfo = varInfo;
            PlanCompiler.Assert(varInfo.NewVars != null && varInfo.NewVars.Count > 0, "No new Vars specified");
            m_varList = varInfo.NewVars.GetEnumerator();
            m_typeInfo = typeInfo;
        }

        #endregion

        #region private state

        private readonly IEnumerator<Var> m_varList;
        private readonly VarInfo m_varInfo;
        private readonly VarRefColumnMap m_columnMap;
        private readonly StructuredTypeInfo m_typeInfo;
        private const string c_TypeIdColumnName = "__TypeId"; // name of the typeid column
        private const string c_EntitySetIdColumnName = "__EntitySetId"; // name of the entityset column
        private const string c_NullSentinelColumnName = "__NullSentinel"; // name of the nullability column

        #endregion

        #region private methods

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GetNextVar")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private Var GetNextVar()
        {
            if (m_varList.MoveNext())
            {
                return m_varList.Current;
            }
            PlanCompiler.Assert(false, "Could not GetNextVar");
            return null;
        }

        /// <summary>
        /// Creates a column map for a column
        /// </summary>
        /// <param name="type">column datatype</param>
        /// <param name="name">column name</param>
        /// <returns></returns>
        private ColumnMap CreateColumnMap(md.TypeUsage type, string name)
        {
            // For simple types, create a simple column map
            // Temporarily, handle collections exactly the same way
            if (!TypeUtils.IsStructuredType(type))
            {
                return CreateSimpleColumnMap(type, name);
            }

            // At this point, we must be dealing with either a record type, a 
            // complex type, or an entity type
            return CreateStructuralColumnMap(type, name);
        }

        /// <summary>
        /// Create a column map for a complextype column
        /// </summary>
        /// <param name="typeInfo">Type information for the type</param>
        /// <param name="name">column name</param>
        /// <param name="superTypeColumnMap">Supertype info if any</param>
        /// <param name="discriminatorMap">Dictionary of typeidvalue->column map</param>
        /// <param name="allMaps">List of all maps</param>
        /// <returns></returns>
        private ComplexTypeColumnMap CreateComplexTypeColumnMap(
            TypeInfo typeInfo, string name, ComplexTypeColumnMap superTypeColumnMap,
            Dictionary<object, TypedColumnMap> discriminatorMap, List<TypedColumnMap> allMaps)
        {
            var propertyColumnMapList = new List<ColumnMap>();
            IEnumerable myProperties = null;

            SimpleColumnMap nullSentinelColumnMap = null;
            if (typeInfo.HasNullSentinelProperty)
            {
                nullSentinelColumnMap = CreateSimpleColumnMap(
                    md.Helper.GetModelTypeUsage(typeInfo.NullSentinelProperty), c_NullSentinelColumnName);
            }

            // Copy over information from my supertype if it already exists
            if (superTypeColumnMap != null)
            {
                foreach (var c in superTypeColumnMap.Properties)
                {
                    propertyColumnMapList.Add(c);
                }
                myProperties = TypeHelpers.GetDeclaredStructuralMembers(typeInfo.Type);
            }
            else
            {
                // need to get all members otherwise
                myProperties = TypeHelpers.GetAllStructuralMembers(typeInfo.Type);
            }

            // Now add on all of my "specific" properties
            foreach (md.EdmMember property in myProperties)
            {
                var propertyColumnMap = CreateColumnMap(md.Helper.GetModelTypeUsage(property), property.Name);
                propertyColumnMapList.Add(propertyColumnMap);
            }

            // Create a map for myself
            var columnMap = new ComplexTypeColumnMap(typeInfo.Type, name, propertyColumnMapList.ToArray(), nullSentinelColumnMap);

            // if a dictionary is supplied, add myself to the dictionary
            if (discriminatorMap != null)
            {
                discriminatorMap[typeInfo.TypeId] = columnMap;
            }
            if (allMaps != null)
            {
                allMaps.Add(columnMap);
            }
            // Finally walk through my subtypes - use the same column name
            foreach (var subTypeInfo in typeInfo.ImmediateSubTypes)
            {
                CreateComplexTypeColumnMap(subTypeInfo, name, columnMap, discriminatorMap, allMaps);
            }

            return columnMap;
        }

        /// <summary>
        /// Create a column map for an entitytype column. 
        /// Currently, the key columns are not duplicated (ie) they point into the 
        /// same locations as in the properties list.
        /// Note: we also don't handle keys that are properties of nested fields
        /// </summary>
        /// <param name="typeInfo">Type information for the type</param>
        /// <param name="name">column name</param>
        /// <param name="superTypeColumnMap">supertype information if any</param>
        /// <param name="discriminatorMap">Dictionary of typeid->column map information</param>
        /// <param name="allMaps">List of all column maps (including those without typeid)</param>
        /// <param name="handleRelProperties">should we handle rel-properties?</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "keyColumnMap")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EntityType")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private EntityColumnMap CreateEntityColumnMap(
            TypeInfo typeInfo, string name, EntityColumnMap superTypeColumnMap,
            Dictionary<object, TypedColumnMap> discriminatorMap, List<TypedColumnMap> allMaps, bool handleRelProperties)
        {
            EntityColumnMap columnMap = null;
            var propertyColumnMapList = new List<ColumnMap>();

            // Copy over information from my supertype if it already exists
            if (superTypeColumnMap != null)
            {
                // get supertype properties
                foreach (var c in superTypeColumnMap.Properties)
                {
                    propertyColumnMapList.Add(c);
                }
                // Now add on all of my "specific" properties
                foreach (md.EdmMember property in TypeHelpers.GetDeclaredStructuralMembers(typeInfo.Type))
                {
                    var propertyColumnMap = CreateColumnMap(md.Helper.GetModelTypeUsage(property), property.Name);
                    propertyColumnMapList.Add(propertyColumnMap);
                }
                // create the entity column map w/ information from my supertype
                columnMap = new EntityColumnMap(typeInfo.Type, name, propertyColumnMapList.ToArray(), superTypeColumnMap.EntityIdentity);
            }
            else
            {
                SimpleColumnMap entitySetIdColumnMap = null;
                if (typeInfo.HasEntitySetIdProperty)
                {
                    entitySetIdColumnMap = CreateEntitySetIdColumnMap(typeInfo.EntitySetIdProperty);
                }

                // build up a list of key columns
                var keyColumnMapList = new List<SimpleColumnMap>();
                // Create a dictionary to look up the key properties
                var keyPropertyMap = new Dictionary<md.EdmProperty, ColumnMap>();

                foreach (md.EdmMember property in TypeHelpers.GetDeclaredStructuralMembers(typeInfo.Type))
                {
                    var propertyColumnMap = CreateColumnMap(md.Helper.GetModelTypeUsage(property), property.Name);
                    propertyColumnMapList.Add(propertyColumnMap);
                    // add property to keymap, if this property is part of the key
                    if (md.TypeSemantics.IsPartOfKey(property))
                    {
                        var edmProperty = property as md.EdmProperty;
                        PlanCompiler.Assert(edmProperty != null, "EntityType key member is not property?");
                        keyPropertyMap[edmProperty] = propertyColumnMap;
                    }
                }

                // Build up the key list if required
                foreach (var keyProperty in TypeHelpers.GetEdmType<md.EntityType>(typeInfo.Type).KeyMembers)
                {
                    var edmKeyProperty = keyProperty as md.EdmProperty;
                    PlanCompiler.Assert(edmKeyProperty != null, "EntityType key member is not property?");
                    var keyColumnMap = keyPropertyMap[edmKeyProperty] as SimpleColumnMap;
                    PlanCompiler.Assert(keyColumnMap != null, "keyColumnMap is null");
                    keyColumnMapList.Add(keyColumnMap);
                }

                //
                // Create the entity identity. 
                //
                var identity = CreateEntityIdentity((md.EntityType)typeInfo.Type.EdmType, entitySetIdColumnMap, keyColumnMapList.ToArray());

                // finally create the entity column map
                columnMap = new EntityColumnMap(typeInfo.Type, name, propertyColumnMapList.ToArray(), identity);
            }

            // if a dictionary is supplied, add myself to the dictionary (abstract types need not be added)
            if (discriminatorMap != null)
            {
                // where DiscriminatedNewInstanceOp is used, there will not be an explicit type id for an abstract type
                // or types that do not appear in the QueryView
                // (the mapping will not include such information)
                if (null != typeInfo.TypeId)
                {
                    discriminatorMap[typeInfo.TypeId] = columnMap;
                }
            }
            if (allMaps != null)
            {
                allMaps.Add(columnMap);
            }
            // Finally walk through my subtypes
            foreach (var subTypeInfo in typeInfo.ImmediateSubTypes)
            {
                CreateEntityColumnMap(subTypeInfo, name, columnMap, discriminatorMap, allMaps, false);
            }

            //
            // Build up the list of rel property column maps
            //
            if (handleRelProperties)
            {
                BuildRelPropertyColumnMaps(typeInfo, true);
            }
            return columnMap;
        }

        /// <summary>
        /// Build up the list of columnmaps for the relproperties. 
        /// Assumption: rel-properties follow after ALL the regular properties of the
        /// types in the type hierarchy.
        /// For now, we're simply going to ignore the rel-property columnmaps - we're
        /// just going to use this function to "drain" the corresponding vars
        /// </summary>
        /// <param name="typeInfo">typeinfo for the entity type</param>
        /// <param name="includeSupertypeRelProperties">should we get rel-properties from our supertype instances</param>
        private void BuildRelPropertyColumnMaps(TypeInfo typeInfo, bool includeSupertypeRelProperties)
        {
            //
            // Get the appropriate set of rel-properties
            //
            IEnumerable<RelProperty> relProperties = null;

            if (includeSupertypeRelProperties)
            {
                relProperties = m_typeInfo.RelPropertyHelper.GetRelProperties(typeInfo.Type.EdmType as md.EntityTypeBase);
            }
            else
            {
                relProperties = m_typeInfo.RelPropertyHelper.GetDeclaredOnlyRelProperties(typeInfo.Type.EdmType as md.EntityTypeBase);
            }

            //
            // Create a column-map for each rel-properties
            //
            foreach (var property in relProperties)
            {
                var propertyColumnMap = CreateColumnMap(property.ToEnd.TypeUsage, property.ToString());
            }

            //
            // Add all subtypes
            //
            foreach (var subTypeInfo in typeInfo.ImmediateSubTypes)
            {
                BuildRelPropertyColumnMaps(subTypeInfo, false);
            }
        }

        /// <summary>
        /// Create a column map for the entitysetid column
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private SimpleColumnMap CreateEntitySetIdColumnMap(md.EdmProperty prop)
        {
            return CreateSimpleColumnMap(md.Helper.GetModelTypeUsage(prop), c_EntitySetIdColumnName);
        }

        /// <summary>
        /// Creates a column map for a polymorphic type. This method first
        /// creates column maps for each type that is a subtype of the input type,
        /// and then creates a dictionary of typeid value -> column
        /// Finally, a PolymorphicColumnMap is created with these pieces of information
        /// </summary>
        /// <param name="typeInfo">Info about the type</param>
        /// <param name="name">column name</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private SimplePolymorphicColumnMap CreatePolymorphicColumnMap(TypeInfo typeInfo, string name)
        {
            // if the typeInfo has a DiscriminatorMap, use TrailingSpaceComparer to ensure that lookups
            // against discriminator values that SQL Server has right-padded (e.g. nchar and char) are properly
            // interpreted
            var discriminatorMap = new Dictionary<object, TypedColumnMap>(
                typeInfo.RootType.DiscriminatorMap == null ? null : TrailingSpaceComparer.Instance);
            // abstract types may not have discriminator values, but may nonetheless be interesting
            var allMaps = new List<TypedColumnMap>();

            // SQLBUDT #433011 -- Polymorphic types must construct column maps
            //                    that map to the entire type hierarchy, so we
            //                    need to use the RootType, not the current type.
            TypeInfo rootTypeInfo = typeInfo.RootType;

            // Get the type discriminant column first
            var typeIdColumnMap = CreateTypeIdColumnMap(rootTypeInfo.TypeIdProperty);

            // Prepare a place for the constructors to put the columns on the base
            // type, as they identify them.
            TypedColumnMap rootTypeColumnMap = null;

            // process complex/entity types appropriately
            // use the same name for the column 
            if (md.TypeSemantics.IsComplexType(typeInfo.Type))
            {
                rootTypeColumnMap = CreateComplexTypeColumnMap(rootTypeInfo, name, null, discriminatorMap, allMaps);
            }
            else
            {
                rootTypeColumnMap = CreateEntityColumnMap(rootTypeInfo, name, null, discriminatorMap, allMaps, true);
            }

            // Naturally, nothing is simple; we need to walk the rootTypeColumnMap hierarchy
            // and find the column map for the type that we are supposed to have as the base
            // type of this hierarchy.

            TypedColumnMap baseTypeColumnMap = null;
            foreach (var value in allMaps)
            {
                if (md.TypeSemantics.IsStructurallyEqual(value.Type, typeInfo.Type))
                {
                    baseTypeColumnMap = value;
                    break;
                }
            }
            PlanCompiler.Assert(null != baseTypeColumnMap, "Didn't find requested type in polymorphic type hierarchy?");

            // Create a polymorphic column map
            var result = new SimplePolymorphicColumnMap(
                typeInfo.Type, name, baseTypeColumnMap.Properties, typeIdColumnMap, discriminatorMap);
            return result;
        }

        /// <summary>
        /// Create a column map for a record type. Simply iterates through the
        /// list of fields, and produces a column map for each field
        /// </summary>
        /// <param name="typeInfo">Type information for the record type</param>
        /// <param name="name">column name</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "RowType")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private RecordColumnMap CreateRecordColumnMap(TypeInfo typeInfo, string name)
        {
            PlanCompiler.Assert(typeInfo.Type.EdmType is md.RowType, "not RowType");
            SimpleColumnMap nullSentinelColumnMap = null;
            if (typeInfo.HasNullSentinelProperty)
            {
                nullSentinelColumnMap = CreateSimpleColumnMap(
                    md.Helper.GetModelTypeUsage(typeInfo.NullSentinelProperty), c_NullSentinelColumnName);
            }

            var properties = TypeHelpers.GetProperties(typeInfo.Type);
            var propertyColumnMapList = new ColumnMap[properties.Count];
            for (var i = 0; i < propertyColumnMapList.Length; ++i)
            {
                md.EdmMember property = properties[i];
                propertyColumnMapList[i] = CreateColumnMap(md.Helper.GetModelTypeUsage(property), property.Name);
            }

            var result = new RecordColumnMap(typeInfo.Type, name, propertyColumnMapList, nullSentinelColumnMap);
            return result;
        }

        /// <summary>
        /// Create a column map for a ref type
        /// </summary>
        /// <param name="typeInfo">Type information for the ref type</param>
        /// <param name="name">Name of the column</param>
        /// <returns>Column map for the ref type</returns>
        private RefColumnMap CreateRefColumnMap(TypeInfo typeInfo, string name)
        {
            SimpleColumnMap entitySetIdColumnMap = null;
            if (typeInfo.HasEntitySetIdProperty)
            {
                entitySetIdColumnMap = CreateSimpleColumnMap(
                    md.Helper.GetModelTypeUsage(typeInfo.EntitySetIdProperty), c_EntitySetIdColumnName);
            }

            // get the target entity type, 
            var entityType = (md.EntityType)(TypeHelpers.GetEdmType<md.RefType>(typeInfo.Type).ElementType);

            // Iterate through the list of "key" properties
            var keyColList = new SimpleColumnMap[entityType.KeyMembers.Count];
            for (var i = 0; i < keyColList.Length; ++i)
            {
                var property = entityType.KeyMembers[i];
                keyColList[i] = CreateSimpleColumnMap(md.Helper.GetModelTypeUsage(property), property.Name);
            }

            // Create the entity identity
            var identity = CreateEntityIdentity(entityType, entitySetIdColumnMap, keyColList);

            var result = new RefColumnMap(typeInfo.Type, name, identity);
            return result;
        }

        /// <summary>
        /// Create a simple columnmap - applies only to scalar properties
        /// (Temporarily, also for collections)
        /// Simply picks up the next available column in the reader
        /// </summary>
        /// <param name="type">Column type</param>
        /// <param name="name">column name</param>
        /// <returns>Column map for this column</returns>
        private SimpleColumnMap CreateSimpleColumnMap(md.TypeUsage type, string name)
        {
            var newVar = GetNextVar();
            SimpleColumnMap result = new VarRefColumnMap(type, name, newVar);
            return result;
        }

        /// <summary>
        /// Create a column map for the typeid column
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private SimpleColumnMap CreateTypeIdColumnMap(md.EdmProperty prop)
        {
            return CreateSimpleColumnMap(md.Helper.GetModelTypeUsage(prop), c_TypeIdColumnName);
        }

        /// <summary>
        /// Create a column map for a structural column - ref/complextype/entity/record
        /// </summary>
        /// <param name="type">Type info for the type</param>
        /// <param name="name">column name</param>
        /// <returns></returns>
        private ColumnMap CreateStructuralColumnMap(md.TypeUsage type, string name)
        {
            // Get our augmented type information for this type
            var typeInfo = m_typeInfo.GetTypeInfo(type);

            // records?
            if (md.TypeSemantics.IsRowType(type))
            {
                return CreateRecordColumnMap(typeInfo, name);
            }

            // ref?
            if (md.TypeSemantics.IsReferenceType(type))
            {
                return CreateRefColumnMap(typeInfo, name);
            }

            // polymorphic type?
            if (typeInfo.HasTypeIdProperty)
            {
                return CreatePolymorphicColumnMap(typeInfo, name);
            }

            // process complex/entity types appropriately
            if (md.TypeSemantics.IsComplexType(type))
            {
                return CreateComplexTypeColumnMap(typeInfo, name, null, null, null);
            }

            if (md.TypeSemantics.IsEntityType(type))
            {
                return CreateEntityColumnMap(typeInfo, name, null, null, null, true);
            }

            // Anything else is not supported (this currently includes relationship types)
            throw new NotSupportedException(type.Identity);
        }

        /// <summary>
        /// Build out an EntityIdentity structure - for use by EntityColumnMap and RefColumnMap
        /// </summary>
        /// <param name="entityType">the entity type in question</param>
        /// <param name="entitySetIdColumnMap">column map for the entitysetid column</param>
        /// <param name="keyColumnMaps">column maps for the keys</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "entitySet")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private EntityIdentity CreateEntityIdentity(
            md.EntityType entityType,
            SimpleColumnMap entitySetIdColumnMap,
            SimpleColumnMap[] keyColumnMaps)
        {
            //
            // If we have an entitysetid (and therefore, a column map for the entitysetid), 
            // then use a discriminated entity identity; otherwise, we use a simpleentityidentity
            // instead
            //
            if (entitySetIdColumnMap != null)
            {
                return new DiscriminatedEntityIdentity(entitySetIdColumnMap, m_typeInfo.EntitySetIdToEntitySetMap, keyColumnMaps);
            }
            else
            {
                var entitySet = m_typeInfo.GetEntitySet(entityType);
                PlanCompiler.Assert(
                    entitySet != null, "Expected non-null entitySet when no entity set ID is required. Entity type = " + entityType);
                return new SimpleEntityIdentity(entitySet, keyColumnMaps);
            }
        }

        #endregion
    }
}
