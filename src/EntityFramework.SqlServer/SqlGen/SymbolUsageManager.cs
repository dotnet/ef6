namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Collections.Generic;

    /// <summary>
    /// Tracks the usage of symbols. 
    /// When registering a symbol with the usage manager if an input symbol is specified,
    /// than the usage of the two is 'connected' - if one ever gets marked as used, 
    /// the other one becomes 'used' too. 
    /// </summary>
    internal class SymbolUsageManager
    {
        private readonly Dictionary<Symbol, BoolWrapper> optionalColumnUsage = new Dictionary<Symbol, BoolWrapper>();

        internal bool ContainsKey(Symbol key)
        {
            return optionalColumnUsage.ContainsKey(key);
        }

        internal bool TryGetValue(Symbol key, out bool value)
        {
            BoolWrapper wrapper;
            if (optionalColumnUsage.TryGetValue(key, out wrapper))
            {
                value = wrapper.Value;
                return true;
            }

            value = false;
            return false;
        }

        internal void Add(Symbol sourceSymbol, Symbol symbolToAdd)
        {
            BoolWrapper wrapper;
            if (sourceSymbol == null
                || !optionalColumnUsage.TryGetValue(sourceSymbol, out wrapper))
            {
                wrapper = new BoolWrapper();
            }
            optionalColumnUsage.Add(symbolToAdd, wrapper);
        }

        internal void MarkAsUsed(Symbol key)
        {
            if (optionalColumnUsage.ContainsKey(key))
            {
                optionalColumnUsage[key].Value = true;
            }
        }

        internal bool IsUsed(Symbol key)
        {
            return optionalColumnUsage[key].Value;
        }
    }
}
