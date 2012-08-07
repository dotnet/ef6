// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    /// <summary>
    ///     Flags used to modify behavior of ObjectContext.SaveChanges()
    /// </summary>
    [Flags]
    public enum SaveOptions
    {
        None = 0,
        AcceptAllChangesAfterSave = 1,
        DetectChangesBeforeSave = 2
    }
}
