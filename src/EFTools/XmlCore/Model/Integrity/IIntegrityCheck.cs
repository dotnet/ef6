// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Integrity
{
    /// <summary>
    ///     Defines the interface of an IntegrityCheck.  An IntegrityCheck can be registered
    ///     during a call in ModelController and it will run after the ModelController call
    ///     has completed.  This code can do work to make sure that our model stays correct.
    /// </summary>
    internal interface IIntegrityCheck
    {
        bool IsEqual(IIntegrityCheck otherCheck);
        void Invoke();
    }
}
