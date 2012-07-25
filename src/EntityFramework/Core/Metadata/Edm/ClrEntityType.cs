// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Threading;

    [SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
    internal sealed class ClrEntityType : EntityType
    {
        /// <summary>cached CLR type handle, allowing the Type reference to be GC'd</summary>
        private readonly RuntimeTypeHandle _type;

        /// <summary>cached dynamic method to construct a CLR instance</summary>
        private Delegate _constructor;

        private readonly string _cspaceTypeName;

        private readonly string _cspaceNamespaceName;

        private string _hash;

        /// <summary>
        /// Initializes a new instance of Complex Type with properties from the type.
        /// </summary>
        /// <param name="type">The CLR type to construct from</param>
        internal ClrEntityType(Type type, string cspaceNamespaceName, string cspaceTypeName)
            : base(EntityUtil.GenericCheckArgumentNull(type, "type").Name, type.Namespace ?? string.Empty,
                DataSpace.OSpace)
        {
            Debug.Assert(
                !String.IsNullOrEmpty(cspaceNamespaceName) &&
                !String.IsNullOrEmpty(cspaceTypeName), "Mapping information must never be null");

            _type = type.TypeHandle;
            _cspaceNamespaceName = cspaceNamespaceName;
            _cspaceTypeName = cspaceNamespaceName + "." + cspaceTypeName;
            Abstract = type.IsAbstract;
        }

        /// <summary>cached dynamic method to construct a CLR instance</summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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

        internal string CSpaceNamespaceName
        {
            get { return _cspaceNamespaceName; }
        }

        /// <summary>
        /// Gets a collision resistent (SHA256) hash of the information used to build
        /// a proxy for this type.  This hash is very, very unlikely to be the same for two
        /// proxies generated from the same CLR type but with different metadata, and is
        /// guarenteed to be the same for proxies generated from the same metadata.  This
        /// means that when EntityType comparison fails because of metadata eviction,
        /// the hash can be used to determine whether or not a proxy is of the correct type.
        /// </summary>
        internal string HashedDescription
        {
            get
            {
                if (_hash == null)
                {
                    Interlocked.CompareExchange(ref _hash, BuildEntityTypeHash(), null);
                }
                return _hash;
            }
        }

        /// <summary>
        /// Creates an SHA256 hash of a description of all the metadata relevant to the creation of a proxy type
        /// for this entity type.
        /// </summary>
        private string BuildEntityTypeHash()
        {
            using (var sha256HashAlgorithm = MetadataHelper.CreateSHA256HashAlgorithm())
            {
                var hash = sha256HashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(BuildEntityTypeDescription()));

                // convert num bytes to num hex digits
                var builder = new StringBuilder(hash.Length * 2);
                foreach (var bite in hash)
                {
                    builder.Append(bite.ToString("X2", CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Creates a description of all the metadata relevant to the creation of a proxy type
        /// for this entity type.
        /// </summary>
        private string BuildEntityTypeDescription()
        {
            var builder = new StringBuilder(512);
            Debug.Assert(ClrType != null, "Expecting non-null CLRType of o-space EntityType.");
            builder.Append("CLR:").Append(ClrType.FullName);
            builder.Append("Conceptual:").Append(CSpaceTypeName);

            var navProps = new SortedSet<string>();
            foreach (var navProperty in NavigationProperties)
            {
                navProps.Add(
                    navProperty.Name + "*" +
                    navProperty.FromEndMember.Name + "*" +
                    navProperty.FromEndMember.RelationshipMultiplicity + "*" +
                    navProperty.ToEndMember.Name + "*" +
                    navProperty.ToEndMember.RelationshipMultiplicity + "*");
            }
            builder.Append("NavProps:");
            foreach (var navProp in navProps)
            {
                builder.Append(navProp);
            }

            var keys = new SortedSet<string>();
            foreach (var member in KeyMemberNames)
            {
                keys.Add(member);
            }
            builder.Append("Keys:");
            foreach (var key in keys)
            {
                builder.Append(key + "*");
            }

            var scalars = new SortedSet<string>();
            foreach (var member in Members)
            {
                if (!keys.Contains(member.Name))
                {
                    scalars.Add(member.Name + "*");
                }
            }
            builder.Append("Scalars:");
            foreach (var scalar in scalars)
            {
                builder.Append(scalar + "*");
            }

            return builder.ToString();
        }
    }
}
