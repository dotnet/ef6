// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    ///     Attribute identifying the Ends defined for a RelationshipSet
    ///     Implied default AttributeUsage properties Inherited=True, AllowMultiple=False,
    ///     The metadata system expects this and will only look at the first of each of these attributes, even if there are more.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EdmRelationshipNavigationPropertyAttribute : EdmPropertyAttribute
    {
        private readonly string _relationshipNamespaceName;
        private readonly string _relationshipName;
        private readonly string _targetRoleName;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Objects.DataClasses.EdmRelationshipNavigationPropertyAttribute" />
        ///     class.
        /// </summary>
        /// <param name="relationshipNamespaceName">The namespace name of the relationship property.</param>
        /// <param name="relationshipName">The name of the relationship. The relationship name is not namespace qualified.</param>
        /// <param name="targetRoleName">The role name at the other end of the relationship.</param>
        public EdmRelationshipNavigationPropertyAttribute(string relationshipNamespaceName, string relationshipName, string targetRoleName)
        {
            _relationshipNamespaceName = relationshipNamespaceName;
            _relationshipName = relationshipName;
            _targetRoleName = targetRoleName;
        }

        /// <summary>The namespace name of the navigation property.</summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that is the namespace name.
        /// </returns>
        public string RelationshipNamespaceName
        {
            get { return _relationshipNamespaceName; }
        }

        /// <summary>Gets the unqualified relationship name. </summary>
        /// <returns>The relationship name.</returns>
        public string RelationshipName
        {
            get { return _relationshipName; }
        }

        /// <summary>Gets the role name at the other end of the relationship.</summary>
        /// <returns>The target role name is specified by the Role attribute of the other End element in the association that defines this relationship in the conceptual model. For more information, see Association (EDM).</returns>
        public string TargetRoleName
        {
            get { return _targetRoleName; }
        }
    }
}
