// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace CmdLine.Tests
{
    extern alias migrate;
    using System;
    using System.Collections.Generic;

    public class TestCommandEnvironment : migrate::CmdLine.ICommandEnvironment
    {
        private string[] args;

        public TestCommandEnvironment()
        {
        }

        public TestCommandEnvironment(string cmdLine)
        {
            CommandLine = cmdLine;
            Args = cmdLine.Split(' ');
        }

        public TestCommandEnvironment(string[] args)
        {
            Args = args;
            CommandLine = string.Join(" ", args);
        }

        public string[] Args
        {
            get
            {
                if (args == null)
                {
                    SetArgs(null);
                }
                return args;
            }
            private set { SetArgs(value); }
        }

        #region ICommandEnvironment Members

        public string CommandLine { get; private set; }

        public string[] GetCommandLineArgs()
        {
            return Args;
        }

        public string Program
        {
            get { return Environment.GetCommandLineArgs()[0]; }
        }

        #endregion

        private void SetArgs(IEnumerable<string> values)
        {
            var argList = new List<string>
                              {
                                  Program
                              };

            if (values != null)
            {
                argList.AddRange(values);
            }

            args = argList.ToArray();
        }
    }
}
