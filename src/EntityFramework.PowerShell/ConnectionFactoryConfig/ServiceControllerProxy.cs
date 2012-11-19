// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Data.Entity.Utilities;
    using System.ServiceProcess;

    /// <summary>
    ///     Acts as a thin wrapper around a <see cref="ServiceController" /> instance such that uses of
    ///     the ServiceController can be mocked.
    /// </summary>
    internal class ServiceControllerProxy : IDisposable
    {
        private readonly ServiceController _controller;

        /// <summary>
        ///     For mocking.
        /// </summary>
        protected ServiceControllerProxy()
        {
        }

        /// <summary>
        ///     Constructs a proxy around a real <see cref="ServiceController" />.
        /// </summary>
        public ServiceControllerProxy(ServiceController controller)
        {
            DebugCheck.NotNull(controller);

            _controller = controller;
        }

        public virtual ServiceControllerStatus Status
        {
            get { return _controller.Status; }
        }

        public virtual void Dispose()
        {
            _controller.Dispose();
        }
    }
}
