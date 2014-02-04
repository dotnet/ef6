namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.CodeGeneration.Generators;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VisualStudio;

    internal class CodeGeneratorFactory : ICodeGeneratorFactory
    {
        private const string CSharpContextTemplatePath = @"CodeTemplates\EFModelFromDatabase\Context.cs.t4";
        private const string VBContextTemplatePath = @"CodeTemplates\EFModelFromDatabase\Context.vb.t4";
        private const string CSharpEntityTypeTemplatePath = @"CodeTemplates\EFModelFromDatabase\EntityType.cs.t4";
        private const string VBEntityTypeTemplatePath = @"CodeTemplates\EFModelFromDatabase\EntityType.vb.t4";

        private readonly IDictionary<string, string> _templatePathCache = new Dictionary<string, string>();
        private readonly Project _project;

        public CodeGeneratorFactory(Project project)
        {
            Debug.Assert(project != null, "project is null.");

            _project = project;
        }

        public IContextGenerator GetContextGenerator(LangEnum language, bool isEmptyModel)
        {
            if (language == LangEnum.VisualBasic)
            {
                if (isEmptyModel)
                {
                    return new VBCodeFirstEmptyModelGenerator();
                }

                return (IContextGenerator)TryGetCustomizedTemplate(VBContextTemplatePath)
                       ?? new DefaultVBContextGenerator();
            }

            if (isEmptyModel)
            {
                return new CSharpCodeFirstEmptyModelGenerator();
            }

            return (IContextGenerator)TryGetCustomizedTemplate(CSharpContextTemplatePath)
                   ?? new DefaultCSharpContextGenerator();
        }

        public IEntityTypeGenerator GetEntityTypeGenerator(LangEnum language)
        {
            if (language == LangEnum.VisualBasic)
            {
                return (IEntityTypeGenerator)TryGetCustomizedTemplate(VBEntityTypeTemplatePath)
                       ?? new DefaultVBEntityTypeGenerator();
            }

            return (IEntityTypeGenerator)TryGetCustomizedTemplate(CSharpEntityTypeTemplatePath)
                   ?? new DefaultCSharpEntityTypeGenerator();
        }

        private CustomGenerator TryGetCustomizedTemplate(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "path is null or empty.");

            if (!_templatePathCache.ContainsKey(path))
            {
                string templatePath = null;

                var templateItem = VsUtils.GetProjectItemByPath(_project, path);
                if (templateItem != null)
                {
                    templatePath = VsUtils.GetPropertyByName(templateItem.Properties, "FullPath") as string;
                }

                _templatePathCache.Add(path, templatePath);
            }

            return _templatePathCache[path] != null
                ? new CustomGenerator(_templatePathCache[path])
                : null;
        }
    }
}