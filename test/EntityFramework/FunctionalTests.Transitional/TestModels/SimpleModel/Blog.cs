// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace SimpleModel
{
    using System.ComponentModel.DataAnnotations;

    public class Blog
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
    }
}
