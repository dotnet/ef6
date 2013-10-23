// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Creates one of three modification function mapping elements inside a ModificationFunctionMapping.
    ///     The type of element to create is determined by the ModificationFunctionType argument.
    ///     Example:
    ///     &lt;ModificationFunctionMapping&gt;
    ///     &lt;InsertFunction FunctionName=&quot;PerfDB.Store.sp_insert_run&quot;&gt;
    ///     &lt;/InsertFunction&gt;
    ///     &lt;UpdateFunction FunctionName=&quot;PerfDB.Store.sp_update_run&quot;&gt;
    ///     &lt;/UpdateFunction&gt;
    ///     &lt;DeleteFunction FunctionName=&quot;PerfDB.Store.sp_delete_run&quot;&gt;
    ///     &lt;/DeleteFunction&gt;
    ///     &lt;/ModificationFunctionMapping&gt;
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class CreateFunctionMappingCommand : Command
    {
        private readonly EntityType _conceptualEntityType;
        private readonly Function _function;
        private readonly ModificationFunctionType _type;
        private ModificationFunction _modificationFunction;
        private readonly Parameter _rowsAffectedParameter;

        /// <summary>
        ///     Creates a function mapping element for the passed in Function, inside an EntityTypeMapping
        ///     for the passed in entity.
        /// </summary>
        /// <param name="conceptualEntityType"></param>
        /// <param name="function"></param>
        /// <param name="rowsAffectedParameter">Optional string argument</param>
        /// <param name="type"></param>
        internal CreateFunctionMappingCommand(
            EntityType conceptualEntityType, Function function, Parameter rowsAffectedParameter, ModificationFunctionType type)
        {
            Debug.Assert(type != ModificationFunctionType.None, "You cannot pass the None type");
            CommandValidation.ValidateConceptualEntityType(conceptualEntityType);
            CommandValidation.ValidateFunction(function);

            _conceptualEntityType = conceptualEntityType;
            _function = function;
            _type = type;
            _rowsAffectedParameter = rowsAffectedParameter;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "conceptualEntityType")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(
                _conceptualEntityType != null && _function != null,
                "InvokeInternal is called when _conceptualEntityType or _function is null");
            if (_conceptualEntityType == null
                || _function == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when _conceptualEntityType or _function is null");
            }

            // see if we have the ETM we need, if not create it
            var entityTypeMapping = ModelHelper.FindEntityTypeMapping(
                cpc,
                _conceptualEntityType,
                EntityTypeMappingKind.Function,
                true);
            Debug.Assert(entityTypeMapping != null, "Failed to create the EntityTypeMapping to house this item");
            if (entityTypeMapping == null)
            {
                throw new ParentItemCreationFailureException();
            }

            // if we created a new ETM, then we'll need to also create the ModificationFunctionMapping
            if (entityTypeMapping.ModificationFunctionMapping == null)
            {
                entityTypeMapping.ModificationFunctionMapping = new ModificationFunctionMapping(entityTypeMapping, null);
                XmlModelHelper.NormalizeAndResolve(entityTypeMapping.ModificationFunctionMapping);
            }
            Debug.Assert(
                entityTypeMapping.ModificationFunctionMapping != null,
                "Failed to create the ModificationFunctionMapping node to house this item");
            if (entityTypeMapping.ModificationFunctionMapping == null)
            {
                throw new ParentItemCreationFailureException();
            }

            // now go and actually create the item
            if (_type == ModificationFunctionType.Insert)
            {
                _modificationFunction = new InsertFunction(entityTypeMapping.ModificationFunctionMapping, null);
                entityTypeMapping.ModificationFunctionMapping.InsertFunction = _modificationFunction as InsertFunction;
            }
            else if (_type == ModificationFunctionType.Update)
            {
                _modificationFunction = new UpdateFunction(entityTypeMapping.ModificationFunctionMapping, null);
                entityTypeMapping.ModificationFunctionMapping.UpdateFunction = _modificationFunction as UpdateFunction;
            }
            else if (_type == ModificationFunctionType.Delete)
            {
                _modificationFunction = new DeleteFunction(entityTypeMapping.ModificationFunctionMapping, null);
                entityTypeMapping.ModificationFunctionMapping.DeleteFunction = _modificationFunction as DeleteFunction;
            }
            Debug.Assert(_modificationFunction != null, "Failed to create the new function mapping");
            if (_modificationFunction == null)
            {
                throw new ItemCreationFailureException();
            }

            // set the function into the function mapping
            _modificationFunction.FunctionName.SetRefName(_function);

            // set the RowsAffectedParameter
            var cmd = new SetRowsAffectedParameterCommand(_modificationFunction, _rowsAffectedParameter);
            CommandProcessor.InvokeSingleCommand(cpc, cmd);

            XmlModelHelper.NormalizeAndResolve(_modificationFunction);
        }

        /// <summary>
        ///     Returns the function mapping created by this command
        /// </summary>
        internal ModificationFunction ModificationFunction
        {
            get { return _modificationFunction; }
        }
    }
}
