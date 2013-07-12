// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ViewGeneration
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;

    public sealed class PregenObjectContextViews : DbMappingViewCache
    {
        public static bool View0Accessed { get; set; }
        public static bool View1Accessed { get; set; }

        public override string MappingHashValue
        {
            get { return "58210c83048909cf6bc9eb2f4c3d677d1b3edc1145c3eb2ddd5197327458371c"; }
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
    FROM CodeFirstContainer.PregenBlogs AS T
) AS T1");

                case "CodeFirstContainer.PregenBlogs":
                    View1Accessed = true;

                    return new DbMappingView(
@"SELECT VALUE -- Constructing PregenBlogs
    [CodeFirstNamespace.PregenBlog](T1.PregenBlog_Id)
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
