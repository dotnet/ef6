// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Diagnostics.Contracts;

    public partial class ConventionsConfiguration
    {
        private class EdmConventionDispatcher : EdmModelVisitor
        {
            private readonly IConvention _convention;
            private readonly EdmModel _model;
            private readonly DataSpace _dataSpace;

            public EdmConventionDispatcher(IConvention convention, EdmModel model, DataSpace dataSpace = DataSpace.CSpace)
            {
                Contract.Requires(convention != null);
                Contract.Requires(model != null);

                _convention = convention;
                _model = model;
                _dataSpace = dataSpace;
            }

            public void Dispatch()
            {
                VisitEdmModel(_model);
            }

            private void Dispatch<TEdmDataModelItem>(TEdmDataModelItem item)
                where TEdmDataModelItem : MetadataItem
            {
                if (_dataSpace == DataSpace.CSpace)
                {
                    var convention = _convention as IEdmConvention<TEdmDataModelItem>;

                    if (convention != null)
                    {
                        convention.Apply(item, _model);
                    }
                }
                else if (_dataSpace == DataSpace.SSpace)
                {
                    var convention = _convention as IDbConvention<TEdmDataModelItem>;

                    if (convention != null)
                    {
                        convention.Apply(item, _model);
                    }
                }
            }

            protected override void VisitEdmModel(EdmModel item)
            {
                if (_dataSpace == DataSpace.CSpace)
                {
                    var convention = _convention as IEdmConvention;

                    if (convention != null)
                    {
                        convention.Apply(item);
                    }
                }
                else if (_dataSpace == DataSpace.SSpace)
                {
                    var convention = _convention as IDbConvention;

                    if (convention != null)
                    {
                        convention.Apply(item);
                    }
                }

                base.VisitEdmModel(item);
            }

            protected override void VisitEdmNavigationProperty(NavigationProperty item)
            {
                Dispatch(item);

                base.VisitEdmNavigationProperty(item);
            }

            protected override void VisitEdmAssociationConstraint(ReferentialConstraint item)
            {
                Dispatch(item);

                if (item != null)
                {
                    VisitMetadataItem(item);
                }
            }

            protected override void VisitEdmAssociationEnd(AssociationEndMember item)
            {
                Dispatch(item);

                base.VisitEdmAssociationEnd(item);
            }

            protected override void VisitEdmProperty(EdmProperty item)
            {
                Dispatch(item);

                base.VisitEdmProperty(item);
            }

            protected override void VisitMetadataItem(MetadataItem item)
            {
                Dispatch(item);

                base.VisitMetadataItem(item);
            }

            protected override void VisitEdmEntityContainer(EntityContainer item)
            {
                Dispatch(item);

                base.VisitEdmEntityContainer(item);
            }

            public override void VisitEdmEntitySet(EntitySet item)
            {
                Dispatch(item);

                base.VisitEdmEntitySet(item);
            }

            public override void VisitEdmAssociationSet(AssociationSet item)
            {
                Dispatch(item);

                base.VisitEdmAssociationSet(item);
            }

            protected override void VisitEdmAssociationSetEnd(EntitySet item)
            {
                Dispatch(item);

                base.VisitEdmAssociationSetEnd(item);
            }

            protected override void VisitEdmNamespace(EdmNamespace item)
            {
                Dispatch(item);

                base.VisitEdmNamespace(item);
            }

            protected override void VisitComplexType(ComplexType item)
            {
                Dispatch(item);

                base.VisitComplexType(item);
            }

            public override void VisitEdmEntityType(EntityType item)
            {
                Dispatch(item);

                VisitMetadataItem(item);

                if (item != null)
                {
                    VisitDeclaredProperties(item, item.DeclaredProperties);
                    VisitDeclaredNavigationProperties(item, item.DeclaredNavigationProperties);
                }
            }

            public override void VisitEdmAssociationType(AssociationType item)
            {
                Dispatch(item);

                base.VisitEdmAssociationType(item);
            }
        }
    }
}
