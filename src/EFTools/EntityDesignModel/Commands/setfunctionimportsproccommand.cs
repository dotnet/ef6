// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to set a function assocated with a function import
    ///     Example Function, corresponding FunctionImport, and the FunctionImportMapping:
    ///     &lt;Function Name=&quot;GetOrderDetails&quot; Aggregate=&quot;false&quot;
    ///     BuiltIn=&quot;false&quot; NiladicFunction=&quot;false&quot;
    ///     IsComposable=&quot;false&quot;
    ///     ParameterTypeSemantics=&quot;AllowImplicitConversion&quot;
    ///     Schema=&quot;dbo&quot;&gt;
    ///     &lt;Parameter Name=&quot;SalesOrderHeaderId&quot; Type=&quot;int&quot; Mode=&quot;in&quot; /&gt;
    ///     &lt;/Function&gt;
    ///     &lt;FunctionImport Name=&quot;GetOrderDetails&quot;
    ///     EntitySet=&quot;SalesOrderDetail&quot;
    ///     ReturnType=&quot;Collection(AdventureWorksModel.SalesOrderDetail)&quot;&gt;
    ///     &lt;Parameter Name=&quot;SalesOrderHeaderId&quot; Type=&quot;Int32&quot; Mode=&quot;in&quot;&gt;&lt;/Parameter&gt;
    ///     &lt;/FunctionImport&gt;
    ///     &lt;FunctionImportMapping FunctionImportName=&quot;GetOrderDetails&quot;
    ///     FunctionName=&quot;AdventureWorksModel.Store.GetOrderDetails&quot;/&gt;
    /// </summary>
    internal class SetFunctionImportSprocCommand : Command
    {
        private FunctionImportMapping _fim;
        private readonly Function _function;
        private readonly FunctionImport _functionImport;

        internal SetFunctionImportSprocCommand(FunctionImport functionImport, Function function)
        {
            CommandValidation.ValidateFunctionImport(functionImport);
            CommandValidation.ValidateFunction(function);

            _functionImport = functionImport;
            _function = function;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(
                _function != null && _functionImport != null, "InvokeInternal is called when _function or _function import is null.");
            if (_function == null
                || _functionImport == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when _function or _function import is null.");
            }

            _fim = _functionImport.FunctionImportMapping;
            _fim.FunctionName.SetRefName(_function);

            XmlModelHelper.NormalizeAndResolve(_fim);
        }
    }
}
