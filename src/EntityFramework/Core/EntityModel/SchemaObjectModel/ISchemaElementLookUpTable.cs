namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;

    /// <summary>
    /// Summary description for ISchemaElementLookUpTable.
    /// </summary>
    internal interface ISchemaElementLookUpTable<T>
        where T : SchemaElement
    {
        /// <summary>
        /// 
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsKey(string key);

        /// <summary>
        /// 
        /// </summary>
        T this[string key] { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator<T> GetEnumerator();

        /// <summary>
        /// Look up a name case insensitively
        /// </summary>
        /// <param name="key">the key to look up</param>
        /// <returns>the element or null</returns>
        T LookUpEquivalentKey(string key);
    }
}
