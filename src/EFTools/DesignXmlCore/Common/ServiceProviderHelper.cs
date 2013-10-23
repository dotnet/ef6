// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Ole = Microsoft.VisualStudio.OLE.Interop;

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.VisualStudio.Data.Tools.Design.XmlCore;

    /// <summary>
    ///     Represents a dynamic, interoperable service provider.
    /// </summary>
    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential)] // because it is COM visible
    [SuppressMessage("Embeddable Types Rule", "NoPIATypeEq03:FlagServiceProviders",
        MessageId = "Microsoft.VisualStudio.Data.Tools.Package.SharedUtilities.ServiceProviderHelper")]
    internal sealed class ServiceProviderHelper : IServiceContainer, IServiceProvider, Ole.IServiceProvider
    {
        #region Public Constructors

        /// <summary>
        ///     Constructor for a new service provider.
        /// </summary>
        public ServiceProviderHelper()
        {
        }

        /// <summary>
        ///     Constructor for a service provider that wraps and/or extends an
        ///     existing managed service provider.
        /// </summary>
        /// <param name="parentProvider">An existing service provider.</param>
        public ServiceProviderHelper(IServiceProvider parentProvider)
        {
            if (parentProvider == null)
            {
                throw new ArgumentNullException("parentProvider");
            }
            _parentProvider = parentProvider;
        }

        /// <summary>
        ///     Constructor for a service provider that wraps and/or extends an
        ///     existing OLE service provider.
        /// </summary>
        /// <param name="serviceProvider">An existing service provider.</param>
        public ServiceProviderHelper(Ole.IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            _parentProvider = serviceProvider;
        }

        #endregion

        #region Public Methods

        public object GetService(Guid serviceGuid)
        {
            object service = null;

            // Try to derive a type from the service GUID
            var serviceType = GetServiceType(serviceGuid);
            if (serviceType != null)
            {
                // Service type found; delegate to the standard GetService
                service = GetService(serviceType);
            }

            // If service is still null, then if parent is OLE service provider, delegate to it
            if (service == null
                && _parentProvider is Ole.IServiceProvider)
            {
                service = QueryService(serviceGuid);
            }

            return service;
        }

        /// <summary>
        ///     This flag controls whether or not this service provider responds to
        ///     queries of IServiceProvider and IObjectWithSite by returning itself.  If
        ///     this is set to true then this service  provider will
        ///     respond to Microsoft.VisualStudio.OLE.Interop.IServiceProvider and IObjectWithSite
        ///     as services.  A query for Microsoft.VisualStudio.OLE.Interop.IServiceProvider will
        ///     return the underlying COM service provider and a query for IObjectWithSite will
        ///     return this object.  If false is passed into defaultServices these two services
        ///     will not be provided and the service provider will be "transparent".
        ///     This is in accordance with the service provider here:
        ///     VSSDK\VSIntegration\Common\Source\CSharp\Shell\ServiceProvider.cs
        /// </summary>
        public bool DefaultServices
        {
            get { return _defaultServices; }
            set { _defaultServices = value; }
        }

        #endregion

        #region RespondWithDefaultService

        /// <summary>
        ///     Returns services if our default services flag is on
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        private object RespondWithDefaultService(Type serviceType)
        {
            if (_defaultServices)
            {
                if (serviceType.IsEquivalentTo(typeof(Ole.IServiceProvider)))
                {
                    return ParentProvider;
                }
                else if (serviceType.IsEquivalentTo(typeof(Ole.IObjectWithSite)))
                {
                    return this;
                }
            }
            return null;
        }

        #endregion

        #region IServiceContainer Members

        public void AddService(Type serviceType, ServiceCreatorCallback callback)
        {
            AddService(serviceType, callback, false);
        }

        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            if (promote && ParentContainer != null)
            {
                ParentContainer.AddService(serviceType, callback, promote);
                return;
            }

            lock (this)
            {
                if (Services.ContainsKey(serviceType))
                {
                    throw new InvalidOperationException(
                        CommonResourceUtil.GetString(Resources.ServiceProvider_ServiceAlreadyExists, serviceType.FullName));
                }
                Services.Add(serviceType, callback);
            }
        }

        public void AddService(Type serviceType, object serviceInstance)
        {
            AddService(serviceType, serviceInstance, false);
        }

        public void AddService(Type serviceType, object serviceInstance, bool promote)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            if (serviceInstance == null)
            {
                throw new ArgumentNullException("serviceInstance");
            }
            if (!(serviceInstance is ServiceCreatorCallback)
                &&
                !serviceType.IsInstanceOfType(serviceInstance))
            {
                throw new ArgumentException(
                    CommonResourceUtil.GetString(Resources.ServiceProvider_InvalidServiceInstance, serviceType.FullName, "serviceInstance"));
            }

            if (promote && ParentContainer != null)
            {
                ParentContainer.AddService(serviceType, serviceInstance, promote);
                return;
            }

            lock (this)
            {
                if (Services.ContainsKey(serviceType))
                {
                    throw new InvalidOperationException(
                        CommonResourceUtil.GetString(Resources.ServiceProvider_ServiceAlreadyExists, serviceType.FullName));
                }
                Services.Add(serviceType, serviceInstance);
            }
        }

        public void RemoveService(Type serviceType)
        {
            RemoveService(serviceType, false);
        }

        public void RemoveService(Type serviceType, bool promote)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            if (promote && ParentContainer != null)
            {
                ParentContainer.RemoveService(serviceType, promote);
                return;
            }

            lock (this)
            {
                Services.Remove(serviceType);
            }
        }

        #endregion

        public T GetService<T>()
        {
            return GetService<T, T>();
        }

        public T GetService<T, S>()
        {
            return (T)GetService(typeof(S));
        }

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            object service = null;

            // If default services is true then respond to IServiceProvider and
            // IObjectWithSite queries ourselves
            service = RespondWithDefaultService(serviceType);

            // If service is still null, try our dictionary
            if (service == null)
            {
                lock (this)
                {
                    // Check if our dictionary contains the service type
                    if (Services.ContainsKey(serviceType))
                    {
                        // Get the service object associated with the type
                        service = Services[serviceType];

                        // If the service is a ServiceCreatorCallback, then we need to create it
                        var serviceCallback = service as ServiceCreatorCallback;
                        if (serviceCallback != null)
                        {
                            // Save the created object as the service object
                            service = Services[serviceType] = serviceCallback(this, serviceType);
                        }
                    }
                }
            }

            // If service is still null, then if parent is managed service provider, delegate to it
            if (service == null
                && _parentProvider is IServiceProvider)
            {
                service = (_parentProvider as IServiceProvider).GetService(serviceType);
            }

            // If service is still null, then if parent container is valid, delegate to it
            if (service == null
                && ParentContainer != null
                && ParentContainer != _parentProvider)
            {
                service = ParentContainer.GetService(serviceType);
            }

            // If service is still null, then if parent is OLE service provider
            // and a GUID can be retrieved from the service type, delegate to it
            if (service == null
                && _parentProvider is Ole.IServiceProvider
                && serviceType != null)
            {
                service = QueryService(serviceType.GUID);
            }

            return service;
        }

        #endregion

        #region Ole.IServiceProvider Members

        int Ole.IServiceProvider.QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject)
        {
            ppvObject = IntPtr.Zero;

            object service = null;

            // Try to derive a type from the service GUID
            var serviceType = GetServiceType(guidService);
            if (serviceType != null)
            {
                // Service type found; delegate to the standard GetService
                service = GetService(serviceType);
            }

            // If we have a service, marshal it into the output parameter
            if (service != null)
            {
                var punk = Marshal.GetIUnknownForObject(service);
                var hr = Marshal.QueryInterface(punk, ref riid, out ppvObject);
                Marshal.Release(punk); // because GetIUnknownForObject AddRefs
                return hr;
            }

            // If parent is an OLE service provider, delegate to it
            var parentOleProvider = _parentProvider as Ole.IServiceProvider;
            if (parentOleProvider != null)
            {
                return parentOleProvider.QueryService(ref guidService, ref riid, out ppvObject);
            }

            return NativeMethods.SVC_E_UNKNOWNSERVICE;
        }

        #endregion

        #region Protected Properties

        private object ParentProvider
        {
            get { return _parentProvider; }
        }

        #endregion

        #region Private Properties

        private IDictionary<Type, object> Services
        {
            get
            {
                if (_services == null)
                {
                    lock (this)
                    {
                        if (_services == null)
                        {
                            _services = new TypeKeyedDictionary<object>();
                        }
                    }
                }
                return _services;
            }
        }

        private IServiceContainer ParentContainer
        {
            get
            {
                // The parent container may be implemented directly by the parent
                // or it may be supplied as a service off the parent; try both
                var parentContainer = _parentProvider as IServiceContainer;
                if (parentContainer == null)
                {
                    var serviceProvider = _parentProvider as IServiceProvider;
                    if (serviceProvider != null)
                    {
                        parentContainer = serviceProvider.GetService(typeof(IServiceContainer)) as IServiceContainer;
                    }
                }
                return parentContainer;
            }
        }

        #endregion

        #region Private Methods

        private Type GetServiceType(Guid serviceGuid)
        {
            Type serviceType = null;

            lock (this)
            {
                // Search this service provider and all parent service providers where possible
                var currentProvider = this;
                do
                {
                    // Look through all the service keys and compare type GUIDs
                    foreach (var type in currentProvider.Services.Keys)
                    {
                        if (type.GUID == serviceGuid)
                        {
                            serviceType = type;
                            break;
                        }
                    }
                    if (serviceType != null)
                    {
                        // Found it
                        break;
                    }

                    // To get the parent provider, we look at both the parent
                    // provider instance and the parent container instance
                    var nextProvider = currentProvider._parentProvider as ServiceProviderHelper;
                    if (nextProvider == null)
                    {
                        nextProvider = currentProvider.ParentContainer as ServiceProviderHelper;
                    }
                    currentProvider = nextProvider;
                }
                while (currentProvider != null);
            }

            return serviceType;
        }

        private object QueryService(Guid serviceGuid)
        {
            var parentOleProvider = _parentProvider as Ole.IServiceProvider;
            Debug.Assert(parentOleProvider != null);

            object service = null;

            var pvObject = IntPtr.Zero;
            var hr = parentOleProvider.QueryService(ref serviceGuid, ref NativeMethods.IID_IUnknown, out pvObject);
            if (NativeMethods.Failed(hr)
                &&
                hr != (NativeMethods.SVC_E_UNKNOWNSERVICE)
                &&
                hr != NativeMethods.E_NOINTERFACE)
            {
                if (pvObject != IntPtr.Zero)
                {
                    Marshal.Release(pvObject);
                }
                throw Marshal.GetExceptionForHR(hr);
            }

            if (pvObject != IntPtr.Zero)
            {
                service = Marshal.GetObjectForIUnknown(pvObject);
                Marshal.Release(pvObject); // because GetObjectForIUnknown AddRefs
            }

            return service;
        }

        #endregion

        #region Private Fields

        private IList<Type> _serviceTypes;
        private IDictionary<Type, object> _services;
        private readonly object _parentProvider;
        private bool _defaultServices;

        #endregion
    }
}
