using System;

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Attribute identifying the Ends defined for a RelationshipSet
    /// Implied default AttributeUsage properties Inherited=True, AllowMultiple=False,
    /// The metadata system expects this and will only look at the first of each of these attributes, even if there are more.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EdmRelationshipNavigationPropertyAttribute : EdmPropertyAttribute
    {
        private string _relationshipNamespaceName;
        private string _relationshipName;
        private string _targetRoleName;

        /// <summary>
        /// Attribute identifying the Ends defined for a RelationshipSet
        /// </summary>
        public EdmRelationshipNavigationPropertyAttribute(string relationshipNamespaceName, string relationshipName, string targetRoleName)
        {
            _relationshipNamespaceName = relationshipNamespaceName;
            _relationshipName = relationshipName;
            _targetRoleName = targetRoleName;
        }

        /// <summary>
        /// the namespace name of the relationship
        /// </summary>
        public string RelationshipNamespaceName
        {
            get { return _relationshipNamespaceName; }
        }

        /// <summary>
        /// the relationship name
        /// </summary>
        public string RelationshipName
        {
            get { return _relationshipName; }
        }

        /// <summary>
        /// the target role name
        /// </summary>
        public string TargetRoleName
        {
            get { return _targetRoleName; }
        }

    }
}
