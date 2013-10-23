// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.ComponentModel.Composition;

    /// <summary>
    ///     Attribute used to specify that an Extension belongs to a particular layer
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EntityDesignerLayerAttribute : Attribute
    {
        private readonly string _layerName;

        /// <summary>
        ///     Creates an EntityDesignerLayerAttribute given a particular layer name
        /// </summary>
        /// <param name="layerName">Unique name specifying the layer (a logical collection of extensions)</param>
        public EntityDesignerLayerAttribute(string layerName)
        {
            _layerName = layerName;
        }

        /// <summary>
        ///     Unique name specifying the layer (a logical collection of extensions)
        /// </summary>
        public string LayerName
        {
            get { return _layerName; }
        }
    }
}
