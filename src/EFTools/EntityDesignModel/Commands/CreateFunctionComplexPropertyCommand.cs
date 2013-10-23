// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to create a FunctionComplexProperty that lives (perhaps nested) inside a ModificationFunction.
    /// </summary>
    internal class CreateFunctionComplexPropertyCommand : Command
    {
        internal static readonly string PrereqId = "CreateFunctionComplexPropertyCommand";

        private readonly ComplexConceptualProperty _property;
        private readonly ModificationFunction _parentModificationFunction;
        private FunctionComplexProperty _parentComplexProperty;
        private FunctionComplexProperty _createdProperty;

        /// <summary>
        ///     Creates a FunctionComplexProperty within a ModificationFunction mapped to the passed in
        ///     C-side property.
        /// </summary>
        /// <param name="parentModificationFunction">The parent inside which this FunctionComplexProperty will be placed</param>
        /// <param name="property">This must be a valid Property from the C-Model.</param>
        internal CreateFunctionComplexPropertyCommand(ModificationFunction parentModificationFunction, ComplexConceptualProperty property)
            : base(PrereqId)
        {
            CommandValidation.ValidateModificationFunction(parentModificationFunction);
            CommandValidation.ValidateConceptualProperty(property);

            _parentModificationFunction = parentModificationFunction;
            _property = property;
        }

        /// <summary>
        ///     Creates a FunctionComplexProperty using a FunctionComplexProperty from prereq command
        /// </summary>
        /// <param name="prereq">Pre-requisite Command which will produce the FunctionComplexProperty inside which the newly produced FunctionComplexProperty will be placed</param>
        /// <param name="property">This must be a valid ComplexConceptualProperty.</param>
        internal CreateFunctionComplexPropertyCommand(CreateFunctionComplexPropertyCommand prereq, ComplexConceptualProperty property)
            : base(PrereqId)
        {
            ValidatePrereqCommand(prereq);
            CommandValidation.ValidateConceptualProperty(property);

            _property = property;
            AddPreReqCommand(prereq);
        }

        protected override void ProcessPreReqCommands()
        {
            var prereq = GetPreReqCommand(PrereqId) as CreateFunctionComplexPropertyCommand;
            if (prereq != null)
            {
                _parentComplexProperty = prereq.FunctionComplexProperty;
                CommandValidation.ValidateFunctionComplexProperty(_parentComplexProperty);

                Debug.Assert(_parentComplexProperty != null, "We didn't get a good FunctionComplexProperty out of the pre-req Command");
            }

            base.ProcessPreReqCommands();
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // check ModificationFunction or ComplexProperty parent exists
            Debug.Assert(
                _parentModificationFunction != null || _parentComplexProperty != null,
                "Must have either a ModificationFunction or a ComplexProperty parent to house this ComplexProperty");
            if (_parentModificationFunction == null
                && _parentComplexProperty == null)
            {
                throw new CannotLocateParentItemException();
            }

            // check both ModificationFunction and ComplexProperty parents don't exist
            Debug.Assert(
                _parentModificationFunction == null || _parentComplexProperty == null,
                "Must not have both a ModificationFunction and a ComplexProperty parent to house this ComplexProperty");
            if (_parentModificationFunction != null
                && _parentComplexProperty != null)
            {
                throw new CannotLocateParentItemException();
            }

            if (_parentModificationFunction != null)
            {
                _createdProperty = CreateComplexPropertyUsingModificationFunction(_parentModificationFunction, _property);
            }
            else if (_parentComplexProperty != null)
            {
                _createdProperty = CreateComplexPropertyUsingComplexProperty(_parentComplexProperty, _property);
            }

            Debug.Assert(_createdProperty != null, "Failed to create a FunctionComplexProperty");
        }

        /// <summary>
        ///     Returns the FunctionComplexProperty created by this command
        /// </summary>
        internal FunctionComplexProperty FunctionComplexProperty
        {
            get { return _createdProperty; }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static FunctionComplexProperty CreateComplexPropertyUsingModificationFunction(
            ModificationFunction parentModificationFunction, ComplexConceptualProperty property)
        {
            // make sure that we don't already have one
            var fcp = parentModificationFunction.FindFunctionComplexProperty(property);
            if (fcp == null)
            {
                fcp = CreateNewFunctionComplexProperty(parentModificationFunction, property);
                parentModificationFunction.AddComplexProperty(fcp);
            }

            return fcp;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static FunctionComplexProperty CreateComplexPropertyUsingComplexProperty(
            FunctionComplexProperty parentComplexProperty, ComplexConceptualProperty property)
        {
            // make sure that we don't already have one
            var fcp = parentComplexProperty.FindFunctionComplexProperty(property);
            if (fcp == null)
            {
                fcp = CreateNewFunctionComplexProperty(parentComplexProperty, property);
                parentComplexProperty.AddComplexProperty(fcp);
            }
            return fcp;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static FunctionComplexProperty CreateNewFunctionComplexProperty(EFElement parent, ComplexConceptualProperty property)
        {
            Debug.Assert(property != null, 
                "CreateFunctionComplexPropertyCommand.CreateNewFunctionComplexProperty() received null property");
            Debug.Assert
                (property.ComplexType.Target != null,
                 typeof(CreateFunctionComplexPropertyCommand).Name
                    + ".CreateNewFunctionComplexProperty() received property with null ComplexType.Target");

            // actually create it in the XLinq tree
            var fcp = new FunctionComplexProperty(parent, null);
            fcp.Name.SetRefName(property);
            fcp.TypeName.SetRefName(property.ComplexType.Target);

            XmlModelHelper.NormalizeAndResolve(fcp);

            if (fcp == null)
            {
                throw new ItemCreationFailureException();
            }

            Debug.Assert(
                fcp.Name.Target != null && fcp.Name.Target.LocalName.Value == fcp.Name.RefName,
                (fcp.Name.Target == null
                     ? "Broken property resolution for FunctionComplexProperty " + fcp.ToPrettyString() + ": null Target"
                     : "Broken property resolution for FunctionComplexProperty " + fcp.ToPrettyString() + ": Target.LocalName = "
                       + fcp.Name.Target.LocalName.Value + ", RefName = " + fcp.Name.RefName));

            return fcp;
        }
    }
}
