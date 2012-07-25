// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServer.SqlGen
{
    /// <summary>
    /// Used for wrapping a boolean value as an object.
    /// </summary>
    internal class BoolWrapper
    {
        internal bool Value { get; set; }

        internal BoolWrapper()
        {
            Value = false;
        }
    }
}
