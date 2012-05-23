namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    /// <summary>
    /// Enumeration of possible operators. 
    /// </summary>
    /// <remarks>
    /// The values are used to determine the order of operations (in the absence of any strong dependencies). 
    /// The chosen order is based on the observation that hidden dependencies (e.g. due to temporary keys in 
    /// the state manager or unknown FKs) favor deletes before inserts and updates before deletes. For instance, 
    /// a deleted entity may have the same real key value as an inserted entity. Similarly, a self-reference 
    /// may require a new dependent row to be updated before the prinpical row is inserted. Obviously, the actual
    /// constraints are required to make reliable decisions so this ordering is merely a heuristic.
    /// </remarks>
    internal enum ModificationOperator : byte
    {
        Update = 0,
        Delete = 1,
        Insert = 2,
    }
}
