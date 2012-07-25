// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.CommandTrees
{
    /// <summary>
    /// Describes the different "kinds" (classes) of command trees.
    /// </summary>
    public enum DbCommandTreeKind
    {
        Query,
        Update,
        Insert,
        Delete,
        Function,
    }
}
