// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to change the the FunctionScalarProperty.
    /// </summary>
    internal class ChangeFunctionScalarPropertyCommand : Command
    {
        private readonly FunctionScalarProperty _existingFunctionScalarProperty;
        private readonly List<Property> _propChain;
        private readonly NavigationProperty _pointingNavProp;
        private readonly Parameter _param;
        private readonly string _version;
        private FunctionScalarProperty _updatedFunctionScalarProperty;

        /// <summary>
        ///     Change the property pointed to by the FunctionScalarProperty. This may involve
        ///     change of property chain, change to/from a NavProp property or change of parameter.
        /// </summary>
        /// <param name="fspToChange">FunctionScalarProperty to be changed</param>
        /// <param name="newPropertiesChain">property chain for new Property</param>
        /// <param name="newPointingNavProp">NavProp for new Property (null indicates new Property not reached via NavProp)</param>
        /// <param name="newParameter">Parameter to which new Property should be mapped (null indicates use existing Parameter)</param>
        /// <param name="newVersion">Version attribute for new Property (null indicates use existing value). Only appropriate for Update Functions</param>
        internal ChangeFunctionScalarPropertyCommand(
            FunctionScalarProperty fspToChange, List<Property> newPropertiesChain,
            NavigationProperty newPointingNavProp, Parameter newParameter, string newVersion)
        {
            Debug.Assert(
                !(newPropertiesChain == null && newPointingNavProp == null && newParameter == null),
                "Not all of newPropertiesChain, newPointingNavProp, newParameter can be null");
            if (newPropertiesChain == null
                && newPointingNavProp == null
                && newParameter == null)
            {
                return;
            }

            Debug.Assert(
                string.IsNullOrEmpty(newVersion) ||
                ModelConstants.FunctionScalarPropertyVersionOriginal.Equals(newVersion, StringComparison.Ordinal) ||
                ModelConstants.FunctionScalarPropertyVersionCurrent.Equals(newVersion, StringComparison.Ordinal),
                "newVersion must be empty or " + ModelConstants.FunctionScalarPropertyVersionOriginal + " or "
                + ModelConstants.FunctionScalarPropertyVersionCurrent + ". Actual value: " + newVersion);

            CommandValidation.ValidateFunctionScalarProperty(fspToChange);
            if (newPointingNavProp != null)
            {
                CommandValidation.ValidateNavigationProperty(newPointingNavProp);
            }
            if (newParameter != null)
            {
                CommandValidation.ValidateParameter(newParameter);
            }

            _existingFunctionScalarProperty = fspToChange;
            _propChain = newPropertiesChain;
            _pointingNavProp = newPointingNavProp;
            _param = newParameter;
            _version = newVersion;
        }

        /// <summary>
        ///     This method lets you change just the version of a FunctionScalarProperty.
        /// </summary>
        /// <param name="fspToChange">FunctionScalarProperty to be changed</param>
        /// <param name="version">New version value</param>
        internal ChangeFunctionScalarPropertyCommand(FunctionScalarProperty fspToChange, string version)
        {
            Debug.Assert(version != null, "Version can't be null");
            Debug.Assert(
                ModelConstants.FunctionScalarPropertyVersionOriginal.Equals(version, StringComparison.Ordinal) ||
                ModelConstants.FunctionScalarPropertyVersionCurrent.Equals(version, StringComparison.Ordinal),
                "Version must be " + ModelConstants.FunctionScalarPropertyVersionOriginal + " or "
                + ModelConstants.FunctionScalarPropertyVersionCurrent + ". Actual value: " + version);

            CommandValidation.ValidateFunctionScalarProperty(fspToChange);
            _existingFunctionScalarProperty = fspToChange;
            _version = version;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ExitingFunctionScalarProperty")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(cpc != null, "InvokeInternal is called when ExitingFunctionScalarProperty is null.");

            // safety check, this should never be hit
            if (_existingFunctionScalarProperty == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when ExitingFunctionScalarProperty is null.");
            }

            if (_propChain == null
                && _pointingNavProp == null
                && _param == null
                && _version != null)
            {
                // setting new version only
                if (string.Compare(_existingFunctionScalarProperty.Version.Value, _version, StringComparison.CurrentCulture) != 0)
                {
                    var mfAncestor = _existingFunctionScalarProperty.GetParentOfType(typeof(ModificationFunction)) as ModificationFunction;
                    Debug.Assert(
                        mfAncestor != null,
                        "Bad attempt to set version on FunctionScalarProperty which does not have a ModificationFunction ancestor");
                    if (mfAncestor != null)
                    {
                        Debug.Assert(
                            mfAncestor.FunctionType == ModificationFunctionType.Update,
                            "Bad attempt to set version on FunctionScalarProperty which has a ModificationFunction ancestor whose FunctionType is "
                            +
                            mfAncestor.FunctionType.ToString() + ". Should be " + ModificationFunctionType.Update.ToString());

                        if (mfAncestor.FunctionType == ModificationFunctionType.Update)
                        {
                            _existingFunctionScalarProperty.Version.Value = _version;
                        }
                    }
                }

                _updatedFunctionScalarProperty = _existingFunctionScalarProperty;
                return;
            }

            // if not just setting version then need to delete and re-create FunctionScalarProperty
            // to allow for changes in properties chain
            // where nulls have been passed in, use existing values (except for _pointingNavProp where null
            // indicates "use no NavProp for the new property")
            var mf = _existingFunctionScalarProperty.GetParentOfType(typeof(ModificationFunction)) as ModificationFunction;
            Debug.Assert(mf != null, "Bad attempt to change FunctionScalarProperty which does not have a ModificationFunction ancestor");
            if (mf == null)
            {
                return;
            }

            var propChain = (_propChain != null ? _propChain : _existingFunctionScalarProperty.GetMappedPropertiesList());
            var parameter = (_param != null ? _param : _existingFunctionScalarProperty.ParameterName.Target);
            var version = (_version != null ? _version : _existingFunctionScalarProperty.Version.Value);

            // now construct delete command for existing FunctionScalarProperty followed by create with new properties
            var cmd1 = _existingFunctionScalarProperty.GetDeleteCommand();
            var cmd2 =
                new CreateFunctionScalarPropertyTreeCommand(mf, propChain, _pointingNavProp, parameter, version);
            cmd2.PostInvokeEvent += (o, eventsArgs) =>
                {
                    _updatedFunctionScalarProperty = cmd2.FunctionScalarProperty;
                    Debug.Assert(
                        _updatedFunctionScalarProperty != null,
                        "CreateFunctionScalarPropertyTreeCommand should not result in null FunctionScalarProperty");
                };

            var cp = new CommandProcessor(cpc, cmd1, cmd2);
            try
            {
                cp.Invoke();
            }
            finally
            {
                _updatedFunctionScalarProperty = null;
            }
        }

        internal FunctionScalarProperty FunctionScalarProperty
        {
            get { return _updatedFunctionScalarProperty; }
        }
    }
}
