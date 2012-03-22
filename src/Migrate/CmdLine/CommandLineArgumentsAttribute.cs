namespace CmdLine
{
    using System;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandLineArgumentsAttribute : Attribute
    {
        public string Title { get; set; }

        public string Description { get; set; }

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
