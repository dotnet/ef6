// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace CmdLine
{
    using System;
    using System.Data.Entity.Migrations.Console.Resources;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandLineArgumentsAttribute : Attribute
    {
        private string title;

        public string Title
        {
            get
            {
                if (TitleResourceId != null)
                {
                    return EntityRes.GetString(TitleResourceId);
                }
                return title;
            }
            set
            {
                if (TitleResourceId != null)
                {
                    throw Error.AmbiguousAttributeValues("Title", "TitleResourceId");
                }
                title = value;
            }
        }

        private string titleResourceId;

        public string TitleResourceId
        {
            get { return titleResourceId; }
            set
            {
                if (Title != null)
                {
                    throw Error.AmbiguousAttributeValues("Title", "TitleResourceId");
                }
                titleResourceId = value;
            }
        }

        private string description;

        public string Description
        {
            get
            {
                if (DescriptionResourceId != null)
                {
                    return EntityRes.GetString(DescriptionResourceId);
                }
                return description;
            }
            set
            {
                if (DescriptionResourceId != null)
                {
                    throw Error.AmbiguousAttributeValues("Description", "DescriptionResourceId");
                }
                description = value;
            }
        }

        private string descriptionResourceId;

        public string DescriptionResourceId
        {
            get { return descriptionResourceId; }
            set
            {
                if (Description != null)
                {
                    throw Error.AmbiguousAttributeValues("Description", "DescriptionResourceId");
                }
                descriptionResourceId = value;
            }
        }

        public string Program { get; set; }

        /// <summary>
        ///   Returns a CommandLineArgumentsAttribute
        /// </summary>
        /// <param name = "member"></param>
        /// <returns></returns>
        public static CommandLineArgumentsAttribute Get(MemberInfo member)
        {
            return
                GetCustomAttributes(member, typeof(CommandLineArgumentsAttribute)).Cast<CommandLineArgumentsAttribute>()
                    .FirstOrDefault();
        }
    }
}
