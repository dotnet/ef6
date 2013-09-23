// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    /// Class representing a simple type
    /// </summary>
    public abstract class SimpleType : EdmType
    {
        // <summary>
        // The default constructor for SimpleType
        // </summary>
        internal SimpleType()
        {
            // No initialization of item attributes in here, it's used as a pass thru in the case for delay population
            // of item attributes
        }

        // <summary>
        // The constructor for SimpleType.  It takes the required information to identify this type.
        // </summary>
        // <param name="name"> The name of this type </param>
        // <param name="namespaceName"> The namespace name of this type </param>
        // <param name="dataSpace"> dataspace in which the simple type belongs to </param>
        // <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal SimpleType(string name, string namespaceName, DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
        }
    }
}
