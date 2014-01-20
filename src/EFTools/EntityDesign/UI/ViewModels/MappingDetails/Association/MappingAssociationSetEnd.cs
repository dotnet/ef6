// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.Associations
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.Model.Mapping.ChildCollectionBuilders;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;

    [TreeGridDesignerRootBranch(typeof(AssociationSetEndBranch))]
    [TreeGridDesignerColumn(typeof(PropertyColumn), Order = 1)]
    internal class MappingAssociationSetEnd : MappingAssociationMappingRoot
    {
        private IList<MappingEndScalarProperty> _scalarProperties;

        public MappingAssociationSetEnd(EditingContext context, AssociationSetEnd end, MappingEFElement parent)
            : base(context, end, parent)
        {
        }

        internal AssociationSetEnd AssociationSetEnd
        {
            get { return ModelItem as AssociationSetEnd; }
        }

        internal AssociationEnd AssociationEnd
        {
            get
            {
                if (AssociationSetEnd != null
                    &&
                    AssociationSetEnd.Role.Status == BindingStatus.Known)
                {
                    return AssociationSetEnd.Role.Target;
                }

                return null;
            }
        }

        internal ConceptualEntityType ConceptualEntityType
        {
            get
            {
                if (AssociationEnd != null)
                {
                    var cet = AssociationEnd.Type.Target as ConceptualEntityType;
                    Debug.Assert(AssociationEnd.Type.Target != null ? cet != null : true, "EntityType is not ConceptualEntityType");
                    return cet;
                }

                return null;
            }
        }

        internal override string Name
        {
            get
            {
                if (AssociationEnd != null
                    &&
                    AssociationEnd.Role.Value != null)
                {
                    return AssociationEnd.Role.Value;
                }

                return string.Empty;
            }
        }

        internal IList<MappingEndScalarProperty> ScalarProperties
        {
            get
            {
                _scalarProperties = new List<MappingEndScalarProperty>();

                if (AssociationEnd != null
                    &&
                    AssociationEnd.Type.Status == BindingStatus.Known)
                {
                    var cpc = new CommandProcessorContext(_context, "MappingAssociationSetEnd", "ScalarProperties");
                    var builder = new AssociationSetEndMappingBuilderForViewModel(
                        AssociationSetEnd, MappingAssociationSet.StorageEntityType, this);
                    builder.Build(cpc);
                }

                return _scalarProperties;
            }
        }

        protected override void LoadChildrenCollection()
        {
            foreach (var child in ScalarProperties)
            {
                _children.Add(child);
            }
        }

        // <summary>
        //     A private class for doing intelli-match with association set ends.  If the builder calls BuildNew(), we create a
        //     "dummy node" in our view model for this column.  If the builder calls BuildExisting() then create a view model item
        //     that has a connection to the existing model item.
        // </summary>
        private class AssociationSetEndMappingBuilderForViewModel : AssociationSetEndMappingBuilder
        {
            private readonly MappingAssociationSetEnd _mase;

            internal AssociationSetEndMappingBuilderForViewModel(
                AssociationSetEnd setEnd, StorageEntityType storeEntityType, MappingAssociationSetEnd mase)
                : base(setEnd, storeEntityType)
            {
                _mase = mase;
            }

            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
            protected override void BuildNew(CommandProcessorContext cpc, string propertyName, string propertyType)
            {
                var mesp = new MappingEndScalarProperty(cpc.EditingContext, null, _mase);
                mesp.Property = propertyName;
                mesp.PropertyType = propertyType;
                _mase._scalarProperties.Add(mesp);
            }

            protected override void BuildExisting(CommandProcessorContext cpc, ScalarProperty scalarProperty)
            {
                var mesp = (MappingEndScalarProperty)ModelToMappingModelXRef.GetNewOrExisting(cpc.EditingContext, scalarProperty, _mase);
                _mase._scalarProperties.Add(mesp);
            }
        }
    }
}
