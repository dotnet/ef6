// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Index
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    // <summary>
    // Used to configure indexing of properties of an entity type or complex type.
    // </summary>
    internal class IndexConfiguration : PropertyConfiguration
    {
        private bool? _isUnique;
        private bool? _isClustered;
        private string _name;

        public IndexConfiguration()
        {

        }

        internal IndexConfiguration(IndexConfiguration source)
        {
            DebugCheck.NotNull(source);

            _isUnique = source._isUnique;
            _isClustered = source._isClustered;
            _name = source._name;
        }


        public bool? IsUnique
        {
            get
            {
                return _isUnique;
            }

            set
            {
                Check.NotNull(value, "value");

                _isUnique = value;
            }
        }

        public bool? IsClustered 
        { 
            get
            {
                return _isClustered;
            }
            set
            {
                Check.NotNull(value, "value");

                _isClustered = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                Check.NotNull(value, "value");

                _name = value;
            }
        }


        internal virtual IndexConfiguration Clone()
        {
            return new IndexConfiguration(this);
        }

        internal void Configure(EdmProperty edmProperty, int indexOrder)
        {
            DebugCheck.NotNull(edmProperty);

            AddAnnotationWithMerge(edmProperty,
                new IndexAnnotation(new IndexAttribute(_name, indexOrder, _isClustered, _isUnique)));
        }

        internal void Configure(EntityType entityType)
        {
            DebugCheck.NotNull(entityType);

            AddAnnotationWithMerge(entityType,
                new IndexAnnotation(new IndexAttribute(_name, _isClustered, _isUnique)));
        }

        private static void AddAnnotationWithMerge(MetadataItem metadataItem, IndexAnnotation newAnnotation)
        {
            var existingAnnotation = metadataItem.Annotations.GetAnnotation(XmlConstants.IndexAnnotationWithPrefix);

            if (existingAnnotation != null)
            {
                newAnnotation = (IndexAnnotation)((IndexAnnotation)existingAnnotation).MergeWith(newAnnotation);
            }

            metadataItem.AddAnnotation(XmlConstants.IndexAnnotationWithPrefix, newAnnotation);
        }
    }
}
