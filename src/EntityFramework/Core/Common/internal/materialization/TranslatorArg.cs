namespace System.Data.Entity.Core.Common.Internal.Materialization
{
    /// <summary>
    /// Struct containing the requested type and parent column map used
    /// as the arg in the Translator visitor.
    /// </summary>
    internal struct TranslatorArg
    {
        internal readonly Type RequestedType;

        internal TranslatorArg(Type requestedType)
        {
            RequestedType = requestedType;
        }
    }
}