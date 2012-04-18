namespace System.Data.Entity
{
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    public static class PartialTrustHelpers
    {
        #region Execution in partial trust

        /// <summary>
        ///     Creates a new <see cref = "AppDomain" /> with medium trust permissions as used by ASP.NET which
        ///     can then be used to execute code to ensure that it works in a partial trust environment.
        /// </summary>
        public static AppDomain CreatePartialTrustSandbox(bool grantReflectionPermission = false, string configurationFile = null)
        {
            var securityConfig = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "CONFIG",
                                              "web_mediumtrust.config");
            var permissionXml = File.ReadAllText(securityConfig).Replace("$AppDir$", Environment.CurrentDirectory);

            // ASP.NET's configuration files still use the full policy levels rather than just permission sets,
            // so we can either write a lot of code to parse them ourselves, or we can use a deprecated API to
            // load them. This API used to throw an Exception in the .NET 4 development cycle, but ASP.NET themselves
            // use it to determine the grant set of the AppDomain, so this was changed to work before .NET 4 RTM.
#pragma warning disable 0618
            var grantSet =
                SecurityManager.LoadPolicyLevelFromString(permissionXml, PolicyLevelType.AppDomain).
                    GetNamedPermissionSet("ASP.Net");
#pragma warning restore 0618

            if (grantReflectionPermission)
            {
                grantSet.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
            }

            var info = new AppDomainSetup
                       {
                           ApplicationBase = Environment.CurrentDirectory,
                           PartialTrustVisibleAssemblies = new string[]
                                                           {
                                                               // Add conditional APTCA assemblies that you need to access in partial trust here.
                                                               // Do NOT add System.Web here since at least one test relies on it not being treated as conditionally APTCA.
                                                               "System.ComponentModel.DataAnnotations, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9"
                                                           }
                       };

            if (configurationFile != null)
            {
                info.ConfigurationFile = configurationFile;
            }

            return AppDomain.CreateDomain("Medium Trust Sandbox", null, info, grantSet, null);
        }

        #endregion
    }
}