namespace System.Data.Entity.Core.Objects
{
    /// <summary>
    /// Flags used to modify behavior of ObjectContext.SaveChanges()
    /// </summary>
    [Flags]
    public enum SaveOptions
    {
        None = 0,
        AcceptAllChangesAfterSave = 1,
        DetectChangesBeforeSave = 2
    }
}
