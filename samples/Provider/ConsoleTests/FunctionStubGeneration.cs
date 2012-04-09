using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LinqFunctionStubsGenerator;

namespace ConsoleTests
{
    class FunctionStubGeneration
    {
        public static void GenerateNewFiles()
        {
            SampleProviderLinqFunctionStubsCodeGen.Generate("..\\..\\..\\SampleEntityFrameworkProvider\\SampleProviderFunctions.cs");          
        }
    }
}
