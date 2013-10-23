// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    [TreeGridDesignerRootBranch(typeof(ResultBindingsBranch))]
    [TreeGridDesignerColumn(typeof(ParameterColumn), Order = 1)]
    internal class MappingResultBindings : MappingFunctionMappingRoot
    {
        private IList<MappingResultBinding> _resultBindings;

        public MappingResultBindings(EditingContext context, ModificationFunction functionMapping, MappingEFElement parent)
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
            get { return Resources.MappingDetails_ResultBindings; }
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

        internal void LoadResultBindings()
        {
            if (null == _resultBindings)
            {
                _resultBindings = new List<MappingResultBinding>();

                if (Function != null
                    && MappingFunctionEntityType != null
                    && MappingFunctionEntityType.EntityType != null)
                {
                    var entityType = MappingFunctionEntityType.EntityType as ConceptualEntityType;
                    Debug.Assert(
                        MappingFunctionEntityType.EntityType == null || entityType != null, "EntityType is not ConceptualEntityType");

                    foreach (var prop in entityType.SafeInheritedAndDeclaredProperties)
                    {
                        foreach (var binding in prop.GetAntiDependenciesOfType<ResultBinding>())
                        {
                            // if we find one, validate it
                            if (binding != null
                                && binding.Name.Status == BindingStatus.Known
                                && binding.Name.Target != null)
                            {
                                // make sure we are looking at something mapped by the entity type we are mapping, for the same function, in the same MF
                                if (entityType
                                    != binding.ModificationFunction.ModificationFunctionMapping.EntityTypeMapping
                                           .FirstBoundConceptualEntityType
                                    || MappingModificationFunctionMapping.Function != binding.ModificationFunction.FunctionName.Target
                                    || ModificationFunction != binding.ModificationFunction)
                                {
                                    continue;
                                }

                                // if not, see if its already mapped by this type or its base types
                                var bindingEntityType = binding.Name.Target.Parent as EntityType;
                                var cet = bindingEntityType as ConceptualEntityType;
                                Debug.Assert(bindingEntityType != null ? cet != null : true, "EntityType is not ConceptualEntityType");

                                Debug.Assert(cet != null, "Parent of Property should be EntityType");
                                if (cet != null
                                    &&
                                    cet.GetSafeSelfAndBaseTypesAsHashSet().Contains(cet))
                                {
                                    // we are already mapping this 
                                    var mrb = (MappingResultBinding)ModelToMappingModelXRef.GetNewOrExisting(_context, binding, this);
                                    _resultBindings.Add(mrb);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal IList<MappingResultBinding> ResultBindings
        {
            get
            {
                LoadResultBindings();
                return _resultBindings;
            }
        }

        protected override void LoadChildrenCollection()
        {
            LoadResultBindings();
            _children.Clear();
            foreach (var child in ResultBindings)
            {
                _children.Add(child);
            }
        }

        protected override void OnChildDeleted(MappingEFElement melem)
        {
            var child = melem as MappingResultBinding;
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
