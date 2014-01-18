// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Functions
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;

    // <summary>
    //     This class represents the root node of the view model when we are mapping entities to functions,
    //     it points to a c-side entity and has a list of the functions that the entity is mapped to.
    //     + MappingFunctionEntityType
    //     |
    //     + MappingModificationFunctionMapping (insert, update, delete)
    //     |
    //     + MappingFunctionScalarProperties
    //     | |
    //     | + MappingFunctionScalarProperty
    //     |
    //     + MappingResultBindings
    //     |
    //     + MappingResultBinding
    // </summary>
    [TreeGridDesignerRootBranch(typeof(FunctionBranch))]
    [TreeGridDesignerColumn(typeof(ParameterColumn), Order = 1)]
    [TreeGridDesignerColumn(typeof(OperatorColumn), Order = 2)]
    [TreeGridDesignerColumn(typeof(PropertyColumn), Order = 3)]
    [TreeGridDesignerColumn(typeof(UseOriginalValueColumn), Order = 4)]
    [TreeGridDesignerColumn(typeof(RowsAffectedParameterColumn), Order = 5)]
    internal class MappingFunctionEntityType : MappingFunctionMappingRoot
    {
        private MappingModificationFunctionMapping _insertMapping;
        private MappingModificationFunctionMapping _updateMapping;
        private MappingModificationFunctionMapping _deleteMapping;

        public MappingFunctionEntityType(EditingContext context, EntityType entityType, MappingEFElement parent)
            : base(context, entityType, parent)
        {
            _insertMapping = new MappingModificationFunctionMapping(context, null, this, ModificationFunctionType.Insert);
            _updateMapping = new MappingModificationFunctionMapping(context, null, this, ModificationFunctionType.Update);
            _deleteMapping = new MappingModificationFunctionMapping(context, null, this, ModificationFunctionType.Delete);
        }

        internal EntityType EntityType
        {
            get { return ModelItem as EntityType; }
        }

        internal MappingModificationFunctionMapping InsertFunction
        {
            get { return _insertMapping; }
        }

        internal MappingModificationFunctionMapping UpdateFunction
        {
            get { return _updateMapping; }
        }

        internal MappingModificationFunctionMapping DeleteFunction
        {
            get { return _deleteMapping; }
        }

        protected override void LoadChildrenCollection()
        {
            if (EntityType != null)
            {
                // loop through every EntityTypeMapping that has a dep on this c-side entity,
                // looking to see if there in a function mapping node
                foreach (var etm in EntityType.GetAntiDependenciesOfType<EntityTypeMapping>())
                {
                    if (etm.Kind == EntityTypeMappingKind.Function)
                    {
                        if (etm.ModificationFunctionMapping.InsertFunction != null
                            && etm.ModificationFunctionMapping.InsertFunction.FunctionName.Status == BindingStatus.Known)
                        {
                            _insertMapping =
                                (MappingModificationFunctionMapping)
                                ModelToMappingModelXRef.GetNewOrExisting(_context, etm.ModificationFunctionMapping.InsertFunction, this);
                            _insertMapping.ModificationFunctionType = ModificationFunctionType.Insert;
                        }

                        if (etm.ModificationFunctionMapping.UpdateFunction != null
                            && etm.ModificationFunctionMapping.UpdateFunction.FunctionName.Status == BindingStatus.Known)
                        {
                            _updateMapping =
                                (MappingModificationFunctionMapping)
                                ModelToMappingModelXRef.GetNewOrExisting(_context, etm.ModificationFunctionMapping.UpdateFunction, this);
                            _updateMapping.ModificationFunctionType = ModificationFunctionType.Update;
                        }

                        if (etm.ModificationFunctionMapping.DeleteFunction != null
                            && etm.ModificationFunctionMapping.DeleteFunction.FunctionName.Status == BindingStatus.Known)
                        {
                            _deleteMapping =
                                (MappingModificationFunctionMapping)
                                ModelToMappingModelXRef.GetNewOrExisting(_context, etm.ModificationFunctionMapping.DeleteFunction, this);
                            _deleteMapping.ModificationFunctionType = ModificationFunctionType.Delete;
                        }

                        break;
                    }
                }
            }

            _children.Add(_insertMapping);
            _children.Add(_updateMapping);
            _children.Add(_deleteMapping);
        }

        protected override void OnChildDeleted(MappingEFElement melem)
        {
            var child = melem as MappingModificationFunctionMapping;
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
