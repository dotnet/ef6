// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ViewGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure.MappingViews;

    public sealed class PregenContextViews : DbMappingViewCache
    {
        public static bool View0Accessed { get; set; }
        public static bool View1Accessed { get; set; }

        public override string MappingHashValue
        {
            get { return "a76b956abbdcf4cc24982c118d0afa3b1cfaedba377a7663cadd041aa30b81be"; }
        }

        public override DbMappingView GetView(EntitySetBase extent)
        {
            var extentFullName = extent.EntityContainer.Name + "." + extent.Name;
            switch (extentFullName)
            {
                case "CodeFirstDatabase.PregenBlog":
                    View0Accessed = true;

                    return new DbMappingView(
@"SELECT VALUE -- Constructing PregenBlog
    [CodeFirstDatabaseSchema.PregenBlog](T1.PregenBlog_Id)
FROM (
    SELECT 
        T.Id AS PregenBlog_Id, 
        True AS _from0
FROM PregenContext.Blogs AS T
) AS T1");

                case "PregenContext.Blogs":
                    View1Accessed = true;
                    
                    return new DbMappingView(
@"SELECT VALUE -- Constructing Blogs
    [System.Data.Entity.ViewGeneration.PregenBlog](T1.PregenBlog_Id)
FROM (
    SELECT 
        T.Id AS PregenBlog_Id, 
        True AS _from0
    FROM CodeFirstDatabase.PregenBlog AS T
) AS T1");

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
