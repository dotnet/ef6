// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using DslModeling = Microsoft.VisualStudio.Modeling;

namespace Microsoft.Data.Tools.Dsl.ModelTranslator
{
    using Microsoft.Data.Entity.Design.Model;

    internal class ModelTranslator<T>
        where T : BaseTranslatorStrategy
    {
        protected T _translatorStrategy;

        internal ModelTranslator(T translatorStrategy)
        {
            _translatorStrategy = translatorStrategy;
        }

        internal DslModeling.ModelElement TranslateModelToDslModel(EFObject modelElement, DslModeling.Partition partition)
        {
            return _translatorStrategy.TranslateModelToDslModel(modelElement, partition);
        }

        internal DslModeling.ModelElement SynchronizeSingleDslModelElement(DslModeling.ModelElement parentViewModel, EFObject modelElement)
        {
            return _translatorStrategy.SynchronizeSingleDslModelElement(parentViewModel, modelElement);
        }
    }
}
