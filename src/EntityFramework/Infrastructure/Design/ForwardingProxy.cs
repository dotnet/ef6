// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.Design
{
    using System.Data.Entity.Utilities;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;

    /// <summary>
    /// This is a small piece of Remoting magic. It enables us to invoke methods on a
    /// remote object without knowing its actual type. The only restriction is that the
    /// names and shapes of the types and their members must be the same on each side of
    /// the boundary.
    /// </summary>
    internal class ForwardingProxy<T> : RealProxy
    {
        private readonly MarshalByRefObject _target;

        public ForwardingProxy(object target)
            : base(typeof(T))
        {
            DebugCheck.NotNull(target);

            _target = (MarshalByRefObject)target;
        }

        /// <summary>
        /// Intercepts method invocations on the object represented by the current instance
        /// and forwards them to the target to finish processing.
        /// </summary>
        public override IMessage Invoke(IMessage msg)
        {
            DebugCheck.NotNull(msg);

            // NOTE: This sets the wrapped message's Uri
            new MethodCallMessageWrapper((IMethodCallMessage)msg).Uri = RemotingServices.GetObjectUri(_target);

            return RemotingServices.GetEnvoyChainForProxy(_target).SyncProcessMessage(msg);
        }

        public new T GetTransparentProxy()
        {
            return (T)base.GetTransparentProxy();
        }
    }
}
