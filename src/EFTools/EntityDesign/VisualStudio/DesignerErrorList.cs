// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio
{
    using System;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    ///     Wrapper around ErrorListProvider
    /// </summary>
    internal class DesignerErrorList : IDisposable
    {
        private readonly ErrorListProvider _provider;

        public DesignerErrorList(IServiceProvider provider)
        {
            _provider = new ErrorListProvider(provider);
        }

        ~DesignerErrorList()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_provider != null)
                {
                    _provider.Dispose();
                }
            }
        }

        public ErrorListProvider Provider
        {
            get { return _provider; }
        }

        public void Clear()
        {
            _provider.Tasks.Clear();
        }

        public void AddItem(ErrorTask error)
        {
            _provider.Tasks.Add(error);
        }
    }
}
