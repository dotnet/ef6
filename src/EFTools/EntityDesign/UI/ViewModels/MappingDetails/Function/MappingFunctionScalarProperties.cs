// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    [TreeGridDesignerRootBranch(typeof(ParametersBranch))]
    [TreeGridDesignerColumn(typeof(ParameterColumn), Order = 1)]
    internal class MappingFunctionScalarProperties : MappingFunctionMappingRoot
    {
        private IList<MappingFunctionScalarProperty> _scalarProperties;

        public MappingFunctionScalarProperties(EditingContext context, ModificationFunction functionMapping, MappingEFElement parent)
            : base(context, functionMapping, parent)
        {
        }

        internal ModificationFunction ModificationFunction
        {
            get { return ModelItem as ModificationFunction; }
        }

        internal Function Function
        {
            get
            {
                if (ModificationFunction == null)
                {
                    return null;
                }
                else
                {
                    return ModificationFunction.FunctionName.Target;
                }
            }
        }

        internal override string Name
        {
            get { return Resources.MappingDetails_Parameters; }
        }

        // we override this property because we don't want to use the base setter; otherwise
        // we'll replace the XRef for the function so that it points here and not to the 
        // MappingModificationFunctionMapping (our parent)
        internal override EFElement ModelItem
        {
            get { return _modelItem; }
            set
            {
                _modelItem = value;
                _isDisposed = false;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal void LoadScalarProperties()
        {
            if (null == _scalarProperties)
            {
                _scalarProperties = new List<MappingFunctionScalarProperty>();

                // load children from model
                // note: have to go to parent to get this as this is a dummy node
                if (Function != null
                    && MappingFunctionEntityType != null
                    && MappingFunctionEntityType.EntityType != null)
                {
                    var entityType = MappingFunctionEntityType.EntityType;

                    // loop through all of the 'parameters' in the function
                    foreach (var parm in Function.Parameters())
                    {
                        FunctionScalarProperty existingScalarProperty = null;

                        // for each column, see if we are already have a scalar property
                        var antiDeps = parm.GetAntiDependenciesOfType<FunctionScalarProperty>();
                        foreach (var scalarProperty in antiDeps)
                        {
                            // this FunctionScalarProperty could be right under the function, nested inside an AssociationEnd, 
                            // or N levels deep inside a complex type hierarchy
                            var spmf = scalarProperty.GetParentOfType(typeof(ModificationFunction)) as ModificationFunction;

                            // if we find one, validate it
                            if (scalarProperty != null
                                && scalarProperty.Name.Status == BindingStatus.Known
                                && spmf != null
                                && ModificationFunction == spmf)
                            {
                                // make sure we are looking at something mapped by the entity type we are mapping
                                if (entityType != spmf.ModificationFunctionMapping.EntityTypeMapping.FirstBoundConceptualEntityType)
                                {
                                    continue;
                                }

                                // we are already mapping this 
                                existingScalarProperty = scalarProperty;
                                break;
                            }
                        }

                        // if we didn't find one, then create a dummy row with just the column info
                        if (existingScalarProperty == null)
                        {
                            var msp = new MappingFunctionScalarProperty(_context, null, this);
                            msp.StoreParameter = parm;
                            _scalarProperties.Add(msp);
                        }
                        else
                        {
                            var msp =
                                (MappingFunctionScalarProperty)
                                ModelToMappingModelXRef.GetNewOrExisting(_context, existingScalarProperty, this);
                            _scalarProperties.Add(msp);
                        }
                    }
                }
            }
        }

        [DebuggerDisplay("ScalarProperties must not be invoked by Debugger just to show its value in the Autos Window")]
        internal IList<MappingFunctionScalarProperty> ScalarProperties
        {
            get
            {
                LoadScalarProperties();
                return _scalarProperties;
            }
        }

        protected override void LoadChildrenCollection()
        {
            LoadScalarProperties();
            _children.Clear();
            foreach (var child in ScalarProperties)
            {
                _children.Add(child);
            }
        }

        protected override void OnChildDeleted(MappingEFElement melem)
        {
            var child = melem as MappingFunctionScalarProperty;
            Debug.Assert(child != null, "Unknown child being deleted");
            if (child != null)
            {
                _children.Remove(child);
                return;
            }

            base.OnChildDeleted(melem);
        }
    }
}
