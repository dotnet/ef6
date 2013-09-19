// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Console.Resources;
    using System.Reflection;

    internal class CommandLineParameter
    {
        internal CommandLineParameter(PropertyInfo property, CommandLineParameterAttribute attribute)
        {
            Property = property;
            Attribute = attribute;
            Command = Attribute.Command;

            // Set the defaults
            if (property.PropertyType
                == typeof(bool))
            {
                // If no switch is specified for bool properties then the name is the switch
                if (string.IsNullOrWhiteSpace(Command))
                {
                    Command = property.Name;
                }
            }
        }

        internal string Command { get; set; }

        internal PropertyInfo Property { get; set; }

        internal CommandArgument Argument { get; set; }

        internal CommandLineParameterAttribute Attribute { get; private set; }

        internal bool ArgumentSupplied { get; set; }

        internal bool RequiredArgumentSupplied
        {
            get { return !Attribute.Required || ArgumentSupplied; }
        }

        public string Key
        {
            get
            {
                return IsCommand()
                           ? CommandLine.CaseSensitive
                                 ? Command
                                 : Command.ToLowerInvariant()
                           : CommandLineParameterAttribute.GetParameterKey(Attribute.ParameterIndex);
            }
        }

        internal bool IsParameter()
        {
            return !IsCommand();
        }

        internal bool IsCommand()
        {
            return !string.IsNullOrWhiteSpace(Command);
        }

        public void SetDefaultValue(object argument)
        {
            if (Attribute == null
                || Attribute.Default == null)
            {
                return;
            }

            var property = argument.GetType().GetProperty(Property.Name);
            property.SetValue(argument, Attribute.Default, null);
        }

        public void SetValue(object argument, CommandArgument cmd)
        {
            // Argument already supplied
            if (!IsCollection() && ArgumentSupplied)
            {
                throw new CommandLineArgumentInvalidException(argument.GetType(), cmd);
            }

            Argument = cmd;

            if (Property.PropertyType
                == typeof(bool))
            {
                Property.SetValue(argument, GetBoolValue(cmd), null);
            }
            else if (Property.PropertyType
                     == typeof(int))
            {
                Property.SetValue(argument, Convert.ToInt32(cmd.Value), null);
            }
            else if (Property.PropertyType
                     == typeof(DateTime))
            {
                Property.SetValue(argument, Convert.ToDateTime(cmd.Value), null);
            }
            else if (Property.PropertyType
                     == typeof(string))
            {
                Property.SetValue(argument, cmd.Value, null);
            }
            else if (Property.PropertyType
                     == typeof(List<string>))
            {
                var list = (List<string>)Property.GetValue(argument, null) ?? new List<string>();
                list.Add(cmd.Value);
                Property.SetValue(argument, list, null);
            }
            else
            {
                throw new CommandLineException(
                    new CommandArgumentHelp(
                        argument.GetType(), Strings.UnsupportedPropertyType(Property.PropertyType)));
            }

            ArgumentSupplied = true;
        }

        private bool IsCollection()
        {
            return Property.PropertyType == typeof(List<string>);
        }

        /// <summary>
        /// Returns a boolean value from a command switch
        /// </summary>
        /// <param name="cmd"> The command switch </param>
        /// <returns> A boolean value based on the switch and value </returns>
        private static bool GetBoolValue(CommandArgument cmd)
        {
            return string.IsNullOrWhiteSpace(cmd.SwitchOption) || cmd.SwitchOption.Trim() == "+";
        }
    }
}
