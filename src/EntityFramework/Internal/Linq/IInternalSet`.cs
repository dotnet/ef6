// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;

    // <summary>
    // An interface implemented by <see cref="InternalSet{TEntity}" />.
    // </summary>
    internal interface IInternalSet<TEntity> : IInternalSet, IInternalQuery<TEntity>
        where TEntity : class
    {
        TEntity Find(params object[] keyValues);

#if !NET40

        Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);

#endif

        TEntity Create();
        TEntity Create(Type derivedEntityType);
        ObservableCollection<TEntity> Local { get; }
    }
}
