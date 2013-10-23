// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal abstract class ExplorerEntityType : EntityDesignExplorerEFElement
    {
        // class to be used when inserting ExplorerProperty objects to decide 
        // where they should live (key properties come before non-key ones)
        protected class ExplorerPropertyComparer : IComparer<ExplorerProperty>
        {
            public int Compare(ExplorerProperty property1, ExplorerProperty property2)
            {
                var isKey1 = property1.IsKeyProperty;
                var isKey2 = property2.IsKeyProperty;
                if (isKey1 != isKey2)
                {
                    if (isKey1)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }

                var name1 = property1.Name;
                var name2 = property2.Name;

                return string.Compare(name1, name2, false, CultureInfo.CurrentCulture);
            }
        }

        private readonly TypedChildList<ExplorerProperty> _properties =
            new TypedChildList<ExplorerProperty>(new ExplorerPropertyComparer());

        private readonly TypedChildList<ExplorerNavigationProperty> _navigationProperties =
            new TypedChildList<ExplorerNavigationProperty>();

        public ExplorerEntityType(EditingContext context, EntityType entityType, ExplorerEFElement parent)
            : base(context, entityType, parent)
        {
            // do nothing
        }

        public IList<ExplorerProperty> Properties
        {
            get { return _properties.ChildList; }
        }

        public IList<ExplorerNavigationProperty> NavigationProperties
        {
            get { return _navigationProperties.ChildList; }
        }

        private void LoadPropertiesFromModel()
        {
            // load Properties from model
            var entityType = ModelItem as EntityType;
            Debug.Assert(
                entityType != null, "Underlying EntityType is null calculating Properties for ExplorerEntityType with name " + Name);
            if (entityType != null)
            {
                foreach (var child in entityType.Properties())
                {
                    if (child.EntityModel.IsCSDL)
                    {
                        _properties.Insert(
                            (ExplorerConceptualProperty)
                            ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerConceptualProperty)));
                    }
                    else
                    {
                        _properties.Insert(
                            (ExplorerStorageProperty)
                            ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerStorageProperty)));
                    }
                }
            }
        }

        private void LoadNavigationPropertiesFromModel()
        {
            // load NavigationProperties from model
            var entityType = ModelItem as EntityType;
            Debug.Assert(
                entityType != null,
                "Underlying EntityType is null calculating NavigationProperties for ExplorerEntityType with name " + Name);
            var cet = entityType as ConceptualEntityType;
            if (cet != null)
            {
                foreach (var child in cet.NavigationProperties())
                {
                    _navigationProperties.Insert(
                        (ExplorerNavigationProperty)
                        ModelToExplorerModelXRef.GetNewOrExisting(_context, child, this, typeof(ExplorerNavigationProperty)));
                }
            }
        }

        protected override void LoadChildrenFromModel()
        {
            LoadPropertiesFromModel();

            // only load navigation properties for the C-side model
            var entityType = ModelItem as EntityType;
            if (entityType.EntityModel.IsCSDL)
            {
                LoadNavigationPropertiesFromModel();
            }
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();
            foreach (var property in Properties)
            {
                _children.Add(property);
            }

            foreach (var navigationProperty in NavigationProperties)
            {
                _children.Add(navigationProperty);
            }
        }

        protected override void InsertChild(EFElement efElementToInsert)
        {
            var prop = efElementToInsert as Property;
            if (prop != null)
            {
                var explorerProp = AddProperty(prop);
                var index = _properties.IndexOf(explorerProp);
                _children.Insert(index, explorerProp);
                return;
            }

            var navProp = efElementToInsert as NavigationProperty;
            if (navProp != null)
            {
                var explorerNavProp = AddNavigationProperty(navProp);
                var index = _navigationProperties.IndexOf(explorerNavProp);

                // need to add _properties.Count to index as the Properties
                // come first in the overall _children list
                _children.Insert(index + _properties.Count, explorerNavProp);
                return;
            }

            var key = efElementToInsert as Key;
            if (key != null)
            {
                // the ViewModel does not keep track of Key elements 
                // but it is not an error - so just return
                return;
            }

            base.InsertChild(efElementToInsert);
        }

        protected override bool RemoveChild(ExplorerEFElement efElementToRemove)
        {
            var explorerProperty = efElementToRemove as ExplorerProperty;
            if (explorerProperty != null)
            {
                var indexOfRemovedChild = _properties.Remove(explorerProperty);
                return (indexOfRemovedChild < 0) ? false : true;
            }

            var explorerNavigationProperty = efElementToRemove as ExplorerNavigationProperty;
            if (explorerNavigationProperty != null)
            {
                var indexOfRemovedChild = _navigationProperties.Remove(explorerNavigationProperty);
                return (indexOfRemovedChild < 0) ? false : true;
            }

            Debug.Fail(
                string.Format(
                    CultureInfo.CurrentCulture, Resources.BadRemoveBadChildType,
                    efElementToRemove.GetType().FullName, Name, GetType().FullName));
            return false;
        }

        private ExplorerProperty AddProperty(Property prop)
        {
            ExplorerProperty explorerProp = null;
            if (prop.EntityModel.IsCSDL)
            {
                explorerProp =
                    ModelToExplorerModelXRef.GetNew(_context, prop, this, typeof(ExplorerConceptualProperty)) as ExplorerConceptualProperty;
            }
            else
            {
                explorerProp =
                    ModelToExplorerModelXRef.GetNew(_context, prop, this, typeof(ExplorerStorageProperty)) as ExplorerStorageProperty;
            }
            _properties.Insert(explorerProp);
            return explorerProp;
        }

        private ExplorerNavigationProperty AddNavigationProperty(NavigationProperty navProp)
        {
            var explorerNavProp =
                ModelToExplorerModelXRef.GetNew(_context, navProp, this, typeof(ExplorerNavigationProperty)) as ExplorerNavigationProperty;
            _navigationProperties.Insert(explorerNavProp);
            return explorerNavProp;
        }
    }
}
