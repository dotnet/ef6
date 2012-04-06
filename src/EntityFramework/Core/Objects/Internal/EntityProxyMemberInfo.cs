using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.Internal
{
    /// <summary>
    /// Specifies information about a proxied class member.
    /// The member must be a Property for the current implementation, 
    /// but this may be generalized later to support methods as well.
    /// </summary>
    /// <remarks>
    /// Initially, this class held a reference to the PropertyInfo that represented the proxy property.
    /// This property was unused, so it was removed.  However, it may be necessary to add it later.
    /// This is pointed out here since it may not seem obvious as to why this would be omitted.
    /// </remarks>
    internal sealed class EntityProxyMemberInfo
    {
        private readonly EdmMember _member;
        private readonly int _propertyIndex;

        internal EntityProxyMemberInfo(EdmMember member, int propertyIndex)
        {
            Debug.Assert(member != null, "member must be non-null");
            Debug.Assert(propertyIndex > -1, "propertyIndex must be non-negative");

            _member = member;
            _propertyIndex = propertyIndex;
        }

        internal EdmMember EdmMember
        {
            get { return _member; }
        }

        internal int PropertyIndex
        {
            get { return _propertyIndex; }
        }
    }
}
