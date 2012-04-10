namespace System.Data.Entity.Core.EntityClient
{
    /// <summary>
    /// Copied from System.Data.dll
    /// </summary>
    sealed internal class NameValuePair {
        private NameValuePair _next;

        internal NameValuePair Next {
            get {
                return _next;
            }
            set {
                if ((null != _next) || (null == value)) {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.NameValuePairNext);
                }
                _next = value;
            }
        } 
    }
}
