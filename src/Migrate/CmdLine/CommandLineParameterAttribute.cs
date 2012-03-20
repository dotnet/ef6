namespace CmdLine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Property)]
    public class CommandLineParameterAttribute : Attribute
    {
        private int parameterIndex = -1;

        public CommandLineParameterAttribute()
        {
        }

        public CommandLineParameterAttribute(string command)
        {
            this.Command = command;
        }

        public string Name { get; set; }

        /// <summary>
        ///   The description of the command
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///   The Resource ID of the description
        /// </summary>
        public string DescriptionID { get; set; }

        public object Default { get; set; }

        public bool Required { get; set; }

        public string ValueExample { get; set; }

        public int ParameterIndex
        {
            get
            {
                return this.parameterIndex;
            }
            set
            {
                if (value < 1)
                {
                    throw new CommandLineException("Invalid ParameterIndex value ");
                }

                this.parameterIndex = value;
            }
        }

        public object Value { get; set; }

        public string Command { get; set; }

        public string NameOrCommand
        {
            get
            {
                return string.IsNullOrWhiteSpace(this.Name)
                           ? this.Command
                           : this.Name;
            }
        }

        public bool IsHelp { get; set; }

        public static CommandLineParameterAttribute Get(MemberInfo member)
        {
            return GetCustomAttributes(member, typeof(CommandLineParameterAttribute)).Cast<CommandLineParameterAttribute>().FirstOrDefault();
        }

        public static IEnumerable<CommandLineParameterAttribute> GetAll(MemberInfo member)
        {
            return GetCustomAttributes(member, typeof(CommandLineParameterAttribute)).Cast<CommandLineParameterAttribute>();
        }

        public static IEnumerable<CommandLineParameterAttribute> GetAllPropertyParameters(Type argumentClassType)
        {
            return argumentClassType.GetProperties().SelectMany(property => property.GetCustomAttributes(typeof(CommandLineParameterAttribute), true).Cast<CommandLineParameterAttribute>());
        }



        public bool IsCommand()
        {
            return !string.IsNullOrWhiteSpace(this.Command);
        }

        internal static string GetParameterKey(int position)
        {
            return string.Format("Parameter[{0}]", position);
        }

        internal void Validate(CommandLineParameter parameter)
        {
            if (this.ParameterIndex < 1)
            {
                throw new CommandLineException(
                    new CommandArgumentHelp(parameter.Property, string.Format("Invalid ParameterIndex value on Property \"{0}\"", parameter.Property.Name)));
            }
        }

        /// <summary>
        /// Searches a type for all properties with the CommandLineParameterAttribute and does action
        /// </summary>
        /// <param name="argumentType">The argument type</param>
        /// <param name="action">The action to apply</param>
        internal static void ForEach(Type argumentType, Action<CommandLineParameter> action)
        {
            ForEach(argumentType, GetAll, action);
        }

        /// <summary>
        /// Searches a type for all properties with the CommandLineParameterAttribute and does action
        /// </summary>
        /// <param name="argumentType">The argument type</param>
        /// <param name="selector"></param>
        /// <param name="action">The action to apply</param>
        internal static void ForEach(Type argumentType, Func<PropertyInfo, IEnumerable<CommandLineParameterAttribute>> selector, Action<CommandLineParameter> action)
        {
            foreach (var parameter in argumentType.GetProperties().SelectMany(
                    property => selector(property).Select(cmdAttribute => new CommandLineParameter(property, cmdAttribute))))
            {
                action(parameter);
            }            
        }

        public bool IsParameter()
        {
            return !this.IsCommand();
        }
    }
}