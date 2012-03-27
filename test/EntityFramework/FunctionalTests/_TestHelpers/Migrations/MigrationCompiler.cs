namespace System.Data.Entity.Migrations
{
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Spatial;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    public class MigrationCompiler
    {
        private readonly CodeDomProvider _codeProvider;

        public MigrationCompiler(string language)
        {
            _codeProvider = CodeDomProvider.CreateProvider(language);
        }

        public Assembly Compile(params string[] sources)
        {
            var options = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true
                };

            options.ReferencedAssemblies.Add(typeof(string).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(Expression).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(DbMigrator).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(DbContext).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(DbConnection).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(Component).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(MigrationCompiler).Assembly.Location);
            options.ReferencedAssemblies.Add(typeof(DbGeography).Assembly.Location);

            var compilerResults = _codeProvider.CompileAssemblyFromSource(options, sources);

            if (compilerResults.Errors.Count > 0)
            {
                sources.Each(s => Console.WriteLine(s));

                throw new InvalidOperationException(BuildCompileErrorMessage(compilerResults.Errors));
            }

            //Console.WriteLine(sources[0]);

            return compilerResults.CompiledAssembly;
        }

        private static string BuildCompileErrorMessage(CompilerErrorCollection errors)
        {
            var stringBuilder = new StringBuilder();

            foreach (CompilerError error in errors)
            {
                stringBuilder.AppendLine(error.ToString());
            }

            return stringBuilder.ToString();
        }
    }
}