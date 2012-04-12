namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Summary description for ReturnValue.
    /// </summary>
    internal sealed class ReturnValue<T>
    {
        #region Instance Fields

        private bool _succeeded;
        private T _value;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        internal bool Succeeded
        {
            get { return _succeeded; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _succeeded = true;
            }
        }
    }
}
