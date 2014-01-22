// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics;
    using EnvDTE;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Resources = Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Properties.Resources;

    internal class CodeFirstModelGenerator
    {
        private const string CSharpContextTemplatePath = @"CodeTemplates\EFModelFromDatabase\Context.cs.t4";
        private const string VBContextTemplatePath = @"CodeTemplates\EFModelFromDatabase\Context.vb.t4";
        private const string CSharpEntityTypeTemplatePath = @"CodeTemplates\EFModelFromDatabase\EntityType.cs.t4";
        private const string VBEntityTypeTemplatePath = @"CodeTemplates\EFModelFromDatabase\EntityType.vb.t4";

        private readonly Project _project;
        private readonly IServiceProvider _serviceProvider;
        private readonly LangEnum _language;
        private readonly IDictionary<string, string> _templatePathCache = new Dictionary<string, string>();

        public CodeFirstModelGenerator(Project project, IServiceProvider serviceProvider)
        {
            Debug.Assert(project != null, "project is null.");
            Debug.Assert(serviceProvider != null, "serviceProvider is null.");

            _serviceProvider = serviceProvider;
            _project = project;

            _language = VsUtils.GetLanguageForProject(project);
            Debug.Assert(_language != LangEnum.Unknown, "_language is Unknown.");
        }

        public IEnumerable<KeyValuePair<string, string>> Generate(DbModel model, string codeNamespace)
        {
            Debug.Assert(model != null, "model is null.");

            var container = model.ConceptualModel.Container;
            var extension = _language == LangEnum.VisualBasic
                ? FileExtensions.VbExt
                : FileExtensions.CsExt;

            var contextFileName = container.Name + extension;

            string contextFileContents;
            try
            {
                contextFileContents = GetContextGenerator().Generate(container, model, codeNamespace);
            }
            catch (Exception ex)
            {
                throw new CodeFirstModelGenerationException(
                    string.Format(Resources.ErrorGeneratingCodeFirstModel, contextFileName),
                    ex);
            }

            yield return new KeyValuePair<string, string>(contextFileName, contextFileContents);

            foreach (var entitySet in model.ConceptualModel.Container.EntitySets)
            {
                var entityTypeGenerator = GetEntityTypeGenerator();
                var entityTypeFileName = entitySet.ElementType.Name + extension;

                string entityTypeFileContents;
                try
                {
                    entityTypeFileContents = entityTypeGenerator.Generate(entitySet, model, codeNamespace);
                }
                catch (Exception ex)
                {
                    throw new CodeFirstModelGenerationException(
                        string.Format(Resources.ErrorGeneratingCodeFirstModel, entityTypeFileName),
                        ex);
                }

                yield return new KeyValuePair<string, string>(entityTypeFileName, entityTypeFileContents);
            }
        }

        private IContextGenerator GetContextGenerator()
        {
            if (_language == LangEnum.VisualBasic)
            {
                return (IContextGenerator)TryGetCustomizedTemplate(VBContextTemplatePath)
                    ?? new DefaultVBContextGenerator();
            }

            return (IContextGenerator)TryGetCustomizedTemplate(CSharpContextTemplatePath)
                    ?? new DefaultCSharpContextGenerator();
        }

        private IEntityTypeGenerator GetEntityTypeGenerator()
        {
            if (_language == LangEnum.VisualBasic)
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
                ? new CustomGenerator(_serviceProvider, _templatePathCache[path])
                : null;
        }
    }
}
