// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Providers
{
    using System;
    using System.Diagnostics;
    using Microsoft.VisualStudio.Shell.Interop;

    internal sealed class ParentServiceProvider
    {
        private ParentServiceProvider()
        {
        }

        /// <summary>
        ///     Helper method that locates a service from our parent frame.
        ///     This can return null if the service doesn't exist or if the
        ///     parent frame doesn't exist.
        /// </summary>
        /// <typeparam name="ServiceType"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        internal static ServiceType GetParentService<ServiceType>(IServiceProvider provider)
        {
            object service = null;
            object @var;

            Debug.Assert(null != provider);
            if (null != provider)
            {
                var ourFrame = provider.GetService(typeof(IVsWindowFrame)) as IVsWindowFrame;
                if (ourFrame != null)
                {
                    var hr = ourFrame.GetProperty((int)__VSFPROPID2.VSFPROPID_ParentFrame, out @var);

                    if (NativeMethods.Succeeded(hr)
                        && @var != null)
                    {
                        var parentFrame = (IVsWindowFrame)@var;
                        hr = parentFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out @var);
                        if (NativeMethods.Succeeded(hr))
                        {
                            var parentViewProvider = @var as IServiceProvider;
                            if (parentViewProvider != null)
                            {
                                service = parentViewProvider.GetService(typeof(ServiceType));
                            }
                        }
                    }
                }
            }
            return (ServiceType)service;
        }
    }
}
