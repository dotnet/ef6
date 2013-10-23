// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    internal partial class ScalarProperty
    {
        internal void ChangeEntityKey()
        {
            using (var t = Store.TransactionManager.BeginTransaction("Entity Key"))
            {
                EntityKey = !EntityKey;
                t.Commit();
            }
        }
    }
}
