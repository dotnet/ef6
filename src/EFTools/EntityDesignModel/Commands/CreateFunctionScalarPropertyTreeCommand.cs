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
    ///     Use this command to create a ScalarProperty that lives on any level of a ComplexProperty tree inside
    ///     a ModificationFunction.
    ///     (This is different than those ScalarProperties that can be added to an EndProperty or MappingFragment.)
    ///     Note: this creates the whole tree including any required intermediate ComplexProperty nodes or
    ///     an AssociationEnd node if the ScalarProperty was via a NavigationProperty.
    ///     Example:
    ///     &lt;ModificationFunctionMapping&gt;
    ///     &lt;UpdateFunction FunctionName=&quot;PerfDB.Store.sp_insert_test_metric&quot;&gt;
    ///     &lt;ScalarProperty Name=&quot;name&quot; ParameterName=&quot;name&quot; /&gt;
    ///     &lt;ComplexProperty Name=&quot;Colors&quot; TypeName=&quot;Model1.HouseColors&quot;&gt;
    ///     &lt;ComplexProperty Name=&quot;MainColor&quot; TypeName=&quot;Model1.Color&quot;&gt;
    ///     &lt;ScalarProperty Name=&quot;G&quot; ParameterName=&quot;Param1&quot; /&gt;
    ///     &lt;/ComplexProperty&gt;
    ///     &lt;/ComplexProperty&gt;
    ///     &lt;AssociationEnd AssociationSet=&quot;FK_TestMetric_Test&quot; From=&quot;TestMetric&quot; To=&quot;Test&quot;&gt;
    ///     &lt;ScalarProperty Name=&quot;id&quot; ParameterName=&quot;testId&quot; Version=&quot;Original&quot; /&gt;
    ///     &lt;/AssociationEnd&gt;
    ///     &lt;/UpdateFunction&gt;
    ///     &lt;/ModificationFunctionMapping&gt;
    /// </summary>
    internal class CreateFunctionScalarPropertyTreeCommand : Command
    {
        private readonly ModificationFunction _modificationFunction;
        private readonly List<Property> _propertyChain;
        private readonly NavigationProperty _navPropPointingToProperty; // null to indicate was not reached via NavProp
        private readonly Parameter _parameter;
        private readonly string _version;
        private FunctionScalarProperty _createdProperty;

        /// <summary>
        ///     Creates a FunctionScalarProperty element (potentially within multiple ComplexProperty elements) that
        ///     maps the Property at the bottom of the passed in property chain to the passed in Parameter
        ///     within the passed in ModificationFunction.
        /// </summary>
        /// <param name="modificationFunction">ultimate parent for ScalarProperty created by this</param>
        /// <param name="propertyChain">property to be inserted, preceded by its ComplexProperty parents if appropriate</param>
        /// <param name="navPropPointingToProperty">
        ///     Optional - if present indicates the Scalar Property at
        ///     the bottom of the propertyChain was reached via a NavProp (propertyChain will have only 1 Property)
        /// </param>
        /// <param name="parameter">The sproc parameter to which we will map</param>
        internal CreateFunctionScalarPropertyTreeCommand(
            ModificationFunction modificationFunction, List<Property> propertyChain,
            NavigationProperty navPropPointingToProperty, Parameter parameter, string version)
        {
            CommandValidation.ValidateModificationFunction(modificationFunction);
            CommandValidation.ValidateParameter(parameter);
            if (null != navPropPointingToProperty)
            {
                CommandValidation.ValidateNavigationProperty(navPropPointingToProperty);
            }
            Debug.Assert(propertyChain.Count > 0, "Properties list should contain at least one element");

            _modificationFunction = modificationFunction;
            _propertyChain = propertyChain;
            _navPropPointingToProperty = navPropPointingToProperty;
            if (_navPropPointingToProperty != null)
            {
                Debug.Assert(
                    propertyChain.Count == 1,
                    "When creating a FunctionScalarProperty via a NavigationProperty propertyChain should have only 1 element, but it has "
                    + propertyChain.Count);
            }
            _parameter = parameter;
            Debug.Assert(
                string.IsNullOrEmpty(version)
                || ModelConstants.FunctionScalarPropertyVersionOriginal.Equals(version, StringComparison.Ordinal)
                || ModelConstants.FunctionScalarPropertyVersionCurrent.Equals(version, StringComparison.Ordinal),
                "version should be empty or " + ModelConstants.FunctionScalarPropertyVersionOriginal + " or "
                + ModelConstants.FunctionScalarPropertyVersionCurrent + ". Actual value: " + version);
            _version = version;
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var cp = new CommandProcessor(cpc);
            CreateFunctionComplexPropertyCommand preReqCmd = null;
            for (var i = 0; i < _propertyChain.Count; i++)
            {
                var property = _propertyChain[i];
                Debug.Assert(property.EntityModel.IsCSDL, "Each Property in the chain must be in the CSDL");
                var complexConceptualProperty = property as ComplexConceptualProperty;
                if (complexConceptualProperty != null)
                {
                    Debug.Assert(i < _propertyChain.Count - 1, "Last property shouldn't be ComplexConceptualProperty");
                    CreateFunctionComplexPropertyCommand cmd = null;
                    if (preReqCmd == null)
                    {
                        // first property has a mapping whose parent is the ModificationFunction itself
                        cmd = new CreateFunctionComplexPropertyCommand(_modificationFunction, complexConceptualProperty);
                    }
                    else
                    {
                        // later properties have a mapping whose parent is the ComplexProperty produced from the previous command
                        cmd = new CreateFunctionComplexPropertyCommand(preReqCmd, complexConceptualProperty);
                    }

                    // set up the prereq Command to use for next time around this loop and for the 
                    // CreateFunctionScalarPropertyCommand below
                    preReqCmd = cmd;

                    // enqueue the command
                    cp.EnqueueCommand(cmd);
                }
                else
                {
                    Debug.Assert(i == _propertyChain.Count - 1, "This should be the last property");

                    CreateFunctionScalarPropertyCommand cmd = null;
                    if (preReqCmd == null)
                    {
                        // create the FunctionScalarProperty command without any other properties in the property chain
                        cmd = new CreateFunctionScalarPropertyCommand(
                            _modificationFunction, property, _navPropPointingToProperty, _parameter, _version);
                    }
                    else
                    {
                        // create the FunctionScalarProperty command using the command for the previous property in the property chain
                        cmd = new CreateFunctionScalarPropertyCommand(preReqCmd, property, _navPropPointingToProperty, _parameter, _version);
                    }

                    cp.EnqueueCommand(cmd);
                    cp.Invoke();
                    _createdProperty = cmd.FunctionScalarProperty;
                    if (_createdProperty != null)
                    {
                        XmlModelHelper.NormalizeAndResolve(_createdProperty);
                    }
                    return;
                }
            }
        }

        internal FunctionScalarProperty FunctionScalarProperty
        {
            get { return _createdProperty; }
        }
    }
}
