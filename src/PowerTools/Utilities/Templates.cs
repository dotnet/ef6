// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace Microsoft.DbContextPackage.Utilities
{
    using System.IO;
    using System.Reflection;
    using System.Text;

    internal static class Templates
    {
        public const string CsharpContextTemplate = @"CodeTemplates\EFModelFromDatabase\Context.cs.t4";
        public const string CsharpEntityTemplate = @"CodeTemplates\EFModelFromDatabase\EntityType.cs.t4";
        public const string VBContextTemplate = @"CodeTemplates\EFModelFromDatabase\Context.vb.t4";
        public const string VBEntityTemplate = @"CodeTemplates\EFModelFromDatabase\EntityType.vb.t4";

        public static string GetDefaultTemplate(string path)
        {
            DebugCheck.NotEmpty(path);

            var fromDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return File.ReadAllText(Path.Combine(fromDir, path), Encoding.UTF8);
        }
    }
}
