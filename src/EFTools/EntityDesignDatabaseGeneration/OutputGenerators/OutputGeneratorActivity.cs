// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.DatabaseGeneration.OutputGenerators
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using Microsoft.Data.Entity.Design.DatabaseGeneration.Properties;

    /// <summary>
    ///     An abstract, base WorkflowElement that allows the transformation of a certain format to another format via code
    /// </summary>
    public abstract class OutputGeneratorActivity : NativeActivity
    {
        /// <summary>
        ///     Specifies the assembly-qualified type name of the output generator.
        /// </summary>
        protected string OutputGeneratorOutput { get; set; }

        /// <summary>
        ///     An <see cref="InArgument{T}" /> that specifies the assembly-qualified type name of the output generator.
        /// </summary>
        public InArgument<string> OutputGeneratorType { get; set; }

        internal IGenerateActivityOutput OutputGenerator { get; set; }

        /// <summary>
        ///     Generates output that is supplied to the specified NativeActivityContext based on input specified in the NativeActivityContext.
        /// </summary>
        /// <param name="context">The state of the current activity.</param>
        protected override void Execute(NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Returns the output produced by the output generator with the specified output generator type name.
        /// </summary>
        /// <typeparam name="T">The type of the output.</typeparam>
        /// <param name="outputGeneratorTypeName">The name of the type of the output generator.</param>
        /// <param name="context">The state of the current activity.</param>
        /// <param name="inputs">Inputs for the activity as key-value pairs.</param>
        /// <returns>The output produced by the output generator along with the specified output generator type name.</returns>
        protected T ProcessOutputGenerator<T>(
            string outputGeneratorTypeName, NativeActivityContext context, IDictionary<string, object> inputs) where T : class
        {
            var outputGeneratorType = Type.GetType(outputGeneratorTypeName);
            if (outputGeneratorType == null)
            {
                // if the type name is correct, then the assembly may not be loaded. Try to load it
                // via the assembly loader, which can provide custom logic on how to load assemblies
                // (for the 'Generate Database' wizard, we will only look in the project/website references)
                // NOTE that this will only apply to assembly-qualified type names
                var assemblyName = String.Empty;
                var fqTypeName = String.Empty;
                var indexOfDelimiter = outputGeneratorTypeName.IndexOf(',');
                if (indexOfDelimiter != -1)
                {
                    assemblyName = outputGeneratorTypeName.Substring(indexOfDelimiter + 1).Trim();
                    fqTypeName = outputGeneratorTypeName.Substring(0, indexOfDelimiter).Trim();
                }

                if (false == String.IsNullOrEmpty(assemblyName)
                    && false == String.IsNullOrEmpty(fqTypeName))
                {
                    var symbolResolver = context.GetExtension<SymbolResolver>();
                    if (symbolResolver != null)
                    {
                        var edmParameterBag = symbolResolver[typeof(EdmParameterBag).Name] as EdmParameterBag;
                        if (edmParameterBag != null)
                        {
                            var assemblyLoader = edmParameterBag.GetParameter<IAssemblyLoader>(EdmParameterBag.ParameterName.AssemblyLoader);
                            if (assemblyLoader != null)
                            {
                                var loadedAssembly = assemblyLoader.LoadAssembly(assemblyName);
                                if (loadedAssembly != null)
                                {
                                    try
                                    {
                                        outputGeneratorType = loadedAssembly.GetType(fqTypeName);
                                    }
                                    catch (Exception e)
                                    {
                                        throw new InvalidOperationException(
                                            String.Format(CultureInfo.CurrentCulture, Resources.ErrorNoCodeViewType, outputGeneratorType), e);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (outputGeneratorType == null)
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, Resources.ErrorNoCodeViewType, outputGeneratorTypeName));
            }

            var outputGeneratorConstructorInfo =
                outputGeneratorType.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[] { }, null);
            Debug.Assert(
                outputGeneratorConstructorInfo != null,
                "Should have found a constructor if we were able to find the type in OutputGeneratorActivity.ProcessOutputGenerator");
            if (outputGeneratorConstructorInfo != null)
            {
                OutputGenerator = outputGeneratorConstructorInfo.Invoke(new object[] { }) as IGenerateActivityOutput;
                if (OutputGenerator != null)
                {
                    return OutputGenerator.GenerateActivityOutput<T>(this, context, inputs);
                }
            }
            return null;
        }
    }
}
