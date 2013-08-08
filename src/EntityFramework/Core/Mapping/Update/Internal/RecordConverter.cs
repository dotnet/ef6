// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Converts records to new instance expressions. Assumes that all inputs come from a single data reader (because
    /// it caches record layout). If multiple readers are used, multiple converters must be constructed in case
    /// the different readers return different layouts for types.
    /// </summary>
    /// <remarks>
    /// Conventions for modifiedProperties enumeration: null means all properties are modified, empty means none,
    /// non-empty means some.
    /// </remarks>
    internal class RecordConverter
    {
        /// <summary>
        /// Initializes a new converter given a command tree context. Initializes a new record layout cache.
        /// </summary>
        /// <param name="updateTranslator">
        /// Sets <see cref="m_updateTranslator" />
        /// </param>
        internal RecordConverter(UpdateTranslator updateTranslator)
        {
            m_updateTranslator = updateTranslator;
        }

        /// <summary>
        /// Context used to produce expressions.
        /// </summary>
        private readonly UpdateTranslator m_updateTranslator;

        /// <summary>
        /// Converts original values in a state entry to a DbNewInstanceExpression. The record must be either an entity or
        /// a relationship set instance.
        /// </summary>
        /// <remarks>
        /// This method is not thread safe.
        /// </remarks>
        /// <param name="stateEntry"> Gets state entry this record is associated with. </param>
        /// <param name="modifiedPropertiesBehavior"> Indicates how to determine whether a property is modified. </param>
        /// <returns> New instance expression. </returns>
        internal PropagatorResult ConvertOriginalValuesToPropagatorResult(
            IEntityStateEntry stateEntry, ModifiedPropertiesBehavior modifiedPropertiesBehavior)
        {
            return ConvertStateEntryToPropagatorResult(
                stateEntry, useCurrentValues: false, modifiedPropertiesBehavior: modifiedPropertiesBehavior);
        }

        /// <summary>
        /// Converts current values in a state entry to a DbNewInstanceExpression. The record must be either an entity or
        /// a relationship set instance.
        /// </summary>
        /// <remarks>
        /// This method is not thread safe.
        /// </remarks>
        /// <param name="stateEntry"> Gets state entry this record is associated with. </param>
        /// <param name="modifiedPropertiesBehavior"> Indicates how to determine whether a property is modified. </param>
        /// <returns> New instance expression. </returns>
        internal PropagatorResult ConvertCurrentValuesToPropagatorResult(
            IEntityStateEntry stateEntry, ModifiedPropertiesBehavior modifiedPropertiesBehavior)
        {
            return ConvertStateEntryToPropagatorResult(
                stateEntry, useCurrentValues: true, modifiedPropertiesBehavior: modifiedPropertiesBehavior);
        }

        private PropagatorResult ConvertStateEntryToPropagatorResult(
            IEntityStateEntry stateEntry, bool useCurrentValues, ModifiedPropertiesBehavior modifiedPropertiesBehavior)
        {
            DebugCheck.NotNull(stateEntry);

            try
            {
                var record = useCurrentValues
                                 ? stateEntry.CurrentValues
                                 : (IExtendedDataRecord)stateEntry.OriginalValues;

                var isModified = false; // the root of the state entry is unchanged because the type is static
                return ExtractorMetadata.ExtractResultFromRecord(
                    stateEntry, isModified, record, useCurrentValues, m_updateTranslator, modifiedPropertiesBehavior);
            }
            catch (Exception e)
            {
                if (e.RequiresContext())
                {
                    throw EntityUtil.Update(Strings.Update_ErrorLoadingRecord, e, stateEntry);
                }
                throw;
            }
        }
    }
}
