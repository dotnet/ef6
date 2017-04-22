// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Represents the base item class for all the metadata
    /// </summary>
    public abstract partial class MetadataItem
    {
        // <summary>
        // Implementing this internal constructor so that this class can't be derived
        // outside this assembly
        // </summary>
        internal MetadataItem()
        {
        }

        internal MetadataItem(MetadataFlags flags)
        {
            _flags = (int)flags;
        }

        [Flags]
        internal enum MetadataFlags
        {
            // GlobalItem
            None = 0, // DataSpace flags are off by one so that zero can be the uninitialized state
            CSpace = 1, // (1 << 0)
            OSpace = 2, // (1 << 1)
            OCSpace = 3, // CSpace | OSpace
            SSpace = 4, // (1 << 2)
            CSSpace = 5, // CSpace | SSpace

            DataSpace = OSpace | CSpace | SSpace | OCSpace | CSSpace,

            // MetadataItem
            Readonly = (1 << 3),

            // EdmType
            IsAbstract = (1 << 4),

            // FunctionParameter
            In = (1 << 9),
            Out = (1 << 10),
            InOut = In | Out,
            ReturnValue = (1 << 11),

            ParameterMode = (In | Out | InOut | ReturnValue),
        }

        private int _flags;
        private MetadataPropertyCollection _itemAttributes;

        // <summary>
        // Gets the currently assigned annotations.
        // </summary>
        internal virtual IEnumerable<MetadataProperty> Annotations
        {
            get { return GetMetadataProperties().Where(p => p.IsAnnotation); }
        }

        /// <summary>Gets the built-in type kind for this type.</summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.BuiltInTypeKind" /> object that represents the built-in type kind for this type.
        /// </returns>
        public abstract BuiltInTypeKind BuiltInTypeKind { get; }

        /// <summary>Gets the list of properties of the current type.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Data.Entity.Core.Metadata.Edm.ReadOnlyMetadataCollection`1" /> that contains the list of properties of the current type.
        /// </returns>
        [MetadataProperty(BuiltInTypeKind.MetadataProperty, true)]
        public virtual ReadOnlyMetadataCollection<MetadataProperty> MetadataProperties
        {
            get { return GetMetadataProperties().AsReadOnlyMetadataCollection(); }
        }

        internal MetadataPropertyCollection GetMetadataProperties()
        {
             if (null == _itemAttributes)
             {
                 var itemAttributes = new MetadataPropertyCollection(this);
                 if (IsReadOnly)
                 {
                     itemAttributes.SetReadOnly();
                 }
                 Interlocked.CompareExchange(
                     ref _itemAttributes, itemAttributes, null);
             }
             return _itemAttributes;
        }

        /// <summary>
        /// Adds or updates an annotation with the specified name and value.
        /// </summary>
        /// <remarks>
        /// If an annotation with the given name already exists then the value of that annotation
        /// is updated to the given value. If the given value is null then the annotation will be
        /// removed.
        /// </remarks>
        /// <param name="name">The name of the annotation property.</param>
        /// <param name="value">The value of the annotation property.</param>
        public void AddAnnotation(string name, object value)
        {
            Check.NotEmpty(name, "name");

            var existingAnnotation = Annotations.FirstOrDefault(a => a.Name == name);

            if (existingAnnotation != null)
            {
                if (value == null)
                {
                    RemoveAnnotation(name);
                }
                else
                {
                    existingAnnotation.Value = value;
                }
            }
            else if (value != null)
            {
                GetMetadataProperties().Add(MetadataProperty.CreateAnnotation(name, value));
            } 
        }

        /// <summary>
        /// Removes an annotation with the specified name.
        /// </summary>
        /// <param name="name">The name of the annotation property.</param>
        /// <returns>true if an annotation was removed; otherwise, false.</returns>
        public bool RemoveAnnotation(string name)
        {
            Check.NotEmpty(name, "name");

            var metadataProperties = GetMetadataProperties();
            MetadataProperty property;

            return
                (metadataProperties.TryGetValue(name, false, out property))
                && metadataProperties.Remove(property);
        }

        // <summary>
        // List of item attributes on this type
        // </summary>
        internal MetadataCollection<MetadataProperty> RawMetadataProperties
        {
            get { return _itemAttributes; }
        }

        /// <summary>Gets or sets the documentation associated with this type.</summary>
        /// <returns>
        /// A <see cref="T:System.Data.Entity.Core.Metadata.Edm.Documentation" /> object that represents the documentation on this type.
        /// </returns>
        public Documentation Documentation { get; set; }

        // <summary>
        // Identity of the item
        // </summary>
        internal abstract String Identity { get; }

        // <summary>
        // Just checks for identities to be equal
        // </summary>
        internal virtual bool EdmEquals(MetadataItem item)
        {
            return ((null != item) &&
                    ((this == item) || // same reference
                     (BuiltInTypeKind == item.BuiltInTypeKind &&
                      Identity == item.Identity)));
        }

        // <summary>
        // Returns true if this item is not-changeable. Otherwise returns false.
        // </summary>
        internal bool IsReadOnly
        {
            get { return GetFlag(MetadataFlags.Readonly); }
        }

        // <summary>
        // Validates the types and sets the readOnly property to true. Once the type is set to readOnly,
        // it can never be changed.
        // </summary>
        internal virtual void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                if (null != _itemAttributes)
                {
                    _itemAttributes.SetReadOnly();
                }
                SetFlag(MetadataFlags.Readonly, true);
            }
        }

        // <summary>
        // Builds identity string for this item. By default, the method calls the identity property.
        // </summary>
        internal virtual void BuildIdentity(StringBuilder builder)
        {
            builder.Append(Identity);
        }

        // <summary>
        // Adds the given metadata property to the metadata property collection
        // </summary>
        internal void AddMetadataProperties(IEnumerable<MetadataProperty> metadataProperties)
        {
            GetMetadataProperties().AddRange(metadataProperties);
        }

        internal DataSpace GetDataSpace()
        {
            switch ((MetadataFlags)_flags & MetadataFlags.DataSpace)
            {
                default:
                    return (DataSpace)(-1);
                case MetadataFlags.CSpace:
                    return DataSpace.CSpace;
                case MetadataFlags.OSpace:
                    return DataSpace.OSpace;
                case MetadataFlags.SSpace:
                    return DataSpace.SSpace;
                case MetadataFlags.OCSpace:
                    return DataSpace.OCSpace;
                case MetadataFlags.CSSpace:
                    return DataSpace.CSSpace;
            }
        }

        internal void SetDataSpace(DataSpace space)
        {
            _flags = (int)(((MetadataFlags)_flags & ~MetadataFlags.DataSpace) | (MetadataFlags.DataSpace & Convert(space)));
        }

        private static MetadataFlags Convert(DataSpace space)
        {
            switch (space)
            {
                default:
                    return MetadataFlags.None; // invalid
                case DataSpace.CSpace:
                    return MetadataFlags.CSpace;
                case DataSpace.OSpace:
                    return MetadataFlags.OSpace;
                case DataSpace.SSpace:
                    return MetadataFlags.SSpace;
                case DataSpace.OCSpace:
                    return MetadataFlags.OCSpace;
                case DataSpace.CSSpace:
                    return MetadataFlags.CSSpace;
            }
        }

        internal ParameterMode GetParameterMode()
        {
            switch ((MetadataFlags)_flags & MetadataFlags.ParameterMode)
            {
                default:
                    return (ParameterMode)(-1); // invalid
                case MetadataFlags.In:
                    return ParameterMode.In;
                case MetadataFlags.Out:
                    return ParameterMode.Out;
                case MetadataFlags.InOut:
                    return ParameterMode.InOut;
                case MetadataFlags.ReturnValue:
                    return ParameterMode.ReturnValue;
            }
        }

        internal void SetParameterMode(ParameterMode mode)
        {
            _flags = (int)(((MetadataFlags)_flags & ~MetadataFlags.ParameterMode) | (MetadataFlags.ParameterMode & Convert(mode)));
        }

        private static MetadataFlags Convert(ParameterMode mode)
        {
            switch (mode)
            {
                default:
                    return MetadataFlags.ParameterMode; // invalid
                case ParameterMode.In:
                    return MetadataFlags.In;
                case ParameterMode.Out:
                    return MetadataFlags.Out;
                case ParameterMode.InOut:
                    return MetadataFlags.InOut;
                case ParameterMode.ReturnValue:
                    return MetadataFlags.ReturnValue;
            }
        }

        internal bool GetFlag(MetadataFlags flag)
        {
            return (flag == ((MetadataFlags)_flags & flag));
        }

        internal void SetFlag(MetadataFlags flag, bool value)
        {
            Debug.Assert(
                flag == MetadataFlags.Readonly
                || (flag & MetadataFlags.Readonly) != MetadataFlags.Readonly, 
                "SetFlag() invoked with Readonly and additional flags.");

            var spinWait = new SpinWait();
            do
            {
                var oldFlags = _flags;
                var newFlags = value ? (oldFlags | (int)flag) : (oldFlags & ~(int)flag);

                if (((MetadataFlags)oldFlags & MetadataFlags.Readonly) == MetadataFlags.Readonly)
                {
                    if ((flag & MetadataFlags.Readonly) == MetadataFlags.Readonly)
                    {
                        return;
                    }

                    throw new InvalidOperationException(Strings.OperationOnReadOnlyItem);
                }

                if (oldFlags == Interlocked.CompareExchange(ref _flags, newFlags, oldFlags))
                {
                    return;
                }

                spinWait.SpinOnce();
            }
            while (true);
        }
    }
}
