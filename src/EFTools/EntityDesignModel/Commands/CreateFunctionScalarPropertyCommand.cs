// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to create a ScalarProperty that lives in a ModificationFunction.  This is different
    ///     than those ScalarProperties that can be added to an EndProperty or MappingFragment.
    ///     Note: this only creates the ScalarProperty itself and probably should not be used directly, instead
    ///     use CreateFunctionScalarPropertyTreeCommand to create the whole tree.
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
    ///     NOTE: These ScalarProperties can be either directly under the function mapping, or inside
    ///     an AssociationEnd or inside a ComplexProperty.
    /// </summary>
    internal class CreateFunctionScalarPropertyCommand : Command
    {
        internal static readonly string PrereqId = "CreateFunctionComplexPropertyCommand";

        private FunctionScalarProperty _sp;
        private readonly Property _property;
        private readonly NavigationProperty _pointingNavProperty;
        private readonly Parameter _parm;
        private readonly string _version;
        private readonly ModificationFunction _parentModFunc;
        private FunctionComplexProperty _parentComplexProperty;

        /// <summary>
        ///     Creates a ScalarProperty element that maps the passed in Property to the Parameter, inside
        ///     the parent ModificationFunction.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="navPropPointingToProperty"></param>
        /// <param name="parm"></param>
        /// <param name="version">Optional</param>
        internal CreateFunctionScalarPropertyCommand(
            ModificationFunction parentModificationFunction,
            Property property, NavigationProperty navPropPointingToProperty, Parameter parm, string version)
        {
            CommandValidation.ValidateProperty(property);
            CommandValidation.ValidateParameter(parm);
            CommandValidation.ValidateModificationFunction(parentModificationFunction);

            // we don't require a non-null navigation property since the function scalar property could exist
            // directly in its parent 
            if (navPropPointingToProperty != null)
            {
                CommandValidation.ValidateNavigationProperty(navPropPointingToProperty);
            }

            _property = property;
            _pointingNavProperty = navPropPointingToProperty;
            _parm = parm;
            _version = version;
            _parentModFunc = parentModificationFunction;
        }

        /// <summary>
        ///     Creates a ScalarProperty element that maps the passed in Property to the Parameter, inside
        ///     the ComplexProperty given by the passed in CreateFunctionComplexPropertyCommand pre-req Command.
        /// </summary>
        /// <param name="prereq"></param>
        /// <param name="property"></param>
        /// <param name="navPropPointingToProperty"></param>
        /// <param name="parm"></param>
        /// <param name="version">Optional</param>
        internal CreateFunctionScalarPropertyCommand(
            CreateFunctionComplexPropertyCommand prereq, Property property,
            NavigationProperty navPropPointingToProperty, Parameter parm, string version)
            : base(PrereqId)
        {
            ValidatePrereqCommand(prereq);
            CommandValidation.ValidateProperty(property);
            CommandValidation.ValidateParameter(parm);

            // we don't require a non-null navigation property since the function scalar property could exist
            // directly in its parent 
            if (navPropPointingToProperty != null)
            {
                CommandValidation.ValidateNavigationProperty(navPropPointingToProperty);
            }

            _property = property;
            _pointingNavProperty = navPropPointingToProperty;
            _parm = parm;
            _version = version;
            _parentModFunc = null; // parent should be a ComplexProperty from the prereq Command

            AddPreReqCommand(prereq);
        }

        protected override void ProcessPreReqCommands()
        {
            var prereq = GetPreReqCommand(CreateFunctionComplexPropertyCommand.PrereqId) as CreateFunctionComplexPropertyCommand;
            if (prereq != null)
            {
                _parentComplexProperty = prereq.FunctionComplexProperty;
                CommandValidation.ValidateFunctionComplexProperty(_parentComplexProperty);
                Debug.Assert(_parentComplexProperty != null, "We didn't get a good FunctionComplexProperty out of the pre-req Command");
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "parm")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(_property != null && _parm != null, "InvokeInternal is called when _property or _parm is null");
            if (_property == null
                || _parm == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when _property or _parm is null");
            }

            // check ModificationFunction or ComplexProperty parent exists
            Debug.Assert(
                _parentModFunc != null || _parentComplexProperty != null,
                "Must have either a ModificationFunction or a FunctionComplexProperty parent to house this ScalarProperty");
            if (_parentModFunc == null
                && _parentComplexProperty == null)
            {
                throw new CannotLocateParentItemException();
            }

            // check both ModificationFunction and ComplexProperty parents don't exist
            Debug.Assert(
                _parentModFunc == null || _parentComplexProperty == null,
                "Must not have both a ModificationFunction and a FunctionComplexProperty parent to house this ScalarProperty");
            if (_parentModFunc != null
                && _parentComplexProperty != null)
            {
                throw new CannotLocateParentItemException();
            }

            // now create it, either directly under this function mapping, or inside an AssociationEnd
            // if the entity property is via a NavProp, or inside the parent ComplexProperty
            if (_parentModFunc != null)
            {
                if (_pointingNavProperty == null)
                {
                    // we can directly add a mapping
                    _sp = CreateFunctionScalarPropertyCommon(_parentModFunc, _property, _parm, _version);
                }
                else
                {
                    // create an AssociationEnd and then add it
                    _sp = CreateFunctionScalarPropertyInAssociationEnd(_parentModFunc, _property, _pointingNavProperty, _parm, _version);
                }
            }
            else if (_parentComplexProperty != null)
            {
                // should not have _pointingNavProperty if we have a FunctionComplexProperty parent
                Debug.Assert(
                    _pointingNavProperty == null,
                    "We're creating a FunctionScalarProperty within a FunctionComplexProperty - but _pointingNavProperty is non-null!");

                if (_pointingNavProperty == null)
                {
                    _sp = CreateFunctionScalarPropertyCommon(_parentComplexProperty, _property, _parm, _version);
                }
            }

            if (_sp == null)
            {
                throw new ItemCreationFailureException();
            }

            Debug.Assert(
                _sp.Name.Target != null && _sp.Name.Target.LocalName.Value == _sp.Name.RefName,
                (_sp.Name.Target == null
                     ? "Broken entity property resolution - Target is null"
                     : "Broken entity property resolution - Target.LocalName.Value (" + _sp.Name.Target.LocalName.Value + ") != RefName ("
                       + _sp.Name.RefName + ")"));

            Debug.Assert(
                _sp.ParameterName.Target != null && _sp.ParameterName.Target.LocalName.Value == _sp.ParameterName.RefName,
                (_sp.ParameterName.Target == null
                     ? "Broken parameter resolution - Target is null"
                     : "Broken parameter resolution - Target.LocalName.Value (" + _sp.ParameterName.Target.LocalName.Value + ") != RefName ("
                       + _sp.ParameterName.RefName + ")"));
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static FunctionScalarProperty CreateFunctionScalarPropertyInAssociationEnd(
            ModificationFunction mf, Property entityProperty, NavigationProperty pointingNavProperty, Parameter parm, string version)
        {
            // in creating the function scalar property, we modify the AssociationEnd depending on the navigation property that is
            // pointing to the actual property. If we don't have this navigation property we can't do anything.
            Debug.Assert(
                pointingNavProperty != null,
                "We need the navigation property pointing to the property in order to create the mapping function scalar property");
            if (pointingNavProperty == null)
            {
                throw new CannotLocateReferencedItemException();
            }

            Debug.Assert(pointingNavProperty.Relationship.Target != null, "Where is the Association for this navigation property?");
            if (pointingNavProperty.Relationship.Target == null)
            {
                throw new CannotLocateReferencedItemException();
            }

            var assocSet = pointingNavProperty.Relationship.Target.AssociationSet;
            var navPropFromEnd = pointingNavProperty.FromRole.Target;
            var navPropToEnd = pointingNavProperty.ToRole.Target;
            Debug.Assert(null != navPropFromEnd, "Null FromRole for pointingNavProperty " + pointingNavProperty.ToPrettyString());
            Debug.Assert(null != navPropToEnd, "Null ToRole for pointingNavProperty " + pointingNavProperty.ToPrettyString());

            AssociationSetEnd assocSetFromEnd = null;
            AssociationSetEnd assocSetToEnd = null;

            // figure which end is which
            // Note: it is valid for the NavigationProperty to point to
            // an EntityType in the same inheritance hierarchy
            foreach (var end in assocSet.AssociationSetEnds())
            {
                if (end.Role.Target == navPropFromEnd)
                {
                    Debug.Assert(
                        null == assocSetFromEnd,
                        "pointingNavProperty From End " + navPropFromEnd.ToPrettyString()
                        + " matches more than 1 AssociationSetEnd for AssociationSet " + assocSet.ToPrettyString());
                    assocSetFromEnd = end;
                }
                else if (end.Role.Target == navPropToEnd)
                {
                    Debug.Assert(
                        null == assocSetToEnd,
                        "pointingNavProperty To End " + navPropToEnd.ToPrettyString()
                        + " matches more than 1 AssociationSetEnd for AssociationSet " + assocSet.ToPrettyString());
                    assocSetToEnd = end;
                }
            }
            Debug.Assert(null != assocSetFromEnd, "Cannot find From end of AssociationSet " + assocSet.ToPrettyString());
            Debug.Assert(null != assocSetToEnd, "Cannot find To end of AssociationSet " + assocSet.ToPrettyString());

            // see if we already have this AssociationEnd
            FunctionAssociationEnd fae = null;
            foreach (var funcAssocEnd in mf.AssociationEnds())
            {
                if (funcAssocEnd.AssociationSet.Target == assocSet
                    && funcAssocEnd.From.Target == assocSetFromEnd
                    && funcAssocEnd.To.Target == assocSetToEnd)
                {
                    fae = funcAssocEnd;
                    break;
                }
            }

            // create the association end if needed
            if (fae == null)
            {
                fae = new FunctionAssociationEnd(mf, null);
                fae.AssociationSet.SetRefName(assocSet);
                fae.From.SetRefName(assocSetFromEnd);
                fae.To.SetRefName(assocSetToEnd);
                mf.AddAssociationEnd(fae);
                XmlModelHelper.NormalizeAndResolve(fae);
            }

            Debug.Assert(fae != null, "Failed to create the AssociationEnd to house this ScalarProperty");
            if (fae == null)
            {
                throw new ParentItemCreationFailureException();
            }

            // create the SP inside this
            return CreateFunctionScalarPropertyCommon(fae, entityProperty, parm, version);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static FunctionScalarProperty CreateFunctionScalarPropertyCommon(
            EFElement parent, Property property, Parameter parm, string version)
        {
            var fsp = new FunctionScalarProperty(parent, null);
            fsp.Name.SetRefName(property);
            fsp.ParameterName.SetRefName(parm);
            if (string.IsNullOrEmpty(version) == false)
            {
                fsp.Version.Value = version;
            }

            var mf = parent as ModificationFunction;
            var fae = parent as FunctionAssociationEnd;
            var fcp = parent as FunctionComplexProperty;
            if (mf != null)
            {
                mf.AddScalarProperty(fsp);
            }
            else if (fae != null)
            {
                fae.AddScalarProperty(fsp);
            }
            else if (fcp != null)
            {
                fcp.AddScalarProperty(fsp);
            }
            else
            {
                Debug.Fail(
                    "Unknown parent type (" + parent.GetType().FullName
                    + ") sent to CreateFunctionScalarPropertyCommand.CreateFunctionScalarPropertyCommon()");
            }

            XmlModelHelper.NormalizeAndResolve(fsp);

            return fsp;
        }

        /// <summary>
        ///     Returns the FunctionScalarProperty created by this command
        /// </summary>
        internal FunctionScalarProperty FunctionScalarProperty
        {
            get { return _sp; }
        }
    }
}
