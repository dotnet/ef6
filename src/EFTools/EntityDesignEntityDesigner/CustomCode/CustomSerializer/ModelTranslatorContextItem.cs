// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.CustomSerializer
{
    using System;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Tools.Dsl.ModelTranslator;

    internal class ModelTranslatorContextItem : ContextItem
    {
        internal static ModelTranslator<BaseTranslatorStrategy> GetEntityModelTranslator(EditingContext context)
        {
            var translatorContextItem = context.Items.GetValue<ModelTranslatorContextItem>();
            if (translatorContextItem.Translator == null)
            {
                translatorContextItem.Translator =
                    new ModelTranslator<BaseTranslatorStrategy>(new EntityModelToDslModelTranslatorStrategy(context));
            }
            return translatorContextItem.Translator;
        }

        internal override Type ItemType
        {
            get { return typeof(ModelTranslatorContextItem); }
        }

        private ModelTranslator<BaseTranslatorStrategy> Translator { get; set; }
    }
}
