// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Reflection;

    internal class ViewAssemblyChecker
    {
        public virtual bool IsViewAssembly(Assembly assembly)
        {
            return assembly.IsDefined(typeof(EntityViewGenerationAttribute), inherit: false);
        }
    }
}
