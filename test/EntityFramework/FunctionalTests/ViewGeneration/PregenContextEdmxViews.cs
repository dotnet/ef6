// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ViewGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.MappingViews;

    public sealed class PregenContextEdmxViews : DbMappingViewCache
    {
        public static bool View0Accessed { get; set; }
        public static bool View1Accessed { get; set; }

        public override string MappingHashValue
        {
            get { return "e204170d57ba6e546c20a5b14040470581534f5d2b996ab84f7b44fe13154168"; }
        }

        public override DbMappingView GetView(EntitySetBase extent)
        {
            var extentFullName = extent.EntityContainer.Name + "." + extent.Name;
            switch (extentFullName)
            {
                case "CodeFirstDatabase.PregenBlogEdmx":
                    View0Accessed = true;

                    return new DbMappingView(
@"SELECT VALUE -- Constructing PregenBlogEdmx
    [CodeFirstDatabaseSchema.PregenBlogEdmx](T1.PregenBlogEdmx_Id)
FROM (
    SELECT 
        T.Id AS PregenBlogEdmx_Id, 
        True AS _from0
    FROM PregenContextEdmx.Blogs AS T
) AS T1");

                case "PregenContextEdmx.Blogs":
                    View1Accessed = true;

                    return new DbMappingView(
@"SELECT VALUE -- Constructing Blogs
    [System.Data.Entity.ViewGeneration.PregenBlogEdmx](T1.PregenBlogEdmx_Id)
FROM (
    SELECT 
        T.Id AS PregenBlogEdmx_Id, 
        True AS _from0
    FROM CodeFirstDatabase.PregenBlogEdmx AS T
) AS T1");

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
