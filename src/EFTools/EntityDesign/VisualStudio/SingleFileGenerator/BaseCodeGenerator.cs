// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.SingleFileGenerator
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    ///     A managed wrapper for VS's concept of an IVsSingleFileGenerator which is
    ///     a custom tool invoked during the build which can take any file as an input
    ///     and provide a compilable code file as output.
    /// </summary>
    [ComVisible(true)]
    public abstract class BaseCodeGenerator : IVsSingleFileGenerator, IDisposable
    {
        private IVsGeneratorProgress _codeGeneratorProgress;

        #region IDisposable implementation

        /// <summary>
        ///     Finalizes an instance of the <see cref="BaseCodeGenerator" /> class.
        /// </summary>
        ~BaseCodeGenerator()
        {
            Dispose(false);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // TODO: uncomment this assert when and if VSCore starts to dispose IVsSingleFileGenerator-s
            //Debug.Assert(disposing,            
            //    typeof(BaseCodeGenerator).Name + ".Dispose(): Finalizing BaseCodeGenerator without disposing it first!");

            _codeGeneratorProgress = null;
        }

        #endregion

        /// <summary>
        ///     interface to the VS shell object we use to tell our
        ///     progress while we are generating.
        /// </summary>
        internal IVsGeneratorProgress CodeGeneratorProgress
        {
            [DebuggerStepThrough] get { return _codeGeneratorProgress; }
        }

        /// <summary>
        ///     gets the default extension for this generator
        /// </summary>
        /// <returns>string with the default extension for this generator</returns>
        protected abstract string DefaultExtensionString { get; }

        /// <summary>
        ///     the method that does the actual work of generating code given the input
        ///     file.
        /// </summary>
        /// <param name="inputFileName">input file name</param>
        /// <param name="inputFileContent">file contents as a string</param>
        /// <returns>the generated code file as a byte-array</returns>
        protected abstract byte[] GenerateCode(string inputFileName, string inputFileContent, string defaultNamespace);

        /// <summary>
        ///     method that will communicate an error via the shell callback mechanism.
        /// </summary>
        /// <param name="warning">true if this is a warning</param>
        /// <param name="level">level or severity</param>
        /// <param name="message">text displayed to the user</param>
        /// <param name="line">line number of error/warning</param>
        /// <param name="column">column number of error/warning</param>
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Microsoft.VisualStudio.Shell.Interop.IVsGeneratorProgress.GeneratorError(System.Int32,System.UInt32,System.String,System.UInt32,System.UInt32)")]
        protected virtual void GeneratorErrorCallback(bool warning, int level, string message, int line, int column)
        {
            var progress = CodeGeneratorProgress;
            if (progress != null)
            {
                progress.GeneratorError(warning ? 1 : 0, (uint)level, message, (uint)line, (uint)column);
            }
        }

        /// <summary>
        ///     Implements the IVsSingleFileGenerator.DefaultExtension method.
        ///     Returns the extension of the generated file
        /// </summary>
        /// <param name="pbstrDefaultExtension">Out parameter, will hold the extension that is to be given to the output file name. The returned extension must include a leading period</param>
        /// <returns>S_OK if successful, E_FAIL if not</returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public int DefaultExtension(out string pbstrDefaultExtension)
        {
            try
            {
                pbstrDefaultExtension = DefaultExtensionString;
                return VSConstants.S_OK;
            }
            catch (Exception e)
            {
                Trace.WriteLine(Strings.GetDefaultExtensionFailed);
                Trace.WriteLine(e.ToString());
                pbstrDefaultExtension = string.Empty;
                return VSConstants.E_FAIL;
            }
        }

        /// <summary>
        ///     main method that the VS shell calls to do the generation
        /// </summary>
        /// <param name="wszInputFilePath">path to the input file</param>
        /// <param name="bstrInputFileContents">contents of the input file as a string (shell handles UTF-8 to Unicode &amp; those types of conversions)</param>
        /// <param name="wszDefaultNamespace">default namespace for the generated code file</param>
        /// <param name="rgbOutputFileContents">byte-array of output file contents</param>
        /// <param name="pcbOutput">count of bytes in the output byte-array</param>
        /// <param name="pGenerateProgress">interface to send progress updates to the shell</param>
        public int Generate(
            string wszInputFilePath,
            string bstrInputFileContents,
            string wszDefaultNamespace,
            IntPtr[] rgbOutputFileContents,
            out uint pcbOutput,
            IVsGeneratorProgress pGenerateProgress)
        {
            if (bstrInputFileContents == null)
            {
                throw new ArgumentNullException(bstrInputFileContents);
            }

            _codeGeneratorProgress = pGenerateProgress;

            var generatedCode = GenerateCode(wszInputFilePath, bstrInputFileContents, wszDefaultNamespace);

            // generated code can be null here if there was a valid exception from XmlReader (not valid byte order marker)
            if (generatedCode == null)
            {
                rgbOutputFileContents[0] = IntPtr.Zero;
                pcbOutput = 0;
            }
            else
            {
                // The contract between IVsSingleFileGenerator implementors and consumers is that 
                // any output returned from IVsSingleFileGenerator.Generate() is returned through  
                // memory allocated via CoTaskMemAlloc(). Therefore, we have to convert the 
                // byte[] array returned from GenerateCode() into an unmanaged blob.  

                var outputLength = generatedCode.Length;
                rgbOutputFileContents[0] = Marshal.AllocCoTaskMem(outputLength);
                Marshal.Copy(generatedCode, 0, rgbOutputFileContents[0], outputLength);
                pcbOutput = (uint)outputLength;
            }
            return VSConstants.S_OK;
        }
    }
}
