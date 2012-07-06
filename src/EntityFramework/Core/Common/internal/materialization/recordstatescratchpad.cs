namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Used in the Translator to aggregate information about a (nested) record
    /// state.  After the translator visits the columnMaps, it will compile
    /// the recordState(s) which produces an immutable RecordStateFactory that 
    /// can be shared amongst many query instances.
    /// </summary>
    internal class RecordStateScratchpad
    {
        internal int StateSlotNumber { get; set; }

        internal int ColumnCount { get; set; }

        internal DataRecordInfo DataRecordInfo { get; set; }

        internal Expression GatherData { get; set; }

        internal string[] PropertyNames { get; set; }

        internal TypeUsage[] TypeUsages { get; set; }

        private readonly List<RecordStateScratchpad> _nestedRecordStateScratchpads = new List<RecordStateScratchpad>();

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal RecordStateFactory Compile()
        {
            var nestedRecordStateFactories = new RecordStateFactory[_nestedRecordStateScratchpads.Count];
            for (var i = 0; i < nestedRecordStateFactories.Length; i++)
            {
                nestedRecordStateFactories[i] = _nestedRecordStateScratchpads[i].Compile();
            }

            var result = (RecordStateFactory)Activator.CreateInstance(
                typeof(RecordStateFactory), new object[]
                    {
                        StateSlotNumber,
                        ColumnCount,
                        nestedRecordStateFactories,
                        DataRecordInfo,
                        GatherData,
                        PropertyNames,
                        TypeUsages
                    });
            return result;
        }
    }
}
