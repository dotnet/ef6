// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    ///     Represent the Edm Complex Type
    /// </summary>
    public class ComplexType : StructuralType
    {
        /// <summary>
        ///     Initializes a new instance of Complex Type with the given properties
        /// </summary>
        /// <param name="name"> The name of the complex type </param>
        /// <param name="namespaceName"> The namespace name of the type </param>
        /// <param name="version"> The version of this type </param>
        /// <param name="dataSpace"> dataSpace in which this ComplexType belongs to </param>
        /// <exception cref="System.ArgumentNullException">If either name, namespace or version arguments are null</exception>
        internal ComplexType(string name, string namespaceName, DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
        }

        /// <summary>
        ///     Initializes a new instance of Complex Type - required for bootstraping code
        /// </summary>
        internal ComplexType()
        {
            // No initialization of item attributes in here, it's used as a pass thru in the case for delay population
            // of item attributes
        }

        private ReadOnlyMetadataCollection<EdmProperty> _properties;

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.ComplexType; }
        }

        /// <summary>
        ///     Returns just the properties from the collection
        ///     of members on this type
        /// </summary>
        public virtual ReadOnlyMetadataCollection<EdmProperty> Properties
        {
            get
            {
                Debug.Assert(
                    IsReadOnly,
                    "this is a wrapper around this.Members, don't call it during metadata loading, only call it after the metadata is set to readonly");
                if (null == _properties)
                {
                    Interlocked.CompareExchange(
                        ref _properties,
                        new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(
                            Members, Helper.IsEdmProperty), null);
                }
                return _properties;
            }
        }

        /// <summary>
        ///     Validates a EdmMember object to determine if it can be added to this type's 
        ///     Members collection. If this method returns without throwing, it is assumed
        ///     the member is valid.
        /// </summary>
        /// <param name="member"> The member to validate </param>
        /// <exception cref="System.ArgumentException">Thrown if the member is not a EdmProperty</exception>
        internal override void ValidateMemberForAdd(EdmMember member)
        {
            Debug.Assert(
                Helper.IsEdmProperty(member) || Helper.IsNavigationProperty(member),
                "Only members of type Property may be added to ComplexType.");
        }
    }

    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal sealed class ClrComplexType : ComplexType
    {
        /// <summary>
        ///     cached CLR type handle, allowing the Type reference to be GC'd
        /// </summary>
        private readonly RuntimeTypeHandle _type;

        /// <summary>
        ///     cached dynamic method to construct a CLR instance
        /// </summary>
        private Delegate _constructor;

        private readonly string _cspaceTypeName;

        /// <summary>
        ///     Initializes a new instance of Complex Type with properties from the type.
        /// </summary>
        /// <param name="clrType"> The CLR type to construct from </param>
        internal ClrComplexType(Type clrType, string cspaceNamespaceName, string cspaceTypeName)
            : base(EntityUtil.GenericCheckArgumentNull(clrType, "clrType").Name, clrType.Namespace ?? string.Empty,
                DataSpace.OSpace)
        {
            Debug.Assert(
                !String.IsNullOrEmpty(cspaceNamespaceName) &&
                !String.IsNullOrEmpty(cspaceTypeName), "Mapping information must never be null");

            _type = clrType.TypeHandle;
            _cspaceTypeName = cspaceNamespaceName + "." + cspaceTypeName;
            Abstract = clrType.IsAbstract;
        }

        internal static ClrComplexType CreateReadonlyClrComplexType(Type clrType, string cspaceNamespaceName, string cspaceTypeName)
        {
            var type = new ClrComplexType(clrType, cspaceNamespaceName, cspaceTypeName);
            type.SetReadOnly();

            return type;
        }

        /// <summary>
        ///     cached dynamic method to construct a CLR instance
        /// </summary>
        internal Delegate Constructor
        {
            get { return _constructor; }
            set
            {
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _constructor, value, null);
            }
        }

        /// <summary>
        /// </summary>
        internal override Type ClrType
        {
            get { return Type.GetTypeFromHandle(_type); }
        }

        internal string CSpaceTypeName
        {
            get { return _cspaceTypeName; }
        }
    }
}
