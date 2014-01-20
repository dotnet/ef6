// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System;
    using System.Globalization;
    using System.Xaml;

    // <summary>
    //     This class helps the database generation workflow resolve assemblies that contain
    //     activities that are referenced in the owning project of the artifact.
    // </summary>
    internal class DatabaseGenerationXamlSchemaContext : XamlSchemaContext
    {
        private string _clrNamespacePrefix = "clr-namespace:";
        private string _assemblyPrefix = "assembly=";
        private readonly DatabaseGenerationAssemblyLoader _assemblyLoader;

        internal DatabaseGenerationXamlSchemaContext(DatabaseGenerationAssemblyLoader assemblyLoader)
        {
            _assemblyLoader = assemblyLoader;
        }

        protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
        {
            var xamlType = base.GetXamlType(xamlNamespace, name, typeArguments);

            if (xamlType == null
                || xamlType.IsUnknown)
            {
                // if the XAML namespace is a CLR namespace then look through the project references for the assembly
                // name and lazily load the assembly. Then attempt to get the XAML type.
                string clrNamespace, assemblyName;
                if (TrySplitXamlNamespace(xamlNamespace, out clrNamespace, out assemblyName))
                {
                    try
                    {
                        var loadedAssembly = _assemblyLoader.LoadAssembly(assemblyName);
                        if (loadedAssembly != null)
                        {
                            var fqName = String.Format(CultureInfo.InvariantCulture, "{0}.{1}", clrNamespace, name);
                            var clrType = loadedAssembly.GetType(fqName);
                            xamlType = base.GetXamlType(clrType);
                        }
                    }
                    catch (Exception e)
                    {
                        // There are a variety of exceptions that could happen during assembly load. If this fails
                        // we will let the DBGen XAML loader complain to the user so they will know how to fix it.
                        // Note that with CLR v4, auto-sandboxing will not take place on assemblies in network shares so
                        // this process will fail. We will attempt to bubble up details of the inner exception to the user
                        // so they will know how to proceed.
                        if (e.InnerException == null)
                        {
                            throw new InvalidOperationException(
                                String.Format(
                                    CultureInfo.CurrentCulture, Resources.DatabaseCreation_AssemblyLoadError, assemblyName, e.Message), e);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                String.Format(
                                    CultureInfo.CurrentCulture, Resources.DatabaseCreation_AssemblyLoadErrorWithInner, assemblyName,
                                    e.Message, e.InnerException.Message), e);
                        }
                    }
                }
            }

            return xamlType;
        }

        private bool TrySplitXamlNamespace(string xamlNamespace, out string clrNamespace, out string assemblyName)
        {
            clrNamespace = String.Empty;
            assemblyName = String.Empty;

            var indexOfClrNamespace = xamlNamespace.IndexOf(_clrNamespacePrefix, StringComparison.OrdinalIgnoreCase);
            var indexOfAssembly = xamlNamespace.IndexOf(_assemblyPrefix, StringComparison.OrdinalIgnoreCase);
            if (indexOfClrNamespace == -1
                || indexOfAssembly == -1
                || indexOfAssembly < indexOfClrNamespace)
            {
                return false;
            }

            clrNamespace = xamlNamespace.Substring(
                indexOfClrNamespace + _clrNamespacePrefix.Length, indexOfAssembly - _clrNamespacePrefix.Length - 1);
            assemblyName = xamlNamespace.Substring(indexOfAssembly + _assemblyPrefix.Length);
            return true;
        }

        public override XamlType GetXamlType(Type type)
        {
            return base.GetXamlType(type);
        }
    }
}
