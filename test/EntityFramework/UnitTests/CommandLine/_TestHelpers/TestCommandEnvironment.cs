// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace CmdLine.Tests
{
    extern alias migrate;
    using System;
    using System.Collections.Generic;
    using migrate::CmdLine;

    public class TestCommandEnvironment : ICommandEnvironment
    {
        private string[] args;

        public TestCommandEnvironment()
        {
        }

        public TestCommandEnvironment(string cmdLine)
        {
            this.CommandLine = cmdLine;
            this.Args = cmdLine.Split(' ');
        }

        public TestCommandEnvironment(string[] args)
        {
            this.Args = args;
            this.CommandLine = string.Join(" ", args);
        }

        public string[] Args
        {
            get
            {
                if (this.args == null)
                {
                    this.SetArgs(null);
                }
                return this.args;
            }
            private set
            {
                this.SetArgs(value);
            }
        }

        #region ICommandEnvironment Members

        public string CommandLine { get; private set; }

        public string[] GetCommandLineArgs()
        {
            return this.Args;
        }

        public string Program
        {
            get
            {
                return Environment.GetCommandLineArgs()[0];
            }
        }

        #endregion

        private void SetArgs(IEnumerable<string> values)
        {
            var argList = new List<string> { this.Program };

            if (values != null)
            {
                argList.AddRange(values);                
            }

            this.args = argList.ToArray();
        }
    }
}