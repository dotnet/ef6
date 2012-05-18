namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Security;

    internal class AspProxy
    {
        private const string BUILD_MANAGER_TYPE_NAME = @"System.Web.Compilation.BuildManager";
        private const string AspNetAssemblyName = "System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        private Assembly _webAssembly;
        private bool _triedLoadingWebAssembly;

        /// <summary>
        /// Determine whether we are inside an ASP.NET application.
        /// </summary>
        /// <param name="webAssembly">The System.Web assembly</param>
        /// <returns>true if we are running inside an ASP.NET application</returns>
        internal bool IsAspNetEnvironment()
        {
            if (!TryInitializeWebAssembly())
            {
                return false;
            }

            try
            {
                var result = PrivateMapWebPath(EdmConstants.WebHomeSymbol);
                return result != null;
            }
            catch (SecurityException)
            {
                // When running under partial trust but not running as an ASP.NET site the System.Web assembly
                // may not be not treated as conditionally APTCA and hence throws a security exception. However,
                // since this happens when not running as an ASP.NET site we can just return false because we're
                // not in an ASP.NET environment.
                return false;
            }
            catch (Exception e)
            {
                if (e.IsCatchableExceptionType())
                {
                    return false;
                }

                throw;
            }
        }

        private bool TryInitializeWebAssembly()
        {
            // We cannot introduce a hard dependency on the System.Web assembly, so we load
            // it via reflection.
            //
            if (_webAssembly != null)
            {
                return true;
            }
            else if (_triedLoadingWebAssembly)
            {
                return false;
            }

            Debug.Assert(_triedLoadingWebAssembly == false);
            Debug.Assert(_webAssembly == null);
            _triedLoadingWebAssembly = true;
            try
            {
                _webAssembly = Assembly.Load(AspNetAssemblyName);
                return _webAssembly != null;
            }
            catch (Exception e)
            {
                if (!e.IsCatchableExceptionType())
                {
                    throw; // StackOverflow, OutOfMemory, ...
                }

                // It is possible that we are operating in an environment where
                // System.Web is simply not available (for instance, inside SQL
                // Server). Instead of throwing or rethrowing, we simply fail
                // gracefully
            }

            return false;
        }

        private void InitializeWebAssembly()
        {
            if (!TryInitializeWebAssembly())
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext);
            }
        }

        /// <summary>
        /// This method accepts a string parameter that represents a path in a Web (specifically,
        /// an ASP.NET) application -- one that starts with a '~' -- and resolves it to a 
        /// canonical file path.
        /// </summary>
        /// <remarks>
        /// The implementation assumes that you cannot have file names that begin with the '~'
        /// character. (This is a pretty reasonable assumption.) Additionally, the method does not
        /// test for the existence of a directory or file resource after resolving the path.
        /// CONSIDER: Caching the reflection results to satisfy subsequent path resolution requests.
        /// ISSUE: Need to maintain context for a set of path resolution requests, so that we
        /// don't run into a situation where an incorrect context is applied to a path resolution
        /// request.
        /// </remarks>
        /// <param name="path">A path in an ASP.NET application</param>
        /// <returns>A fully-qualified path</returns>
        internal string MapWebPath(string path)
        {
            Debug.Assert(path != null, "path == null");

            path = PrivateMapWebPath(path);
            if (path == null)
            {
                var errMsg = Strings.InvalidUseOfWebPath(EdmConstants.WebHomeSymbol);
                throw new InvalidOperationException(errMsg);
            }
            return path;
        }

        private string PrivateMapWebPath(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            Debug.Assert(path.StartsWith(EdmConstants.WebHomeSymbol, StringComparison.Ordinal));

            InitializeWebAssembly();
            // Each managed application domain contains a static instance of the HostingEnvironment class, which 
            // provides access to application-management functions and application services. We'll try to invoke
            // the static method MapPath() on that object.
            //
            try
            {
                var hostingEnvType = _webAssembly.GetType("System.Web.Hosting.HostingEnvironment", true);

                var miMapPath = hostingEnvType.GetMethod("MapPath");
                Debug.Assert(miMapPath != null, "Unpexpected missing member in type System.Web.Hosting.HostingEnvironment");

                // Note:
                //   1. If path is null, then the MapPath() method returns the full physical path to the directory 
                //      containing the current application.
                //   2. Any attempt to navigate out of the application directory (using "../..") will generate
                //      a (wrapped) System.Web.HttpException under ASP.NET (which we catch and re-throw).
                //
                return (string)miMapPath.Invoke(null, new object[] { path });
            }
            catch (TargetException e)
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, e);
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, e);
            }
            catch (TargetInvocationException e)
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, e);
            }
            catch (TargetParameterCountException e)
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, e);
            }
            catch (MethodAccessException e)
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, e);
            }
            catch (MemberAccessException e)
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, e);
            }
            catch (TypeLoadException e)
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, e);
            }
        }

        internal bool HasBuildManagerType()
        {
            Type buildManager;
            return TryGetBuildManagerType(out buildManager);
        }

        private bool TryGetBuildManagerType(out Type buildManager)
        {
            InitializeWebAssembly();
            buildManager = _webAssembly.GetType(BUILD_MANAGER_TYPE_NAME, false);
            return buildManager != null;
        }

        internal IEnumerable<Assembly> GetBuildManagerReferencedAssemblies()
        {
            // We are interested in invoking the following method on the class
            // System.Web.Compilation.BuildManager, which is available only in Orcas:
            //
            //    public static ICollection GetReferencedAssemblies();
            //
            Type buildManager;
            if (!TryGetBuildManagerType(out buildManager))
            {
                throw new InvalidOperationException(Strings.UnableToFindReflectedType(BUILD_MANAGER_TYPE_NAME, AspNetAssemblyName));
            }

            var getRefAssembliesMethod = buildManager.GetMethod(
                @"GetReferencedAssemblies",
                BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public
                );

            if (getRefAssembliesMethod == null)
            {
                // eat this problem
                return new List<Assembly>();
            }

            ICollection referencedAssemblies = null;
            try
            {
                referencedAssemblies = (ICollection)getRefAssembliesMethod.Invoke(null, null);
                if (referencedAssemblies == null)
                {
                    return new List<Assembly>();
                }
                return referencedAssemblies.Cast<Assembly>();
            }
            catch (TargetException e)
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, e);
            }
            catch (TargetInvocationException e)
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, e);
            }
            catch (MethodAccessException e)
            {
                throw new InvalidOperationException(Strings.UnableToDetermineApplicationContext, e);
            }
        }
    }
}
