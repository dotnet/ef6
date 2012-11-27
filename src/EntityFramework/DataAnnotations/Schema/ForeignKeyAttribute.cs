// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if NET40

namespace System.ComponentModel.DataAnnotations.Schema
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Denotes a property used as a foreign key in a relationship.
    ///     The annotation may be placed on the foreign key property and specify the associated navigation property name, 
    ///     or placed on a navigation property and specify the associated foreign key name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
        Justification = "We want users to be able to extend this class")]
    public class ForeignKeyAttribute : Attribute
    {
        private readonly string _name;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ForeignKeyAttribute" /> class.
        /// </summary>
        /// <param name="name"> If placed on a foreign key property, the name of the associated navigation property. If placed on a navigation property, the name of the associated foreign key(s). If a navigation property has multiple foreign keys, a comma separated list should be supplied. </param>
        public ForeignKeyAttribute(string name)
        {
            Check.NotEmpty(name, "name");

            _name = name;
        }

        /// <summary>
        ///     If placed on a foreign key property, the name of the associated navigation property.
        ///     If placed on a navigation property, the name of the associated foreign key(s).
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
    }
}

#endif
