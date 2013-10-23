// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.VisualStudio.TestTools.VsIdeTesting
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class VsIdePreHostExecutionMethod : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class VsIdePostHostExecutionMethod : Attribute
    {
    }
}
