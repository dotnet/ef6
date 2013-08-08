// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Console.Resources;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Property)]
    [Serializable]
    internal class CommandLineParameterAttribute : Attribute
    {
        private int parameterIndex = -1;

        public CommandLineParameterAttribute()
        {
        }

        public CommandLineParameterAttribute(string command)
        {
            Command = command;
        }

        private string name;

        public string Name
        {
            get
            {
                if (NameResourceId != null)
                {
                    return EntityRes.GetString(NameResourceId);
                }
                return name;
            }
            set
            {
                if (NameResourceId != null)
                {
                    throw Error.AmbiguousAttributeValues("Name", "NameResourceId");
                }
                name = value;
            }
        }

        private string nameResourceId;

        public string NameResourceId
        {
            get { return nameResourceId; }
            set
            {
                if (Name != null)
                {
                    throw Error.AmbiguousAttributeValues("Name", "NameResourceId");
                }
                nameResourceId = value;
            }
        }

        private string description;

        /// <summary>
        /// The description of the command
        /// </summary>
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

        /// <summary>
        /// The resource id of the command description
        /// </summary>
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

        public object Default { get; set; }

        public bool Required { get; set; }

        public string ValueExample { get; set; }

        public int ParameterIndex
        {
            get { return parameterIndex; }
            set
            {
                if (value < 1)
                {
                    throw Error.InvalidParameterIndexValue();
                }

                parameterIndex = value;
            }
        }

        public object Value { get; set; }

        public string Command { get; set; }

        public string NameOrCommand
        {
            get
            {
                return string.IsNullOrWhiteSpace(Name)
                           ? Command
                           : Name;
            }
        }

        public bool IsHelp { get; set; }

        public static CommandLineParameterAttribute Get(MemberInfo member)
        {
            return
                GetCustomAttributes(member, typeof(CommandLineParameterAttribute)).Cast<CommandLineParameterAttribute>()
                                                                                  .FirstOrDefault();
        }

        public static IEnumerable<CommandLineParameterAttribute> GetAll(MemberInfo member)
        {
            return
                GetCustomAttributes(member, typeof(CommandLineParameterAttribute)).Cast<CommandLineParameterAttribute>();
        }

        public static IEnumerable<CommandLineParameterAttribute> GetAllPropertyParameters(Type argumentClassType)
        {
            return
                argumentClassType.GetProperties().SelectMany(
                    property =>
                    property.GetCustomAttributes(typeof(CommandLineParameterAttribute), true).Cast
                        <CommandLineParameterAttribute>());
        }

        public bool IsCommand()
        {
            return !string.IsNullOrWhiteSpace(Command);
        }

        internal static string GetParameterKey(int position)
        {
            return string.Format("Parameter[{0}]", position);
        }

        internal void Validate(CommandLineParameter parameter)
        {
            if (ParameterIndex < 1)
            {
                throw new CommandLineException(
                    new CommandArgumentHelp(parameter.Property, Strings.InvalidPropertyParameterIndexValue(parameter.Property.Name)));
            }
        }

        /// <summary>
        /// Searches a type for all properties with the CommandLineParameterAttribute and does action
        /// </summary>
        /// <param name="argumentType"> The argument type </param>
        /// <param name="action"> The action to apply </param>
        internal static void ForEach(Type argumentType, Action<CommandLineParameter> action)
        {
            ForEach(argumentType, GetAll, action);
        }

        /// <summary>
        /// Searches a type for all properties with the CommandLineParameterAttribute and does action
        /// </summary>
        /// <param name="argumentType"> The argument type </param>
        /// <param name="selector"> </param>
        /// <param name="action"> The action to apply </param>
        internal static void ForEach(
            Type argumentType, Func<PropertyInfo, IEnumerable<CommandLineParameterAttribute>> selector,
            Action<CommandLineParameter> action)
        {
            foreach (var parameter in argumentType.GetProperties().SelectMany(
                property => selector(property).Select(cmdAttribute => new CommandLineParameter(property, cmdAttribute)))
                )
            {
                action(parameter);
            }
        }

        public bool IsParameter()
        {
            return !IsCommand();
        }
    }
}
