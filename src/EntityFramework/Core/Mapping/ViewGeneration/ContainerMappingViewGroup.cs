// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Provides the names of the store container and the model container that are part of a container mapping, 
    /// a mapping closure hash used for validation, and the ESQL views corresponding to each entity set in the container.
    /// </summary>
    public sealed class ContainerMappingViewGroup
    {
        private readonly string _storeContainerName;
        private readonly string _modelContainerName;
        private readonly string _mappingHash;
        private readonly Dictionary<EntitySetBase, string> _views;

        internal ContainerMappingViewGroup(            
            string storeContainerName,
            string modelContainerName,
            string mappingHash,
            Dictionary<EntitySetBase, string> views)
        {            
            _storeContainerName = storeContainerName;
            _modelContainerName = modelContainerName;
            _mappingHash = mappingHash;
            _views = views;
        }

        /// <summary>
        /// Gets the name of the store container.
        /// </summary>
        public string StoreContainerName 
        {
            get { return _storeContainerName; }
        }

        /// <summary>
        /// Gets the name of the model container.
        /// </summary>
        public string ModelContainerName 
        {
            get { return _modelContainerName; }
        }

        /// <summary>
        /// Gets the mapping closure hash used for validation.
        /// </summary>
        public string MappingHash 
        {
            get { return _mappingHash; }
        }

        /// <summary>
        /// Gets a dictionary that maps the entity sets from a container mapping 
        /// to the corresponding ESQL views.
        /// </summary>
        public IDictionary<EntitySetBase, string> Views
        {
            get { return _views; }
        }
    }
}
