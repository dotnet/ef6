// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class StorageEntitySet : EntitySet
    {
        internal StorageEntitySet(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        internal static readonly string AttributeSchema = "Schema";
        internal static readonly string AttributeTable = "Table";

        private DefaultableValue<string> _schemaAttr;
        private DefaultableValue<string> _tableAttr;
        private DefaultableValue<string> _storeSchemaGenTypeAttr;
        private DefaultableValue<string> _storeSchemaGenSchemaAttr;
        private DefaultableValue<string> _storeSchemaGenNameAttr;

        private DefiningQuery _definingQuery;

        internal DefiningQuery DefiningQuery
        {
            get { return _definingQuery; }
            set { _definingQuery = value; }
        }

        /// <summary>
        ///     Manages the content of the Schema attribute (used for S-space only)
        /// </summary>
        internal DefaultableValue<string> Schema
        {
            get
            {
                Debug.Assert(!ModelHelper.GetBaseModelRoot(this).IsCSDL, "Expected SSDL model");

                return _schemaAttr ?? (_schemaAttr = new SchemaDefaultableValue(this));
            }
        }

        private class SchemaDefaultableValue : DefaultableValue<string>
        {
            internal SchemaDefaultableValue(EFElement parent)
                : base(parent, AttributeSchema, string.Empty)
            {
                // note: added the string.Empty namespace to distinguish from the
                // StoreSchemaGenerator Schema attribute
            }

            internal override string AttributeName
            {
                get { return AttributeSchema; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        /// <summary>
        ///     Manages the content of the Table attribute (used for S-space only)
        /// </summary>
        internal DefaultableValue<string> Table
        {
            get
            {
                Debug.Assert(!ModelHelper.GetBaseModelRoot(this).IsCSDL, "Expected SSDL model");

                return _tableAttr ?? (_tableAttr = new TableDefaultableValue(this));
            }
        }

        private class TableDefaultableValue : DefaultableValue<string>
        {
            internal TableDefaultableValue(EFElement parent)
                : base(parent, AttributeTable)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeTable; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        /// <summary>
        ///     Manages the content of the StoreSchemaGenerator Type attribute (used for S-space only)
        ///     This is a special attribute put on by the EntityStoreSchemaGenerator.
        ///     It will have the value "Tables" or "Views" representing whether the underlying
        ///     database object was a Table or a View. If not present we assume "Tables".
        /// </summary>
        internal DefaultableValue<string> StoreSchemaGeneratorType
        {
            get
            {
                if (_storeSchemaGenTypeAttr == null)
                {
                    _storeSchemaGenTypeAttr = new StoreSchemaGeneratorTypeDefaultableValue(this);
                }

                return _storeSchemaGenTypeAttr;
            }
        }

        private class StoreSchemaGeneratorTypeDefaultableValue : DefaultableValue<string>
        {
            internal StoreSchemaGeneratorTypeDefaultableValue(EFElement parent)
                : base(parent, ModelConstants.StoreSchemaGeneratorTypeAttributeName
                    , SchemaManager.GetEntityStoreSchemaGeneratorNamespaceName())
            {
            }

            internal override string AttributeName
            {
                get { return ModelConstants.StoreSchemaGeneratorTypeAttributeName; }
            }

            public override string DefaultValue
            {
                get { return ModelConstants.StoreSchemaGenTypeAttributeValueTables; }
            }
        }

        /// <summary>
        ///     Manages the content of the StoreSchemaGenerator Schema attribute (used for S-space only)
        ///     This is a special attribute put on by the EntityStoreSchemaGenerator.
        ///     It's optional. If present it overrides all other attributes to define the
        ///     schema of the database object (needed for cases where the EntitySet element has
        ///     a child DefiningQuery as in that case the standard SSDL Schema attribute
        ///     (see Schema method above) is not present).
        /// </summary>
        internal DefaultableValue<string> StoreSchemaGeneratorSchema
        {
            get
            {
                if (_storeSchemaGenSchemaAttr == null)
                {
                    _storeSchemaGenSchemaAttr = new StoreSchemaGeneratorSchemaDefaultableValue(this);
                }

                return _storeSchemaGenSchemaAttr;
            }
        }

        private class StoreSchemaGeneratorSchemaDefaultableValue : DefaultableValue<string>
        {
            internal StoreSchemaGeneratorSchemaDefaultableValue(EFElement parent)
                : base(parent, ModelConstants.StoreSchemaGeneratorSchemaAttributeName,
                    SchemaManager.GetEntityStoreSchemaGeneratorNamespaceName())
            {
            }

            internal override string AttributeName
            {
                get { return ModelConstants.StoreSchemaGeneratorSchemaAttributeName; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        /// <summary>
        ///     Manages the content of the StoreSchemaGenerator Name attribute (used for S-space only)
        ///     This is a special attribute put on by the EntityStoreSchemaGenerator.
        ///     It's optional. If present it overrides all other attributes to define the
        ///     name of the database object (needed for cases where the EntitySet element has
        ///     a child DefiningQuery as in that case the standard SSDL Table attribute
        ///     (see Table method above) may not be present).
        /// </summary>
        internal DefaultableValue<string> StoreSchemaGeneratorName
        {
            get
            {
                if (_storeSchemaGenNameAttr == null)
                {
                    _storeSchemaGenNameAttr = new StoreSchemaGeneratorNameDefaultableValue(this);
                }

                return _storeSchemaGenNameAttr;
            }
        }

        private class StoreSchemaGeneratorNameDefaultableValue : DefaultableValue<string>
        {
            internal StoreSchemaGeneratorNameDefaultableValue(EFElement parent)
                : base(parent, ModelConstants.StoreSchemaGeneratorNameAttributeName
                    , SchemaManager.GetEntityStoreSchemaGeneratorNamespaceName())
            {
            }

            internal override string AttributeName
            {
                get { return ModelConstants.StoreSchemaGeneratorNameAttributeName; }
            }

            public override string DefaultValue
            {
                get { return null; }
            }
        }

        internal bool StoreSchemaGeneratorTypeIsView
        {
            get { return (ModelConstants.StoreSchemaGenTypeAttributeValueViews == StoreSchemaGeneratorType.Value); }
        }

        internal ICollection<MappingFragment> MappingFragments
        {
            get
            {
                var antiDeps = Artifact.ArtifactSet.GetAntiDependencies(this);

                var fragments = new List<MappingFragment>();
                foreach (var antiDep in antiDeps)
                {
                    var frag = antiDep as MappingFragment;
                    if (frag == null
                        && antiDep.Parent != null)
                    {
                        frag = antiDep.Parent as MappingFragment;
                    }

                    if (frag != null)
                    {
                        fragments.Add(frag);
                    }
                }

                return fragments.AsReadOnly();
            }
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeSchema);
            s.Add(AttributeTable);
            s.Add(ModelConstants.StoreSchemaGeneratorTypeAttributeName);
            s.Add(ModelConstants.StoreSchemaGeneratorSchemaAttributeName);
            s.Add(ModelConstants.StoreSchemaGeneratorNameAttributeName);
            return s;
        }
#endif

#if DEBUG
        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(DefiningQuery.ElementName);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_definingQuery);
            _definingQuery = null;

            ClearEFObject(_schemaAttr);
            _schemaAttr = null;
            ClearEFObject(_tableAttr);
            _tableAttr = null;
            ClearEFObject(_storeSchemaGenTypeAttr);
            _storeSchemaGenTypeAttr = null;
            ClearEFObject(_storeSchemaGenSchemaAttr);
            _storeSchemaGenSchemaAttr = null;
            ClearEFObject(_storeSchemaGenNameAttr);
            _storeSchemaGenNameAttr = null;

            base.PreParse();
        }

        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == DefiningQuery.ElementName)
            {
                if (_definingQuery != null)
                {
                    // multiple DefiningQuery elements
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, elem.Name.LocalName);
                    Artifact.AddParseErrorForObject(this, msg, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED);
                }
                else
                {
                    _definingQuery = new DefiningQuery(this, elem);
                    _definingQuery.Parse(unprocessedElements);
                }
            }
            else
            {
                return base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }

        // we unfortunately get a warning from the compiler when we use the "base" keyword in "iterator" types generated by using the
        // "yield return" keyword.  By adding this method, I was able to get around this.  Unfortunately, I wasn't able to figure out
        // a way to implement this once and have derived classes share the implementation (since the "base" keyword is resolved at 
        // compile-time and not at runtime.
        private IEnumerable<EFObject> BaseChildren
        {
            get { return base.Children; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                foreach (var efobj in BaseChildren)
                {
                    yield return efobj;
                }

                if (_definingQuery != null)
                {
                    yield return _definingQuery;
                }

                yield return Schema;
                yield return Table;
                yield return StoreSchemaGeneratorType;
                yield return StoreSchemaGeneratorSchema;
                yield return StoreSchemaGeneratorName;
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            var child = efContainer as DefiningQuery;
            if (child != null)
            {
                _definingQuery = null;
                return;
            }

            base.OnChildDeleted(efContainer);
        }

        /// <summary>
        ///     Returns the name of the schema on the underlying database that
        ///     this entity represents
        /// </summary>
        internal string DatabaseSchemaName
        {
            get
            {
                var schemaNameAttr = StoreSchemaGeneratorSchema;
                if (null != schemaNameAttr
                    && !string.IsNullOrEmpty(schemaNameAttr.Value))
                {
                    // if the store:Schema attribute is present then that 
                    // overrides any other attribute and defines the schema name
                    return schemaNameAttr.Value;
                }
                else
                {
                    schemaNameAttr = Schema;
                    if (null != schemaNameAttr
                        && !string.IsNullOrEmpty(schemaNameAttr.Value))
                    {
                        // otherwise if the Schema attribute is present 
                        // then that defines the schema name
                        return schemaNameAttr.Value;
                    }
                    else
                    {
                        // if neither of the above attributes are present then the schema name is
                        // defined by the name of the containing EntityContainer
                        var sec = Parent as StorageEntityContainer;
                        Debug.Assert(
                            sec != null,
                            "Parent of StorageEntitySet should be a StorageEntityContainer. Actual parent has type "
                            + (Parent == null ? "NULL" : Parent.GetType().FullName));
                        if (null == sec)
                        {
                            return null;
                        }
                        else
                        {
                            return sec.LocalName.Value;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Returns the name of the table on the underlying database that
        ///     this entity represents
        /// </summary>
        internal string DatabaseTableName
        {
            get
            {
                var tableNameAttr = StoreSchemaGeneratorName;
                if (!string.IsNullOrEmpty(tableNameAttr.Value))
                {
                    // if the store:Name attribute is present then that 
                    // overrides any other attribute and defines the table name
                    return tableNameAttr.Value;
                }
                else
                {
                    tableNameAttr = Table;
                    if (!string.IsNullOrEmpty(tableNameAttr.Value))
                    {
                        // otherwise if the Table attribute is present then that 
                        // defines the table name
                        return tableNameAttr.Value;
                    }
                    else
                    {
                        // if neither of the above attributes are present then the 
                        // table name is defined by the name of the EntitySet itself
                        return LocalName.Value;
                    }
                }
            }
        }
    }
}
