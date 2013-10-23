// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using DslModeling = Microsoft.VisualStudio.Modeling;

namespace Microsoft.Data.Tools.Dsl.ModelTranslator
{
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    /// <summary>
    ///     Base class that is used to translate Data-Tools model to DSL model element.
    /// </summary>
    internal abstract class BaseTranslatorStrategy
    {
        protected EditingContext _editingContext;

        internal BaseTranslatorStrategy(EditingContext context)
        {
            _editingContext = context;
        }

        /// <summary>
        ///     Translate Model to DSL Model.
        /// </summary>
        /// <param name="modelElement"></param>
        /// <param name="partition"></param>
        /// <returns></returns>
        internal abstract DslModeling.ModelElement TranslateModelToDslModel(EFObject modelElement, DslModeling.Partition partition);

        /// <summary>
        ///     Synchronize DSL Model with the value from modelElement.
        /// </summary>
        /// <param name="parentViewModel"></param>
        /// <param name="modelElement"></param>
        /// <returns></returns>
        internal abstract DslModeling.ModelElement SynchronizeSingleDslModelElement(
            DslModeling.ModelElement parentViewModel, EFObject modelElement);

        internal EditingContext EditingContext
        {
            get { return _editingContext; }
        }
    }
}
