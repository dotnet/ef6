// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Supports materialization of complex type instances from records. Used
    /// by the ObjectStateManager.
    /// </summary>
    internal class ComplexTypeMaterializer
    {
        private readonly MetadataWorkspace _workspace;
        private const int MaxPlanCount = 4;
        private Plan[] _lastPlans;
        private int _lastPlanIndex;

        internal ComplexTypeMaterializer(MetadataWorkspace workspace)
        {
            _workspace = workspace;
        }

        internal object CreateComplex(IExtendedDataRecord record, DataRecordInfo recordInfo, object result)
        {
            DebugCheck.NotNull(record);
            DebugCheck.NotNull(recordInfo);
            DebugCheck.NotNull(recordInfo.RecordType);
            DebugCheck.NotNull(recordInfo.RecordType.EdmType);

            Debug.Assert(
                Helper.IsEntityType(recordInfo.RecordType.EdmType) ||
                Helper.IsComplexType(recordInfo.RecordType.EdmType),
                "not EntityType or ComplexType");

            var plan = GetPlan(recordInfo);
            if (null == result)
            {
                result = plan.ClrType();
            }
            SetProperties(record, result, plan.Properties);
            return result;
        }

        private void SetProperties(IExtendedDataRecord record, object result, PlanEdmProperty[] properties)
        {
            DebugCheck.NotNull(record);
            DebugCheck.NotNull(result);
            DebugCheck.NotNull(properties);

            for (var i = 0; i < properties.Length; ++i)
            {
                if (null != properties[i].GetExistingComplex)
                {
                    var existing = properties[i].GetExistingComplex(result);
                    var obj = CreateComplexRecursive(record.GetValue(properties[i].Ordinal), existing);
                    if (null == existing)
                    {
                        properties[i].ClrProperty(result, obj);
                    }
                }
                else
                {
                    properties[i].ClrProperty(
                        result,
                        ConvertDBNull(
                            record.GetValue(
                                properties[i].Ordinal)));
                }
            }
        }

        private static object ConvertDBNull(object value)
        {
            return ((DBNull.Value != value) ? value : null);
        }

        private object CreateComplexRecursive(object record, object existing)
        {
            return ((DBNull.Value != record) ? CreateComplexRecursive((IExtendedDataRecord)record, existing) : existing);
        }

        private object CreateComplexRecursive(IExtendedDataRecord record, object existing)
        {
            return CreateComplex(record, record.DataRecordInfo, existing);
        }

        private Plan GetPlan(DataRecordInfo recordInfo)
        {
            DebugCheck.NotNull(recordInfo);
            DebugCheck.NotNull(recordInfo.RecordType);

            var plans = _lastPlans ?? (_lastPlans = new Plan[MaxPlanCount]);

            // find an existing plan in circular buffer
            var index = _lastPlanIndex - 1;
            for (var i = 0; i < MaxPlanCount; ++i)
            {
                index = (index + 1) % MaxPlanCount;
                if (null == plans[index])
                {
                    break;
                }
                if (plans[index].Key
                    == recordInfo.RecordType)
                {
                    _lastPlanIndex = index;
                    return plans[index];
                }
            }
            Debug.Assert(0 <= index, "negative index");
            Debug.Assert(index != _lastPlanIndex || (null == plans[index]), "index wrapped around");

            // create a new plan
            var mapping = Util.GetObjectMapping(recordInfo.RecordType.EdmType, _workspace);
            Debug.Assert(null != mapping, "null ObjectTypeMapping");

            Debug.Assert(
                Helper.IsComplexType(recordInfo.RecordType.EdmType),
                "IExtendedDataRecord is not ComplexType");

            _lastPlanIndex = index;
            plans[index] = new Plan(recordInfo.RecordType, mapping, recordInfo.FieldMetadata);
            return plans[index];
        }

        private sealed class Plan
        {
            internal readonly TypeUsage Key;
            internal readonly Func<object> ClrType;
            internal readonly PlanEdmProperty[] Properties;

            internal Plan(TypeUsage key, ObjectTypeMapping mapping, ReadOnlyCollection<FieldMetadata> fields)
            {
                DebugCheck.NotNull(mapping);
                DebugCheck.NotNull(fields);

                Key = key;
                Debug.Assert(!Helper.IsEntityType(mapping.ClrType), "Expecting complex type");
                ClrType = DelegateFactory.GetConstructorDelegateForType((ClrComplexType)mapping.ClrType);
                Properties = new PlanEdmProperty[fields.Count];

                var lastOrdinal = -1;
                for (var i = 0; i < Properties.Length; ++i)
                {
                    var field = fields[i];

                    Debug.Assert(
                        unchecked((uint)field.Ordinal) < unchecked((uint)fields.Count), "FieldMetadata.Ordinal out of range of Fields.Count");
                    Debug.Assert(lastOrdinal < field.Ordinal, "FieldMetadata.Ordinal is not increasing");
                    lastOrdinal = field.Ordinal;

                    Properties[i] = new PlanEdmProperty(lastOrdinal, mapping.GetPropertyMap(field.FieldType.Name).ClrProperty);
                }
            }
        }

        private struct PlanEdmProperty
        {
            internal readonly int Ordinal;
            internal readonly Func<object, object> GetExistingComplex;
            internal readonly Action<object, object> ClrProperty;

            internal PlanEdmProperty(int ordinal, EdmProperty property)
            {
                Debug.Assert(0 <= ordinal, "negative ordinal");
                DebugCheck.NotNull(property);

                Ordinal = ordinal;
                GetExistingComplex = Helper.IsComplexType(property.TypeUsage.EdmType)
                                         ? DelegateFactory.GetGetterDelegateForProperty(property)
                                         : null;
                ClrProperty = DelegateFactory.GetSetterDelegateForProperty(property);
            }
        }
    }
}
