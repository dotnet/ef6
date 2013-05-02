// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Interception
{
    public class BlogContextNoInit : BlogContext
    {
        static BlogContextNoInit()
        {
            Database.SetInitializer<BlogContextNoInit>(new BlogInitializer());
        }
    }
}
