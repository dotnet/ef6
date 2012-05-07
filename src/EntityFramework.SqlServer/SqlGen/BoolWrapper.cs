namespace System.Data.Entity.SqlServer.SqlGen
{
    /// <summary>
    /// Used for wrapping a boolean value as an object.
    /// </summary>
    internal class BoolWrapper
    {
        internal bool Value { get; set; }

        internal BoolWrapper()
        {
            Value = false;
        }
    }
}