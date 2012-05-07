namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents the data contained in a StateEntry using internal data structures
    /// of the UpdatePipeline.
    /// </summary>
    internal struct ExtractedStateEntry
    {
        internal readonly EntityState State;
        internal readonly PropagatorResult Original;
        internal readonly PropagatorResult Current;
        internal readonly IEntityStateEntry Source;

        internal ExtractedStateEntry(EntityState state, PropagatorResult original, PropagatorResult current, IEntityStateEntry source)
        {
            State = state;
            Original = original;
            Current = current;
            Source = source;
        }

        internal ExtractedStateEntry(UpdateTranslator translator, IEntityStateEntry stateEntry)
        {
            Contract.Requires(translator != null);
            Contract.Requires(stateEntry != null);

            State = stateEntry.State;
            Source = stateEntry;

            switch (stateEntry.State)
            {
                case EntityState.Deleted:
                    Original = translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.AllModified);
                    Current = null;
                    break;
                case EntityState.Unchanged:
                    Original = translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.NoneModified);
                    Current = translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.NoneModified);
                    break;
                case EntityState.Modified:
                    Original = translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.SomeModified);
                    Current = translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.SomeModified);
                    break;
                case EntityState.Added:
                    Original = null;
                    Current = translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.AllModified);
                    break;
                default:
                    Contract.Assert(false, "Unexpected IEntityStateEntry.State for entity " + stateEntry.State);
                    Original = null;
                    Current = null;
                    break;
            }
        }
    }
}