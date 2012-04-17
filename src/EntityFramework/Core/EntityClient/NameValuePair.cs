namespace System.Data.Entity.Core.EntityClient
{
    using System.Data.Entity.Resources;

    /// <summary>
    /// Copied from System.Data.dll
    /// </summary>
    internal sealed class NameValuePair
    {
        private NameValuePair _next;

        internal NameValuePair Next
        {
            get { return _next; }
            set
            {
                if ((null != _next)
                    || (null == value))
                {
                    throw new InvalidOperationException(Strings.ADP_InternalProviderError((int)EntityUtil.InternalErrorCode.NameValuePairNext));
                }
                _next = value;
            }
        }
    }
}
