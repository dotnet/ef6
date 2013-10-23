// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using VsErrorHandler = Microsoft.VisualStudio.ErrorHandler;

namespace Microsoft.Data.Entity.Design.VisualStudio.SingleFileGenerator
{
    using System;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using EnvDTE;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Designer.Interfaces;
    using Microsoft.VisualStudio.OLE.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     This class exists to be cocreated a in a preprocessor build step.
    /// </summary>
    [ComVisible(true)]
    public abstract class BaseCodeGeneratorWithSite : BaseCodeGenerator, IObjectWithSite
    {
        private object _site;
        private ServiceProvider _serviceProvider;
        private CodeDomProvider _codeDomProvider;
        private bool _disposeCodeDomProvider; // flag so we only dispose the CodeDomProvider if we constructed it.

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (_serviceProvider != null)
                {
                    _serviceProvider.Dispose();
                    _serviceProvider = null;
                }

                if (_disposeCodeDomProvider && _codeDomProvider != null)
                {
                    _codeDomProvider.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        ///     demand-creates a ServiceProvider given an IOleServiceProvider
        /// </summary>
        protected ServiceProvider SiteServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                {
                    var oleServiceProvider = _site as IOleServiceProvider;
                    Debug.Assert(oleServiceProvider != null, "Unable to get IOleServiceProvider from site object.");

                    _serviceProvider = new ServiceProvider(oleServiceProvider);
                }
                return _serviceProvider;
            }
        }

        /// <summary>
        ///     method to get a service by its Type
        /// </summary>
        /// <param name="serviceType">Type of service to retrieve</param>
        /// <returns>an object that implements the requested service</returns>
        protected object GetService(Type serviceType)
        {
            return SiteServiceProvider.GetService(serviceType);
        }

        /// <summary>
        ///     SetSite method of IOleObjectWithSite
        /// </summary>
        /// <param name="pUnkSite">site for this object to use</param>
        public virtual void SetSite(object pUnkSite)
        {
            _site = pUnkSite;
            _codeDomProvider = null;
            _disposeCodeDomProvider = false;
            _serviceProvider = null;
        }

        /// <summary>
        ///     GetSite method of IOleObjectWithSite
        /// </summary>
        /// <param name="riid">interface to get</param>
        /// <param name="ppvSite">array in which to stuff return value</param>
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        public virtual void GetSite(ref Guid riid, out IntPtr ppvSite)
        {
            if (_site == null)
            {
                throw new COMException(Strings.ObjectNotSited, VSConstants.E_FAIL);
            }

            var pUnknownPointer = Marshal.GetIUnknownForObject(_site);
            var intPointer = IntPtr.Zero;
            Marshal.QueryInterface(pUnknownPointer, ref riid, out intPointer);

            if (intPointer == IntPtr.Zero)
            {
                throw new COMException(Strings.SiteInterfaceNotSupported, VSConstants.E_NOINTERFACE);
            }
            ppvSite = intPointer;
        }

        /// <summary>
        ///     Returns a CodeDomProvider object for the language of the project containing
        ///     the project item the generator was called on
        /// </summary>
        /// <returns>A CodeDomProvider object</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected CodeDomProvider CodeProvider
        {
            get
            {
                if (null == _codeDomProvider)
                {
                    try
                    {
                        var sp = SiteServiceProvider;
                        Debug.Assert(sp != null, "provider should not be null");
                        if (null == sp)
                        {
                            return null;
                        }

                        var vsmdCodeDomProvider = sp.GetService(typeof(IVSMDCodeDomProvider)) as IVSMDCodeDomProvider;

                        // the vsmdCodeDomProvider will be null in some error situations (eg, if the user added an EDMX file to a web site, but didn't
                        // put it in App_Code.  So Don't assert here. 
                        if (null == vsmdCodeDomProvider)
                        {
                            return null;
                        }

                        _codeDomProvider = vsmdCodeDomProvider.CodeDomProvider as CodeDomProvider;
                    }
                    catch (Exception)
                    {
                        // in some cases, we will get an exception in web sites, but just eat the exception
                        // and return null; the user will then see an error message in the code file
                        _codeDomProvider = null;
                    }
                }

                return _codeDomProvider;
            }
        }

        /// <summary>
        ///     Returns the EnvDTE.ProjectItem object that corresponds to the project item the code
        ///     generator was called on
        /// </summary>
        /// <returns>The EnvDTE.ProjectItem of the project item the code generator was called on</returns>
        protected ProjectItem ProjectItem
        {
            get
            {
                var p = GetService(typeof(ProjectItem));
                Debug.Assert(p != null, "Unable to get Project Item.");
                return (ProjectItem)p;
            }
        }

        /// <summary>
        ///     Returns the EnvDTE.Project object of the project containing the project item the code
        ///     generator was called on
        /// </summary>
        /// <returns>
        ///     The EnvDTE.Project object of the project containing the project item the code generator was called on
        /// </returns>
        protected Project Project
        {
            get { return ProjectItem.ContainingProject; }
        }

        #region SVsErrorList Service

        /// <summary>
        ///     Get the Vs ErrorList object
        /// </summary>
        protected IVsErrorList ErrorList
        {
            get
            {
                IVsErrorList errorList = null;
                // Attempt to get site 
                var hierarchy = GetService(typeof(IVsHierarchy)) as IVsHierarchy;
                if (hierarchy != null)
                {
                    IOleServiceProvider sp = null;
                    var hresult = hierarchy.GetSite(out sp);
                    if (NativeMethods.Succeeded(hresult) && sp != null)
                    {
                        Debug.Assert(hresult == VSConstants.S_OK, "hresult = " + hresult + " should be " + VSConstants.S_OK);
                        var sid = typeof(SVsErrorList).GUID;
                        var iid = typeof(IVsErrorList).GUID;
                        var iunk = IntPtr.Zero;

                        try
                        {
                            VsErrorHandler.ThrowOnFailure(sp.QueryService(ref sid, ref iid, out iunk));
                            if (iunk != IntPtr.Zero)
                            {
                                errorList = Marshal.GetObjectForIUnknown(iunk) as IVsErrorList;
                            }
                        }
                        finally
                        {
                            if (iunk != IntPtr.Zero)
                            {
                                Marshal.Release(iunk);
                            }
                        }
                    }
                }
                return errorList;
            }
        }

        #endregion
    }
}
