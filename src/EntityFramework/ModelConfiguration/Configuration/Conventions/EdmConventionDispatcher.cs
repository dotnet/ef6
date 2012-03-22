namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Internal;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Diagnostics.Contracts;

    public partial class ConventionsConfiguration
    {
        private class EdmConventionDispatcher : EdmModelVisitor
        {
            private readonly IConvention _convention;
            private readonly EdmModel _model;

            public EdmConventionDispatcher(IConvention convention, EdmModel model)
            {
                Contract.Requires(convention != null);
                Contract.Requires(model != null);

                _convention = convention;
                _model = model;
            }

            public void Dispatch()
            {
                VisitEdmModel(_model);
            }

            private void Dispatch<TEdmDataModelItem>(TEdmDataModelItem item)
                where TEdmDataModelItem : EdmDataModelItem
            {
                var convention = _convention as IEdmConvention<TEdmDataModelItem>;

                if (convention != null)
                {
                    convention.Apply(item, _model);
                }
            }

            protected override void VisitEdmModel(EdmModel item)
            {
                var convention = _convention as IEdmConvention;

                if (convention != null)
                {
                    convention.Apply(item);
                }

                base.VisitEdmModel(item);
            }

#if IncludeUnusedEdmCode
            protected override void VisitFunctionParameter(EdmFunctionParameter item)
            {
                Dispatch(item);

                base.VisitFunctionParameter(item);
            }
#endif

            protected override void VisitEdmNavigationProperty(EdmNavigationProperty item)
            {
                Dispatch(item);

                base.VisitEdmNavigationProperty(item);
            }

            protected override void VisitEdmAssociationConstraint(EdmAssociationConstraint item)
            {
                Dispatch(item);

                if (item != null)
                {
                    VisitEdmMetadataItem(item);
                }
            }

            protected override void VisitEdmAssociationEnd(EdmAssociationEnd item)
            {
                Dispatch(item);

                base.VisitEdmAssociationEnd(item);
            }

            protected override void VisitEdmProperty(EdmProperty item)
            {
                Dispatch(item);

                base.VisitEdmProperty(item);
            }

            protected override void VisitEdmDataModelItem(EdmDataModelItem item)
            {
                Dispatch(item);

                base.VisitEdmDataModelItem(item);
            }

            protected override void VisitEdmMetadataItem(EdmMetadataItem item)
            {
                Dispatch(item);

                base.VisitEdmMetadataItem(item);
            }

            protected override void VisitEdmNamedMetadataItem(EdmNamedMetadataItem item)
            {
                Dispatch(item);

                base.VisitEdmNamedMetadataItem(item);
            }

            protected override void VisitEdmNamespaceItem(EdmNamespaceItem item)
            {
                Dispatch(item);

                base.VisitEdmNamespaceItem(item);
            }

            protected override void VisitEdmEntityContainer(EdmEntityContainer item)
            {
                Dispatch(item);

                base.VisitEdmEntityContainer(item);
            }

            protected override void VisitEdmEntitySet(EdmEntitySet item)
            {
                Dispatch(item);

                base.VisitEdmEntitySet(item);
            }

            protected override void VisitEdmAssociationSet(EdmAssociationSet item)
            {
                Dispatch(item);

                base.VisitEdmAssociationSet(item);
            }

            protected override void VisitEdmAssociationSetEnd(EdmEntitySet item)
            {
                Dispatch(item);

                base.VisitEdmAssociationSetEnd(item);
            }

#if IncludeUnusedEdmCode
            protected override void VisitFunctionImport(EdmFunctionImport item)
            {
                Dispatch(item);

                base.VisitFunctionImport(item);
            }
#endif

            protected override void VisitEdmNamespace(EdmNamespace item)
            {
                Dispatch(item);

                base.VisitEdmNamespace(item);
            }

            protected override void VisitComplexType(EdmComplexType item)
            {
                Dispatch(item);

                base.VisitComplexType(item);
            }

            protected override void VisitEdmEntityType(EdmEntityType item)
            {
                Dispatch(item);

                VisitEdmNamedMetadataItem(item);

                if (item != null)
                {
                    VisitDeclaredProperties(item, item.DeclaredProperties);
                    VisitDeclaredNavigationProperties(item, item.DeclaredNavigationProperties);
                }
            }

            protected override void VisitEdmAssociationType(EdmAssociationType item)
            {
                Dispatch(item);

                base.VisitEdmAssociationType(item);
            }

#if IncludeUnusedEdmCode
           protected override void VisitEdmFunctionGroup(EdmFunctionGroup item)
            {
                Dispatch(item);

                base.VisitEdmFunctionGroup(item);
            }
#endif
        }
    }
}
