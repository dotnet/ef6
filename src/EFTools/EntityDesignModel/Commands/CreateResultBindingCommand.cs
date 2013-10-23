// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to create a new ResultBinding inside a function mapping.
    ///     Example:
    ///     &lt;InsertFunction FunctionName=&quot;PerfDB.Store.sp_insert_test_metric&quot;&gt;
    ///     &lt;ResultBinding ColumnName=&quot;id&quot; Name=&quot;id&quot; /&gt;
    ///     &lt;/InsertFunction&gt;
    ///     These can live as direct children of a ModificationFunction (not Delete).  The different
    ///     thing with these, is that ColumnName does NOT point to a Property in an S-Side entity but
    ///     to a column in the function's result set.  These "column names" cannot be validated until runtime.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class CreateResultBindingCommand : Command
    {
        private ResultBinding _binding;
        private readonly EntityType _conceptualEntityType;
        private readonly Function _function;
        private readonly ModificationFunctionType _functionType;
        private readonly Property _entityProperty;
        private readonly string _columnName;

        /// <summary>
        ///     Creates a ResultBinding element that maps the passed in Property to the result column, inside
        ///     an EntityTypeMapping for the passed in entity.  The Function is used to locate the correct
        ///     ModificationFunction to house this.
        /// </summary>
        /// <param name="conceptualEntityType"></param>
        /// <param name="function"></param>
        /// <param name="entityProperty">Optional</param>
        /// <param name="columnName">Optional</param>
        internal CreateResultBindingCommand(
            EntityType conceptualEntityType, Function function, ModificationFunctionType functionType, Property entityProperty,
            string columnName)
        {
            CommandValidation.ValidateConceptualEntityType(conceptualEntityType);
            CommandValidation.ValidateFunction(function);
            Debug.Assert(functionType != ModificationFunctionType.None, "You cannot pass the None type");

            _conceptualEntityType = conceptualEntityType;
            _function = function;
            _functionType = functionType;
            _entityProperty = entityProperty;
            _columnName = columnName;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "conceptualEntityType")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(
                _conceptualEntityType != null && _function != null,
                "InvokeInternal is called when _conceptualEntityType or _function is null.");
            if (_conceptualEntityType == null
                || _function == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when _conceptualEntityType or _function is null");
            }

            // find the parent item to put this in
            var mf = ModelHelper.FindModificationFunction(cpc, _conceptualEntityType, _function, _functionType);
            Debug.Assert(mf != null, "Couldn't find the ModificationFunction to house this ResultBinding");
            if (mf == null)
            {
                throw new CannotLocateParentItemException();
            }

            // create it
            _binding = new ResultBinding(mf, null);
            if (_entityProperty != null)
            {
                _binding.Name.SetRefName(_entityProperty);
            }
            if (string.IsNullOrEmpty(_columnName) == false)
            {
                _binding.ColumnName.Value = _columnName;
            }
            mf.AddResultBinding(_binding);

            XmlModelHelper.NormalizeAndResolve(_binding);

            // if we passed in a property, validate that the binding worked
            if (_entityProperty != null)
            {
                Debug.Assert(
                    _binding.Name.Target != null && _binding.Name.Target.LocalName.Value == _binding.Name.RefName,
                    "Broken entity property resolution");
            }
        }

        /// <summary>
        ///     Returns the ResultBinding created by this command
        /// </summary>
        internal ResultBinding ResultBinding
        {
            get { return _binding; }
        }
    }
}
