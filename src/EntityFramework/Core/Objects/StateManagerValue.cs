namespace System.Data.Entity.Core.Objects
{
    internal struct StateManagerValue
    {
        internal StateManagerMemberMetadata memberMetadata;
        internal object userObject;
        internal object originalValue;

        internal StateManagerValue(StateManagerMemberMetadata metadata, object instance, object value)
        {
            memberMetadata = metadata;
            userObject = instance;
            originalValue = value;
        }
    }
}