// Copyright (c) Microsoft Corporation.  All rights reserved.
using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestTools.Vsip;
using Microsoft.Win32;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    [ComVisible(true)]
    [Guid("F4ABA8B2-0798-4e7d-827D-6D171283CB37")]
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Vs", Justification = "Public interface, cannot rename")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Addin", Justification = "Public interface, cannot rename")]
    public interface IVsIdeTestHostAddin
    {
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // This is not a simle property and does things behind the scene.
        HostAdapterHostSide GetHostSide();
    }

    /// <summary>
    /// The object implementing VS Add-in.
    /// </summary>
    /// <seealso class='IDTExtensibility2' />
    [ComVisible(true)]
    [Guid("F4ABA8B2-0798-4e7d-827D-6D171283CB38")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]  // Lifetime is managed by VS Addin manager, so don't do IDisposable.
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Vs", Justification = "Public class, cannot rename")]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Addin", Justification = "Public class, cannot rename")]
    public sealed class VsIdeTestHostAddin : MarshalByRefObject, IDTExtensibility2, IVsIdeTestHostAddin
    {
        private DTE2 m_applicationObject;
        private IChannel m_serverChannel;
        private IChannel m_clientChannel;
        private HostAdapterHostSide m_hostSide;
        private ServiceProvider m_serviceProvider;
        private VsIdeHostDebugger m_debugger;
        private object m_hostLock = new object();
        private ManualResetEvent m_hostInitializedEvent = new ManualResetEvent(false);
        private bool m_hostInitialized; // Whether VS we use to run the tests is initialized; static in case if VS creaes 2 instances.

        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public VsIdeTestHostAddin()
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#")]
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            Trace.WriteLine(string.Format(
                CultureInfo.InvariantCulture,
                "[pid={0,4}, tid={1,2}, {2:yyyy}/{2:MM}/{2:dd} {2:HH}:{2:mm}:{2:ss}.{2:fff}] VsIdeTestHostAddin.OnConnection", 
                System.Diagnostics.Process.GetCurrentProcess().Id, 
                System.Threading.Thread.CurrentThread.ManagedThreadId, 
                DateTime.Now));
        
            if (RegistrySettings.LookupRegistrySetting<int>("EnableDebugBreak", 0) == 1)
            {
                System.Diagnostics.Debugger.Break();
            }

            // The idea about connection modes is to make sure we are initialized
            // after 1st OnConnection is called. Because this is when VS thinks that
            // addin is ready and returns it to outside.
            if (connectMode == ext_ConnectMode.ext_cm_UISetup ||    // When VS sets up UI for Addin.
                connectMode == ext_ConnectMode.ext_cm_Startup ||    // When VS is started.
                connectMode == ext_ConnectMode.ext_cm_AfterStartup) // When loading from Tools->Addin Manager.
            {
                try
                {
                    lock (m_hostLock)   // Protect from calling with different modes at the same time.
                    {
                        DTE2 dteApplication = (DTE2)application;
                        Debug.Assert(dteApplication != null, "OnConnect: (DTE2)application = null!");

                        if (!m_hostInitialized) // When loading from Tools->Addin Manager.
                        {
                            m_applicationObject = dteApplication;

                            SetupChannels();

                            InitHostSide();

                            m_hostInitialized = true;
                            m_hostInitializedEvent.Set();
                        }
                    }

                    // Marshal the object to serve debug requests from the adapter.
                    // This does not have to be done inside the lock.
                    if (m_debugger == null)
                    {
                        m_debugger = new VsIdeHostDebugger(m_applicationObject.DTE);
                        RemotingServices.Marshal(m_debugger, VsIdeHostSession.RemoteObjectName);
                    }
                }
                catch (Exception ex)
                {
                    Debug.Fail("Addin.OnConnection: " + ex.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// This can be called from OnConnect by Addin AND from GetHostSide by HA.
        /// </summary>
        private void InitHostSide()
        {
            try
            {
                lock (m_hostLock)
                {
                    if (m_hostSide == null)
                    {
                        Debug.Assert(m_applicationObject != null, "HostSide.InitHostSide: m_applicationObject is null!");

                        m_serviceProvider = new ServiceProvider((IOleServiceProvider)m_applicationObject);
                        Debug.Assert(m_serviceProvider != null, "VsIdeTestHostAddin.InitHostSide: failed to init service provider!");

                        m_hostSide = new HostAdapterHostSide(m_serviceProvider);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Fail("HA.InitHostSide: " + ex.ToString());
                throw;
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            RemotingServices.Disconnect(m_debugger);
            CleanupChannels();
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // This is not a simle property and does things behind the scene.
        HostAdapterHostSide IVsIdeTestHostAddin.GetHostSide()
        {
            // Wait for OnConnection to initialize the addin.
            m_hostInitializedEvent.WaitOne(TimeSpan.FromMinutes(1), false);

            lock (m_hostLock)
            {
                Debug.Assert(m_hostInitialized, "Addin.GetHostSide: m_hostInitialized is false!");

                InitHostSide();
                Debug.Assert(m_hostSide != null, "m_hostSide is null!");
            }

            return m_hostSide;
        }

        private void SetupChannels()
        {
            lock (m_hostLock)
            {
                // If channels are not set up yet, set them up.
                if (m_serverChannel == null)
                {
                    Debug.Assert(m_clientChannel == null);

                    // This channel is used for debugging session, when TA connects to authoring vs instance.
                    BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                    serverProvider.TypeFilterLevel = TypeFilterLevel.Full;  // Enable remoting objects as arguments.
                    Hashtable props = new Hashtable();
                    string serverChannelName = VsIdeHostSession.Id;         // Note that HA needs this to be session id.
                    props["name"] = serverChannelName;
                    props["portName"] = serverChannelName;           // Must be different from client's port.
                    // Default IpcChannel security is: allow for all users who can authorize on this machine.
                    props["authorizedGroup"] = WindowsIdentity.GetCurrent().Name;
                    m_serverChannel = new IpcServerChannel(props, serverProvider);
                    ChannelServices.RegisterChannel(m_serverChannel, false);

                    // This channel is used for connecting to both authoring and host side VS.
                    m_clientChannel = new IpcClientChannel(VsIdeHostSession.Id + "_ClientChannel", new BinaryClientFormatterSinkProvider());
                    ChannelServices.RegisterChannel(m_clientChannel, false);
                }
            }
        }

        private void CleanupChannels()
        {
            lock (m_hostLock)
            {
                if (m_serverChannel != null)
                {
                    ChannelServices.UnregisterChannel(m_serverChannel);
                    m_serverChannel = null;
                }

                if (m_clientChannel != null)
                {
                    ChannelServices.UnregisterChannel(m_clientChannel);
                    m_clientChannel = null;
                }
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
