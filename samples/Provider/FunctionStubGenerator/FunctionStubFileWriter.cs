//---------------------------------------------------------------------
// <copyright file="FunctionStubFileWriter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//--------------------------------------------------------------------- 

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Metadata.Edm;

namespace LinqFunctionStubsGenerator
{
    /// <summary>
    /// Class that writes the files using the input function metadata.
    /// </summary>
    class FunctionStubFileWriter
    {
        private readonly IEnumerable<EdmFunction> _functions;
        private readonly Dictionary<String, String> _funcDictionaryToUse;
        private readonly Dictionary<String, String> _paramDictionaryToUse;

        /// <summary>
        /// Initializes member fields.
        /// </summary>
        /// <param name="functions">Metadata about functions</param>
        /// <param name="functionNames">Dictionary containing better function names</param>
        /// <param name="parameterNames">Dictionary containing better parameter names</param>
        public FunctionStubFileWriter(IEnumerable<EdmFunction> functions, Dictionary<String, String> functionNames, Dictionary<String, String> parameterNames)
        {
            _functions = functions;
            _funcDictionaryToUse = functionNames;
            _paramDictionaryToUse = parameterNames;
        }

        /// <summary>
        ///  Generates code and writes to the specified file.
        /// </summary>
        /// <param name="destinationFile">Filepath of the destination file</param>
        /// <param name="namespaceString">Namespace where the class will reside</param>
        /// <param name="className">Generated class name</param>
        /// <param name="attributeNamespace">The 'EdmFunction' attribute parameter</param>
        /// <param name="pascalCaseFunctionNames">If the input function names need to be pascal cased.</param>
        public void GenerateToFile(String destinationFile, string namespaceString, string className, string attributeNamespace, Boolean pascalCaseFunctionNames)
        {
            //Use passed in class information to generate the class definition.
            StringWriter newCode = GenerateCode(namespaceString, className, attributeNamespace, pascalCaseFunctionNames);
            
            //Write to file.
            try
            {
                using (StreamWriter writer = new StreamWriter(destinationFile, false))
                {
                    writer.Write(newCode.ToString());
                    writer.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Main code generator method.
        /// </summary>
        /// <param name="namespaceString">Namespace where the class will reside</param>
        /// <param name="className">Generated class name</param>
        /// <param name="attributeNamespace">The 'EdmFunction' attribute parameter</param>
        /// <param name="pascalCaseFunctionNames">If the input function names need to be pascal cased.</param>
        public StringWriter GenerateCode(string namespaceString, string className, string attributeNamespace, Boolean pascalCaseFunctionNames)
        {        
            StringWriter newCode = new StringWriter();
            Boolean isAggregateFunction;
            bool hasSByteParameterOrReturnType;
            bool hasStringInParameterName;
            String separator;

            GenerateFileHeader(newCode, className);
            GenerateUsingStatements(newCode);

            newCode.WriteLine("namespace " + namespaceString);
            newCode.WriteLine("{");

            GenerateClassHeader(newCode, className, attributeNamespace);

            foreach (System.Data.Metadata.Edm.EdmFunction function in _functions)
            {
                isAggregateFunction = false;
                hasSByteParameterOrReturnType = false;
                hasStringInParameterName = false;
                separator = "";
                
                String functionNameToUse = FindCorrectFunctionName(function.Name, pascalCaseFunctionNames);
                GenerateFunctionHeader(newCode,attributeNamespace,function.Name);
                Type returnType = ((PrimitiveType)(function.ReturnParameter.TypeUsage.EdmType)).ClrEquivalentType;

                //Suppress warning that 'SByte' is not CLS-compliant.
                if (returnType == typeof(SByte))
                {
                    hasSByteParameterOrReturnType = true;
                }
                StringBuilder functionSignatureString = new StringBuilder();
                AppendSpaces(functionSignatureString, 8);
                functionSignatureString.Append("public static ");
                WriteType(functionSignatureString, returnType);
                functionSignatureString.Append(functionNameToUse + "(");

                ReadOnlyMetadataCollection<FunctionParameter> functionParameters = function.Parameters;
                Type parameterType;                    
                foreach (System.Data.Metadata.Edm.FunctionParameter parameter in functionParameters)
                {
                    String parameterNameToUse = parameter.Name;
                    parameterNameToUse = FindCorrectParameterName(parameterNameToUse);
                    
                    //Detect aggregate functions. They have just one parameter and so stub can be generated here.
                    if (parameter.TypeUsage.EdmType.GetType() == typeof(System.Data.Metadata.Edm.CollectionType))
                    {
                        isAggregateFunction = true;
                        if (parameterNameToUse.ToLowerInvariant().Contains("string"))
                        {
                            hasStringInParameterName = true;
                        }
                        
                        System.Data.Metadata.Edm.CollectionType collectionType = (System.Data.Metadata.Edm.CollectionType)parameter.TypeUsage.EdmType;
                        parameterType = ((PrimitiveType)(collectionType.TypeUsage.EdmType)).ClrEquivalentType;
                        //Detect if there is an 'SByte' parameter to suppress non-CLS-compliance warning.
                        //Generate the attribute only once for each function.
                        if (parameterType == typeof(SByte))
                        {
                            hasSByteParameterOrReturnType = true;
                        }
                        
                        //Generate stub for non-nullable input parameters
                        functionSignatureString.Append("IEnumerable<" + parameterType.ToString());
                        //Supress fxcop message and CLS non-compliant attributes
                        GenerateFunctionAttributes(newCode, hasStringInParameterName, hasSByteParameterOrReturnType);
                        //Use the constructed function signature
                        newCode.Write(functionSignatureString.ToString());
                        GenerateAggregateFunctionStub(newCode,parameterType, returnType, parameterNameToUse, false);

                        //Generate stub for nullable input parameters
                        //Special Case: Do not generate nullable stub for input parameter of types Byte[]
                        //and String, since they are nullable.
                        if (!IsNullableType(parameterType))
                        {
                            GenerateFunctionHeader(newCode, attributeNamespace, function.Name);
                            //Supress fxcop message and CLS non-compliant attributes
                            GenerateFunctionAttributes(newCode, hasStringInParameterName, hasSByteParameterOrReturnType);
                            //Use the constructed function signature
                            newCode.Write(functionSignatureString.ToString());
                            GenerateAggregateFunctionStub(newCode, parameterType, returnType, parameterNameToUse, true);
                        }
                    } //End of processing parameters for aggregate functions.
                    //Process each parameter in case of non-aggregate functions.
                    else
                    {
                        parameterType = ((PrimitiveType)(parameter.TypeUsage.EdmType)).ClrEquivalentType;
                        functionSignatureString.Append(separator);
                        WriteType(functionSignatureString, parameterType);
                        functionSignatureString.Append(parameterNameToUse);
                        separator = ", ";
                        //Detect if there is an 'SByte' parameter to suppress non-CLS-compliance warning.
                        if (parameterType == typeof(SByte))
                        {
                            hasSByteParameterOrReturnType = true;
                        }
                        if (parameterNameToUse.ToLowerInvariant().Contains("string"))
                        {
                            hasStringInParameterName = true;
                        }
                    }
                } //End for each parameter
                
                //Generate stub for Non-aggregate functions after all input parameters are found.
                if (!isAggregateFunction)
                {   
                    //Supress fxcop supression and CLS non-compliant attributes
                    GenerateFunctionAttributes(newCode, hasStringInParameterName, hasSByteParameterOrReturnType);
                    newCode.WriteLine(functionSignatureString.ToString() + ")");
                    AppendSpaces(newCode, 8);
                    newCode.WriteLine("{");
                    WriteExceptionStatement(newCode);
                }
            } //End for each function

            AppendSpaces(newCode, 4);
            newCode.WriteLine("}");
            newCode.WriteLine("}");
            newCode.Close();
            return newCode;
        }

        /// <summary>
        /// Generates the content for aggregate function stubs.
        /// </summary>
        /// <param name="newCode">Buffer used to store the code constructed so far</param>
        /// <param name="parameterType">Type of parameter</param>
        /// <param name="returnType">Return type of function</param>
        /// <param name="parameterNameToUse">Parameter name</param>
        /// <param name="isNullable">If this invokation is for generating the nullable/non-nullable stub</param>
        private void GenerateAggregateFunctionStub(StringWriter newCode, Type parameterType, Type returnType, string parameterNameToUse, bool isNullable)
        {
            GenerateQuestionMark(newCode, isNullable);
            newCode.Write("> ");
            newCode.WriteLine(parameterNameToUse + ")");
            AppendSpaces(newCode, 8);
            newCode.WriteLine("{");
            AppendSpaces(newCode, 12);
            newCode.Write("ObjectQuery<" + parameterType.ToString());
            GenerateQuestionMark(newCode, isNullable);
            newCode.Write("> objectQuerySource = " + parameterNameToUse);
            newCode.Write(" as ObjectQuery<" + parameterType.ToString());
            GenerateQuestionMark(newCode, isNullable);
            newCode.WriteLine(">;");
            
            AppendSpaces(newCode, 12);
            newCode.WriteLine("if (objectQuerySource != null)");
            AppendSpaces(newCode, 12);
            newCode.WriteLine("{");
            AppendSpaces(newCode, 16);
            newCode.Write("return ((IQueryable)objectQuerySource).Provider.Execute<" + returnType.ToString());

            //Special case: Byte[], String are nullable
            if (!IsNullableType(returnType))
            {
                newCode.Write("?");
            }
            newCode.Write(">(Expression.Call((MethodInfo)MethodInfo.GetCurrentMethod(),Expression.Constant(" + parameterNameToUse);
            newCode.WriteLine(")));");
            AppendSpaces(newCode, 12);
            newCode.WriteLine("}");
            WriteExceptionStatement(newCode);
        }

        /// <summary>
        /// Writes a question mark in generated code for nullable parameters
        /// </summary>
        /// <param name="newCode">Buffer used to store the code constructed so far</param>
        /// <param name="isNullable">Is this invokation for generating the nullable/non-nullable stub</param>
        public void GenerateQuestionMark(StringWriter newCode, bool isNullable)
        {
            if (isNullable)
            {
                newCode.Write("?");
            }
        }

        /// <summary>
        /// Generates fxcop suppression and CLS non-compliant attributes.
        /// </summary>
        /// <param name="newCode"></param>
        /// <param name="hasStringInParameterName"></param>
        /// <param name="hasSByteParameterOrReturnType"></param>
        private void GenerateFunctionAttributes(StringWriter newCode, bool hasStringInParameterName, bool hasSByteParameterOrReturnType)
        {
            //Supress fxcop message about 'string' in argument names.
            if (hasStringInParameterName)
            {
                GenerateFxcopSuppressionAttribute(newCode);
            }
            //Suppress warning that 'SByte' is not CLS-compliant, generate the attribute only once.
            if (hasSByteParameterOrReturnType)
            {
                GenerateSByteCLSNonComplaintAttribute(newCode);
            }
        }

        /// <summary>
        /// Generates the output file header.
        /// </summary>
        /// <param name="newCode">Buffer used to store the code constructed so far</param>
        /// <param name="className">Generated class name</param>
        private  void GenerateFileHeader(StringWriter newCode, String className)
        {
            DateTime theTime = DateTime.Now;
            newCode.WriteLine("//------------------------------------------------------------------------------");
            newCode.WriteLine("// <auto-generated>");
            newCode.WriteLine("//     This code was generated by a tool.");
            newCode.Write("//     Generation date and time : ");
            newCode.WriteLine(theTime.Date.ToShortDateString() + " " + theTime.TimeOfDay.ToString());
            newCode.WriteLine("//");
            newCode.WriteLine("//     Changes to this file will be lost if the code is regenerated.");
            newCode.WriteLine("// </auto-generated>");
            newCode.WriteLine("//------------------------------------------------------------------------------");
            newCode.WriteLine();
        }

        /// <summary>
        /// Generates 'using' statements in the output C# file.
        /// </summary>
        /// <param name="newCode">Buffer used to store the code constructed so far</param>
        private  void GenerateUsingStatements(StringWriter newCode)
        {
            newCode.WriteLine(@"using System;");
            newCode.WriteLine(@"using System.Collections.Generic;");
            newCode.WriteLine(@"using System.Data.Objects;");
            newCode.WriteLine(@"using System.Data.Objects.DataClasses;");
            newCode.WriteLine(@"using System.Linq;");
            newCode.WriteLine(@"using System.Linq.Expressions;");
            newCode.WriteLine(@"using System.Reflection;");
            newCode.WriteLine();
        }

        /// <summary>
        /// Generates the function header comment and 'EdmFunction' attribute for every function.
        /// </summary>
        /// <param name="newCode">Buffer used to store the code constructed so far</param>
        /// <param name="attributeNamespace">Namespace parameter to the 'EdmFunction' attribute</param>
        /// <param name="functionName">Function name</param>
        private  void GenerateFunctionHeader(StringWriter newCode, String attributeNamespace, String functionName)
        {
                AppendSpaces(newCode, 8);
                newCode.WriteLine("/// <summary>");
                AppendSpaces(newCode, 8);
                newCode.WriteLine("/// Proxy for the function " + attributeNamespace + "." + functionName);
                AppendSpaces(newCode, 8);
                newCode.WriteLine("/// </summary>");
                AppendSpaces(newCode, 8);
                newCode.WriteLine("[EdmFunction(\""+attributeNamespace+"\", \""+functionName+"\")]");
        }

        /// <summary>
        /// Writes the function attribute to suppress warnings about non-CLS compliance due 
        /// to SByte arguments.
        /// </summary>
        /// <param name="newCode">Buffer used to store the code constructed so far</param>
        private  void GenerateSByteCLSNonComplaintAttribute(StringWriter newCode)
        {
            AppendSpaces(newCode, 8);
            newCode.WriteLine("[CLSCompliant(false)]");
        }

        /// <summary>
        /// Writes the function attribute to suppress 'fxcop' errors about argument names
        /// like 'string1','characterString', etc.
        /// </summary>
        /// <param name="newCode">Buffer used to store the code constructed so far</param>
        private  void GenerateFxcopSuppressionAttribute(StringWriter newCode)
        {
            AppendSpaces(newCode, 8);
            newCode.WriteLine("[System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Naming\", \"CA1720:IdentifiersShouldNotContainTypeNames\", MessageId = \"string\")]");
        }

        /// <summary>
        /// Returns if the given data type is nullable.
        /// </summary>
        /// <param name="parameterType">Input data type</param>
        /// <returns>True of input type is nullable, false otherwise</returns>
        private  Boolean IsNullableType(Type parameterType)
        {
            return ((parameterType == typeof(Byte[])) ||
                    (parameterType == typeof(String)));
        }

        /// <summary>
        /// Generates type information in code according to whether it is nullable
        /// </summary>
        /// <param name="code">Buffer used to store the code constructed so far</param>
        /// <param name="parameterType">Input type</param>
        private  void WriteType(StringBuilder code, Type parameterType)
        {
            code.Append(parameterType.ToString());
            if (!IsNullableType(parameterType))
            {
                code.Append("?");
            }
            code.Append(" ");
        }

        /// <summary>
        /// Generates the exception statement which is thrown from each function stub.
        /// </summary>
        /// <param name="code">Buffer used to store the code constructed so far</param>
        private  void WriteExceptionStatement(StringWriter code)
        {
            AppendSpaces(code, 12);
            code.WriteLine("throw new NotSupportedException(\"This function can only be invoked from LINQ to Entities.\");");
            AppendSpaces(code, 8);
            code.WriteLine("}");
            code.WriteLine();
        }

        /// <summary>
        /// Appends spaces to code line, this is avoid tabs as required by coding standards.
        /// </summary>
        /// <param name="str">Buffer used to store the code constructed so far</param>
        /// <param name="num">Number of desired spaces</param>
        private  void AppendSpaces(StringWriter str, int num)
        {
            for (int i = 0; i < num; i++)
            {
                str.Write(" ");
            }
        }

        /// <summary>
        /// Appends spaces to code line, this is avoid tabs as required by coding standards.
        /// </summary>
        /// <param name="str">Buffer used to store the code constructed so far</param>
        /// <param name="num">Number of desired spaces</param>
        private  void AppendSpaces(StringBuilder str, int num)
        {
            for (int i = 0; i < num; i++)
            {
                str.Append(" ");
            }
        }

        /// <summary>
        /// Looks up dictionary to find better function name(that fxcop likes). If not in dictionary
        /// Pascal cases it if asked to.
        /// </summary>
        /// <param name="inputName">Function name from metadata</param>
        /// <param name="pascalCaseFunctionNames">If required to Pascal case function name</param>
        /// <returns>Better function name</returns>
        private  String FindCorrectFunctionName(String inputName, Boolean pascalCaseFunctionNames)
        {
            if (_funcDictionaryToUse == null)
            {
                return inputName;
            }
            
            String value;
            if (_funcDictionaryToUse.TryGetValue(inputName, out value))
            {
                return value;
            }
            else if (pascalCaseFunctionNames)
            {
                String interFunctionName = inputName.ToLower();
                char[] charFuncName = interFunctionName.ToCharArray();
                charFuncName[0] = System.Char.ToUpper(charFuncName[0]);
                return new String(charFuncName);
            }
            return inputName;
        }

        /// <summary>
        /// Looks up dictionary to find better parameter name(that fxcop likes and one that is friendlier). 
        /// </summary>
        /// <param name="inputName">Parameter name from metadata</param>
        /// <returns>Better parameter name</returns>
        private  String FindCorrectParameterName(String inputParameterName)
        {
            String value;

            if (_paramDictionaryToUse == null)
            {
                return inputParameterName;
            }
            if (_paramDictionaryToUse.TryGetValue(inputParameterName, out value))
            {
                return value;
            }
            return inputParameterName;
        }

        /// <summary>
        /// Generates header for the class.
        /// </summary>
        /// <param name="newCode">Buffer used to store the code constructed so far</param>
        /// <param name="className">Output class name</param>
        public  void GenerateClassHeader(StringWriter newCode, String className, String namespaceString)
        {
            AppendSpaces(newCode, 4);
            newCode.WriteLine("/// <summary>");
            AppendSpaces(newCode, 4);
            newCode.Write("/// Contains function stubs that expose " +namespaceString);
            newCode.WriteLine(" methods in Linq to Entities.");
            AppendSpaces(newCode, 4);
            newCode.WriteLine("/// </summary>");
            AppendSpaces(newCode, 4);
            newCode.Write("public static ");
            if (className.Equals("EntityFunctions", StringComparison.Ordinal))
            {
                newCode.Write("partial ");
            }
            newCode.WriteLine("class " + className);
            AppendSpaces(newCode, 4);
            newCode.WriteLine("{");
        }
    }
}
