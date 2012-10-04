// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Represent the edm navigation property class
    /// </summary>
    public sealed class NavigationProperty : EdmMember
    {
        /// <summary>
        ///     Initializes a new instance of the navigation property class
        /// </summary>
        /// <param name="name"> name of the navigation property </param>
        /// <param name="typeUsage"> TypeUsage object containing the navigation property type and its facets </param>
        /// <exception cref="System.ArgumentNullException">Thrown if name or typeUsage arguments are null</exception>
        /// <exception cref="System.ArgumentException">Thrown if name argument is empty string</exception>
        internal NavigationProperty(string name, TypeUsage typeUsage)
            : base(name, typeUsage)
        {
            EntityUtil.CheckStringArgument(name, "name");
            EntityUtil.GenericCheckArgumentNull(typeUsage, "typeUsage");
            _accessor = new NavigationPropertyAccessor(name);
        }

        /// <summary>
        ///     Initializes a new OSpace instance of the property class
        /// </summary>
        /// <param name="name"> name of the property </param>
        /// <param name="typeUsage"> TypeUsage object containing the property type and its facets </param>
        /// <param name="propertyInfo"> for the property </param>
        internal NavigationProperty(string name, TypeUsage typeUsage, PropertyInfo propertyInfo)
            : this(name, typeUsage)
        {
            Debug.Assert(name == propertyInfo.Name, "different PropertyName?");
            if (null != propertyInfo)
            {
                MethodInfo method;

                method = propertyInfo.GetGetMethod();
                PropertyGetterHandle = ((null != method) ? method.MethodHandle : default(RuntimeMethodHandle));
            }
        }

        /// <summary>
        ///     Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind
        {
            get { return BuiltInTypeKind.NavigationProperty; }
        }

        internal const string RelationshipTypeNamePropertyName = "RelationshipType";
        internal const string ToEndMemberNamePropertyName = "ToEndMember";

        /// <summary>
        ///     Store the handle, allowing the PropertyInfo/MethodInfo/Type references to be GC'd
        /// </summary>
        internal readonly RuntimeMethodHandle PropertyGetterHandle;

        /// <summary>
        ///     cached dynamic methods to access the property values from a CLR instance
        /// </summary>
        private readonly NavigationPropertyAccessor _accessor;

        /// <summary>
        ///     Gets/Sets the relationship type that this navigation property operates on
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the NavigationProperty instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipType, false)]
        public RelationshipType RelationshipType { get; internal set; }

        /// <summary>
        ///     Gets/Sets the to relationship end member in the navigation
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the NavigationProperty instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
        public RelationshipEndMember ToEndMember { get; internal set; }

        /// <summary>
        ///     Gets/Sets the from relationship end member in the navigation
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the NavigationProperty instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
        public RelationshipEndMember FromEndMember { get; internal set; }

        internal NavigationPropertyAccessor Accessor
        {
            get { return _accessor; }
        }

        /// <summary>
        ///     Where the given navigation property is on the dependent end of a referential constraint,
        ///     returns the foreign key properties. Otherwise, returns an empty set. We will return the members in the order
        ///     of the principal end key properties.
        /// </summary>
        /// <returns> Foreign key properties </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<EdmProperty> GetDependentProperties()
        {
            // Get the declared type
            var associationType = (AssociationType)RelationshipType;
            Debug.Assert(
                associationType.ReferentialConstraints != null,
                "ReferenceConstraints cannot be null");

            if (associationType.ReferentialConstraints.Count > 0)
            {
                var rc = associationType.ReferentialConstraints[0];
                var dependentEndMember = rc.ToRole;

                if (dependentEndMember.EdmEquals(FromEndMember))
                {
                    //Order the dependant properties in the order of principal end's key members.
                    var keyMembers = rc.FromRole.GetEntityType().KeyMembers;
                    var dependantProperties = new List<EdmProperty>(keyMembers.Count);
                    for (var i = 0; i < keyMembers.Count; i++)
                    {
                        dependantProperties.Add(rc.ToProperties[rc.FromProperties.IndexOf(((EdmProperty)keyMembers[i]))]);
                    }
                    return dependantProperties.AsReadOnly();
                }
            }

            return Enumerable.Empty<EdmProperty>();
        }
    }
}
