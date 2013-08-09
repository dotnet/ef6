// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    /// <summary>
    /// An immutable class used to generate new RecordStates, which are used
    /// at runtime to produce value-layer (aka DataReader) results.
    /// Contains static information collected by the Translator visitor.  The
    /// expressions produced by the Translator are compiled.  The RecordStates
    /// will refer to this object for all static information.
    /// This class is cached in the query cache as part of the CoordinatorFactory.
    /// </summary>
    internal class RecordStateFactory
    {
        #region state

        /// <summary>
        /// Indicates which state slot in the Shaper.State is expected to hold the
        /// value for this record state.  Each unique record shape has it's own state
        /// slot.
        /// </summary>
        internal readonly int StateSlotNumber;

        /// <summary>
        /// How many column values we have to reserve space for in this record.
        /// </summary>
        internal readonly int ColumnCount;

        /// <summary>
        /// The DataRecordInfo we must return for this record.  If the record represents
        /// an entity, this will be used to construct a unique EntityRecordInfo with the
        /// EntityKey and EntitySet for the entity.
        /// </summary>
        internal readonly DataRecordInfo DataRecordInfo;

        /// <summary>
        /// A function that will gather the data for the row and store it on the record state.
        /// </summary>
        internal readonly Func<Shaper, bool> GatherData;

        /// <summary>
        /// Collection of nested records for this record, such as a complex type that is
        /// part of an entity.  This does not include records that are part of a nested
        /// collection, however.
        /// </summary>
        internal readonly ReadOnlyCollection<RecordStateFactory> NestedRecordStateFactories;

        /// <summary>
        /// The name for each column.
        /// </summary>
        internal readonly ReadOnlyCollection<string> ColumnNames;

        /// <summary>
        /// The type usage information for each column.
        /// </summary>
        internal readonly ReadOnlyCollection<TypeUsage> TypeUsages;

        /// <summary>
        /// Tracks which columns might need special handling (nested readers/records)
        /// </summary>
        internal readonly ReadOnlyCollection<bool> IsColumnNested;

        /// <summary>
        /// Tracks whether there are ANY columns that need special handling.
        /// </summary>
        internal readonly bool HasNestedColumns;

        /// <summary>
        /// A helper class to make the translation from name->ordinal.
        /// </summary>
        internal readonly FieldNameLookup FieldNameLookup;

        /// <summary>
        /// Description of this RecordStateFactory, used for debugging only; while this
        /// is not  needed in retail code, it is pretty important because it's the only
        /// description we'll have once we compile the Expressions; debugging a problem
        /// with retail bits would be pretty hard without this.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private readonly string Description;

        #endregion

        #region Constructors

        public RecordStateFactory(
            int stateSlotNumber, int columnCount, RecordStateFactory[] nestedRecordStateFactories, DataRecordInfo dataRecordInfo,
            Expression<Func<Shaper, bool>> gatherData, string[] propertyNames, TypeUsage[] typeUsages, bool[] isColumnNested)
        {
            StateSlotNumber = stateSlotNumber;
            ColumnCount = columnCount;
            NestedRecordStateFactories = new ReadOnlyCollection<RecordStateFactory>(nestedRecordStateFactories);
            DataRecordInfo = dataRecordInfo;
            GatherData = gatherData.Compile();
            Description = gatherData.ToString();
            ColumnNames = new ReadOnlyCollection<string>(propertyNames);
            TypeUsages = new ReadOnlyCollection<TypeUsage>(typeUsages);

            FieldNameLookup = new FieldNameLookup(ColumnNames);

            // pre-compute the nested objects from typeUsage, for performance
            if (isColumnNested == null)
            {
                isColumnNested = new bool[columnCount];

                for (var ordinal = 0; ordinal < columnCount; ordinal++)
                {
                    switch (typeUsages[ordinal].EdmType.BuiltInTypeKind)
                    {
                        case BuiltInTypeKind.EntityType:
                        case BuiltInTypeKind.ComplexType:
                        case BuiltInTypeKind.RowType:
                        case BuiltInTypeKind.CollectionType:
                            isColumnNested[ordinal] = true;
                            HasNestedColumns = true;
                            break;
                        default:
                            isColumnNested[ordinal] = false;
                            break;
                    }
                }
            }
            IsColumnNested = new ReadOnlyCollection<bool>(isColumnNested);
        }

        public RecordStateFactory(
            int stateSlotNumber, int columnCount, RecordStateFactory[] nestedRecordStateFactories, DataRecordInfo dataRecordInfo,
            Expression gatherData, string[] propertyNames, TypeUsage[] typeUsages)
            : this(stateSlotNumber, columnCount, nestedRecordStateFactories, dataRecordInfo,
                CodeGenEmitter.BuildShaperLambda<bool>(gatherData), propertyNames, typeUsages, isColumnNested: null)
        {
        }

        #endregion

        #region "public" surface area

        /// <summary>
        /// It's GO time, create the record state.
        /// </summary>
        internal RecordState Create(CoordinatorFactory coordinatorFactory)
        {
            return new RecordState(this, coordinatorFactory);
        }

        #endregion
    }
}
