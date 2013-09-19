// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if NET40

namespace System.ComponentModel.DataAnnotations.Schema
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Specifies the inverse of a navigation property that represents the other end of the same relationship.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
        Justification = "We want users to be able to extend this class")]
    public class InversePropertyAttribute : Attribute
    {
        private readonly string _property;

        /// <summary>
        /// Initializes a new instance of the <see cref="InversePropertyAttribute" /> class.
        /// </summary>
        /// <param name="property"> The navigation property representing the other end of the same relationship. </param>
        public InversePropertyAttribute(string property)
        {
            Check.NotEmpty(property, "property");

            _property = property;
        }

        /// <summary>
        /// The navigation property representing the other end of the same relationship.
        /// </summary>
        public string Property
        {
            get { return _property; }
        }
    }
}

#endif
