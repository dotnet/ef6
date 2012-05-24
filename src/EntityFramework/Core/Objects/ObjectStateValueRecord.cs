namespace System.Data.Entity.Core.Objects
{
    internal enum ObjectStateValueRecord
    {
        OriginalReadonly = 0,
        CurrentUpdatable = 1,
        OriginalUpdatableInternal = 2,
        OriginalUpdatablePublic = 3,
    }
}
