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
        private readonly LangEnum _language;
        private readonly CodeGeneratorFactory _codeGeneratorFactory;

        public CodeFirstModelGenerator(Project project)
        {
            Debug.Assert(project != null, "project is null.");

            _codeGeneratorFactory = new CodeGeneratorFactory(project);
            _language = VsUtils.GetLanguageForProject(project);
            Debug.Assert(_language != LangEnum.Unknown, "_language is Unknown.");
        }

        public IEnumerable<KeyValuePair<string, string>> Generate(DbModel model, string codeNamespace, string contextClassName, string connectionStringName)
        {
            var extension = _language == LangEnum.VisualBasic
                ? FileExtensions.VbExt
                : FileExtensions.CsExt;

            var contextFileName = contextClassName + extension;

            string contextFileContents;
            try
            {
                contextFileContents =
                    _codeGeneratorFactory
                        .GetContextGenerator(_language, isEmptyModel: model == null)
                        .Generate(model, codeNamespace, contextClassName, connectionStringName);
            }
            catch (Exception ex)
            {
                throw new CodeFirstModelGenerationException(
                    string.Format(Resources.ErrorGeneratingCodeFirstModel, contextFileName),
                    ex);
            }

            yield return new KeyValuePair<string, string>(contextFileName, contextFileContents);

            if (model != null)
            {
                foreach (var entitySet in model.ConceptualModel.Container.EntitySets)
                {
                    var entityTypeGenerator = _codeGeneratorFactory.GetEntityTypeGenerator(_language);
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
        }
    }
}
