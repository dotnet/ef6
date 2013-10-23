// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to create a FunctionImportMapping in the MSL from a Function (stored procedure) in
    ///     the S-Side and a FunctionImport on the C-Side
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
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class CreateFunctionImportMappingCommand : Command
    {
        internal static readonly string PrereqId = "CreateFunctionImportMappingCommand";
        private FunctionImportMapping _fim;
        internal EntityContainerMapping ContainerMapping { get; set; }
        internal Function Function { get; set; }
        internal FunctionImport FunctionImport { get; set; }
        private readonly string _createFuncImpCmdId;

        internal CreateFunctionImportMappingCommand(EntityContainerMapping em, Function function, string createFuncImpCmdId)
            : base(PrereqId)
        {
            ContainerMapping = em;
            Function = function;
            _createFuncImpCmdId = createFuncImpCmdId;
        }

        internal CreateFunctionImportMappingCommand(EntityContainerMapping em, Function function, FunctionImport functionImport)
            : base(PrereqId)
        {
            ContainerMapping = em;
            Function = function;
            FunctionImport = functionImport;
        }

        internal CreateFunctionImportMappingCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        protected override void ProcessPreReqCommands()
        {
            if (_createFuncImpCmdId != null)
            {
                var createFuncImpCmd = GetPreReqCommand(_createFuncImpCmdId) as CreateFunctionImportCommand;
                if (createFuncImpCmd != null)
                {
                    FunctionImport = createFuncImpCmd.FunctionImport;
                }
            }
        }

        internal FunctionImportMapping FunctionImportMapping
        {
            get { return _fim; }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ContainerMapping")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "FunctionImport")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(
                Function != null && FunctionImport != null && ContainerMapping != null,
                "InvokeInternal is called when Function or FunctionImport or ContainerMapping is null.");

            if (Function == null
                || FunctionImport == null
                || ContainerMapping == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when Function or FunctionImport or ContainerMapping is null.");
            }

            _fim = new FunctionImportMapping(ContainerMapping, null);
            _fim.FunctionImportName.SetRefName(FunctionImport);
            _fim.FunctionName.SetRefName(Function);

            ContainerMapping.AddFunctionImportMapping(_fim);
            XmlModelHelper.NormalizeAndResolve(_fim);
        }
    }
}
