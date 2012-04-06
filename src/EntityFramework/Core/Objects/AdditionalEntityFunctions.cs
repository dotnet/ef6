namespace System.Data.Entity.Core.Objects
{
    public static partial class EntityFunctions
    {
        /// <summary>
        /// An ELINQ operator that ensures the input string is treated as a unicode string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsUnicode(string value)
        {
            return value;
        }

        /// <summary>
        /// An ELINQ operator that treats the input string as a non-unicode string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsNonUnicode(string value)
        {
            return value;
        }
    }
}
