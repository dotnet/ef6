// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Resources
{
    using System;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.Resources;
    using System.Threading;

    /// <summary>
    ///    Strongly-typed and parameterized string resources.
    /// </summary>
    [GeneratedCode("Resources.tt", "1.0.0.0")]
    internal static class Strings
    {
        /// <summary>
        /// A string like "An error occurred while adding custom templates. See the Output window for details."
        /// </summary>
        internal static string AddTemplatesError
        {
            get { return EntityRes.GetString(EntityRes.AddTemplatesError); }
        }

        /// <summary>
        /// A string like "The argument '{0}' cannot be null, empty or contain only white space."
        /// </summary>
        internal static string ArgumentIsNullOrWhitespace(object p0)
        {
            return EntityRes.GetString(EntityRes.ArgumentIsNullOrWhitespace, p0);
        }

        /// <summary>
        /// A string like "Build failed. Unable to discover a DbContext class."
        /// </summary>
        internal static string BuildFailed
        {
            get { return EntityRes.GetString(EntityRes.BuildFailed); }
        }

        /// <summary>
        /// A string like "An error occurred while trying to instantiate the DbContext type {0}. See the Output window for details."
        /// </summary>
        internal static string CreateContextFailed(object p0)
        {
            return EntityRes.GetString(EntityRes.CreateContextFailed, p0);
        }

        /// <summary>
        /// A string like "The Entity Data Model '{0}' has one or more schema errors inside the {1} section."
        /// </summary>
        internal static string EdmSchemaError(object p0, object p1)
        {
            return EntityRes.GetString(EntityRes.EdmSchemaError, p0, p1);
        }

        /// <summary>
        /// A string like "An error occurred while trying to load the configuration file. See the Output window for details."
        /// </summary>
        internal static string LoadConfigFailed
        {
            get { return EntityRes.GetString(EntityRes.LoadConfigFailed); }
        }

        /// <summary>
        /// A string like "A constructible type deriving from DbContext could not be found in the selected file."
        /// </summary>
        internal static string NoContext
        {
            get { return EntityRes.GetString(EntityRes.NoContext); }
        }

        /// <summary>
        /// A string like "Performing EDM view pre-compilation for: {0}. This operation may take several minutes."
        /// </summary>
        internal static string Optimize_Begin(object p0)
        {
            return EntityRes.GetString(EntityRes.Optimize_Begin, p0);
        }

        /// <summary>
        /// A string like "An error occurred while trying to initialize view generation for the DbContext type {0}. See the Output window for details."
        /// </summary>
        internal static string Optimize_ContextError(object p0)
        {
            return EntityRes.GetString(EntityRes.Optimize_ContextError, p0);
        }

        /// <summary>
        /// A string like "An error occurred while trying to initialize view generation for the Entity Data Model {0}. See the Output window for details."
        /// </summary>
        internal static string Optimize_EdmxError(object p0)
        {
            return EntityRes.GetString(EntityRes.Optimize_EdmxError, p0);
        }

        /// <summary>
        /// A string like "Finished EDM view pre-compilation for: {0}! See file: {1}."
        /// </summary>
        internal static string Optimize_End(object p0, object p1)
        {
            return EntityRes.GetString(EntityRes.Optimize_End, p0, p1);
        }

        /// <summary>
        /// A string like "An error occurred while trying to generate views for {0}. See the Output window for details."
        /// </summary>
        internal static string Optimize_Error(object p0)
        {
            return EntityRes.GetString(EntityRes.Optimize_Error, p0);
        }

        /// <summary>
        /// A string like "An error occurred while trying to generate views for {0}."
        /// </summary>
        internal static string Optimize_SchemaError(object p0)
        {
            return EntityRes.GetString(EntityRes.Optimize_SchemaError, p0);
        }

        /// <summary>
        /// A string like "The precondition '{0}' failed. {1}"
        /// </summary>
        internal static string PreconditionFailed(object p0, object p1)
        {
            return EntityRes.GetString(EntityRes.PreconditionFailed, p0, p1);
        }

        /// <summary>
        /// A string like "One or more errors occurred while processing template '{0}'."
        /// </summary>
        internal static string ProcessTemplateError(object p0)
        {
            return EntityRes.GetString(EntityRes.ProcessTemplateError, p0);
        }

        /// <summary>
        /// A string like "Reverse engineer complete."
        /// </summary>
        internal static string ReverseEngineer_Complete
        {
            get { return EntityRes.GetString(EntityRes.ReverseEngineer_Complete); }
        }

        /// <summary>
        /// A string like "An error occurred while reverse engineering Code First. See the Output window for details."
        /// </summary>
        internal static string ReverseEngineer_Error
        {
            get { return EntityRes.GetString(EntityRes.ReverseEngineer_Error); }
        }

        /// <summary>
        /// A string like "Generating entity and mapping classes..."
        /// </summary>
        internal static string ReverseEngineer_GenerateClasses
        {
            get { return EntityRes.GetString(EntityRes.ReverseEngineer_GenerateClasses); }
        }

        /// <summary>
        /// A string like "Generating context..."
        /// </summary>
        internal static string ReverseEngineer_GenerateContext
        {
            get { return EntityRes.GetString(EntityRes.ReverseEngineer_GenerateContext); }
        }

        /// <summary>
        /// A string like "Generating default mapping..."
        /// </summary>
        internal static string ReverseEngineer_GenerateMapping
        {
            get { return EntityRes.GetString(EntityRes.ReverseEngineer_GenerateMapping); }
        }

        /// <summary>
        /// A string like "Installing EntityFramework package..."
        /// </summary>
        internal static string ReverseEngineer_InstallEntityFramework
        {
            get { return EntityRes.GetString(EntityRes.ReverseEngineer_InstallEntityFramework); }
        }

        /// <summary>
        /// A string like "An error occurred while trying to install the EntityFramework package. See the Output window for details."
        /// </summary>
        internal static string ReverseEngineer_InstallEntityFrameworkError
        {
            get { return EntityRes.GetString(EntityRes.ReverseEngineer_InstallEntityFrameworkError); }
        }

        /// <summary>
        /// A string like "Loading schema information..."
        /// </summary>
        internal static string ReverseEngineer_LoadSchema
        {
            get { return EntityRes.GetString(EntityRes.ReverseEngineer_LoadSchema); }
        }

        /// <summary>
        /// A string like "One or more errors occurred while loading schema information."
        /// </summary>
        internal static string ReverseEngineer_SchemaError
        {
            get { return EntityRes.GetString(EntityRes.ReverseEngineer_SchemaError); }
        }

        /// <summary>
        /// A string like "Cannot find processor for directive '{0}'."
        /// </summary>
        internal static string UnknownDirectiveProcessor(object p0)
        {
            return EntityRes.GetString(EntityRes.UnknownDirectiveProcessor, p0);
        }

        /// <summary>
        /// A string like "You are using a version of the Entity Framework that is not supported by the Power Tools. Please upgrade to Entity Framework 4.2 or later."
        /// </summary>
        internal static string UnsupportedVersion
        {
            get { return EntityRes.GetString(EntityRes.UnsupportedVersion); }
        }

        /// <summary>
        /// A string like "An error occurred while trying to build the model for {0}. See the Output window for details."
        /// </summary>
        internal static string ViewContextError(object p0)
        {
            return EntityRes.GetString(EntityRes.ViewContextError, p0);
        }

        /// <summary>
        /// A string like "An error occurred while trying to build the model for {0}. See the Output window for details."
        /// </summary>
        internal static string ViewDdlError(object p0)
        {
            return EntityRes.GetString(EntityRes.ViewDdlError, p0);
        }
    }

    /// <summary>
    ///    Strongly-typed and parameterized exception factory.
    /// </summary>
    [GeneratedCode("Resources.tt", "1.0.0.0")]
    internal static class Error
    {
        /// <summary>
        /// ArgumentException with message like "The argument '{0}' cannot be null, empty or contain only white space."
        /// </summary>
        internal static Exception ArgumentIsNullOrWhitespace(object p0)
        {
            return new ArgumentException(Strings.ArgumentIsNullOrWhitespace(p0));
        }

        /// <summary>
        /// ArgumentException with message like "The precondition '{0}' failed. {1}"
        /// </summary>
        internal static Exception PreconditionFailed(object p0, object p1)
        {
            return new ArgumentException(Strings.PreconditionFailed(p0, p1));
        }

        /// <summary>
        /// InvalidOperationException with message like "Cannot find processor for directive '{0}'."
        /// </summary>
        internal static Exception UnknownDirectiveProcessor(object p0)
        {
            return new InvalidOperationException(Strings.UnknownDirectiveProcessor(p0));
        }

        /// <summary>
        /// InvalidOperationException with message like "You are using a version of the Entity Framework that is not supported by the Power Tools. Please upgrade to Entity Framework 4.2 or later."
        /// </summary>
        internal static Exception UnsupportedVersion()
        {
            return new InvalidOperationException(Strings.UnsupportedVersion);
        }
        /// <summary>
        /// The exception that is thrown when a null reference (Nothing in Visual Basic) is passed to a method that does not accept it as a valid argument.
        /// </summary>
        internal static Exception ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        /// <summary>
        /// The exception that is thrown when the value of an argument is outside the allowable range of values as defined by the invoked method.
        /// </summary>
        internal static Exception ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        /// <summary>
        /// The exception that is thrown when the author has yet to implement the logic at this point in the program. This can act as an exception based TODO tag.
        /// </summary>
        internal static Exception NotImplemented()
        {
            return new NotImplementedException();
        }

        /// <summary>
        /// The exception that is thrown when an invoked method is not supported, or when there is an attempt to read, seek, or write to a stream that does not support the invoked functionality. 
        /// </summary>
        internal static Exception NotSupported()
        {
            return new NotSupportedException();
        }
    }

    /// <summary>
    ///    AutoGenerated resource class. Usage:
    ///
    ///        string s = EntityRes.GetString(EntityRes.MyIdenfitier);
    /// </summary>
    [GeneratedCode("Resources.tt", "1.0.0.0")]
    internal sealed class EntityRes
    {
        internal const string AddTemplatesError = "AddTemplatesError";
        internal const string ArgumentIsNullOrWhitespace = "ArgumentIsNullOrWhitespace";
        internal const string BuildFailed = "BuildFailed";
        internal const string CreateContextFailed = "CreateContextFailed";
        internal const string EdmSchemaError = "EdmSchemaError";
        internal const string LoadConfigFailed = "LoadConfigFailed";
        internal const string NoContext = "NoContext";
        internal const string Optimize_Begin = "Optimize_Begin";
        internal const string Optimize_ContextError = "Optimize_ContextError";
        internal const string Optimize_EdmxError = "Optimize_EdmxError";
        internal const string Optimize_End = "Optimize_End";
        internal const string Optimize_Error = "Optimize_Error";
        internal const string Optimize_SchemaError = "Optimize_SchemaError";
        internal const string PreconditionFailed = "PreconditionFailed";
        internal const string ProcessTemplateError = "ProcessTemplateError";
        internal const string ReverseEngineer_Complete = "ReverseEngineer_Complete";
        internal const string ReverseEngineer_Error = "ReverseEngineer_Error";
        internal const string ReverseEngineer_GenerateClasses = "ReverseEngineer_GenerateClasses";
        internal const string ReverseEngineer_GenerateContext = "ReverseEngineer_GenerateContext";
        internal const string ReverseEngineer_GenerateMapping = "ReverseEngineer_GenerateMapping";
        internal const string ReverseEngineer_InstallEntityFramework = "ReverseEngineer_InstallEntityFramework";
        internal const string ReverseEngineer_InstallEntityFrameworkError = "ReverseEngineer_InstallEntityFrameworkError";
        internal const string ReverseEngineer_LoadSchema = "ReverseEngineer_LoadSchema";
        internal const string ReverseEngineer_SchemaError = "ReverseEngineer_SchemaError";
        internal const string UnknownDirectiveProcessor = "UnknownDirectiveProcessor";
        internal const string UnsupportedVersion = "UnsupportedVersion";
        internal const string ViewContextError = "ViewContextError";
        internal const string ViewDdlError = "ViewDdlError";

        static EntityRes loader = null;
        ResourceManager resources;

        private EntityRes()
        {
            resources = new ResourceManager("Microsoft.DbContextPackage.Properties.Resources", typeof(Microsoft.DbContextPackage.DbContextPackage).Assembly);
        }

        private static EntityRes GetLoader()
        {
            if (loader == null)
            {
                EntityRes sr = new EntityRes();
                Interlocked.CompareExchange(ref loader, sr, null);
            }
            return loader;
        }

        private static CultureInfo Culture
        {
            get { return null/*use ResourceManager default, CultureInfo.CurrentUICulture*/; }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }

        public static string GetString(string name, params object[] args)
        {
            EntityRes sys = GetLoader();
            if (sys == null)
                return null;
            string res = sys.resources.GetString(name, EntityRes.Culture);

            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    String value = args[i] as String;
                    if (value != null && value.Length > 1024)
                    {
                        args[i] = value.Substring(0, 1024 - 3) + "...";
                    }
                }
                return String.Format(CultureInfo.CurrentCulture, res, args);
            }
            else
            {
                return res;
            }
        }

        public static string GetString(string name)
        {
            EntityRes sys = GetLoader();
            if (sys == null)
                return null;
            return sys.resources.GetString(name, EntityRes.Culture);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            // always false for this version of gensr
            usedFallback = false;
            return GetString(name);
        }

        public static object GetObject(string name)
        {
            EntityRes sys = GetLoader();
            if (sys == null)
                return null;
            return sys.resources.GetObject(name, EntityRes.Culture);
        }
    }
}
