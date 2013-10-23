// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.TypeEditors
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing.Design;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.Util;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors;

    internal class FunctionImportReturnTypeTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (null == context)
            {
                Debug.Fail("context object must not be null");
                // returning value means "no change"
                return value;
            }

            if (null == context.Instance)
            {
                Debug.Fail("context Instance object should not be null");
                // returning value means "no change"
                return value;
            }

            if (null == provider)
            {
                Debug.Fail("provider object must not be null");
                // returning value means "no change"
                return value;
            }

            var funcImpDesc = context.Instance as EFFunctionImportDescriptor;
            if (null == funcImpDesc)
            {
                Debug.Fail(
                    "Context Instance object should be EFFunctionImportDescriptor, instead received object of type "
                    + context.Instance.GetType().FullName);
                // returning value means "no change"
                return value;
            }

            var funcImp = funcImpDesc.TypedEFElement;
            if (null == funcImp)
            {
                Debug.Fail("TypedEFElement is null for EFFunctionImportDescriptor object: " + funcImpDesc);
                // returning value means "no change"
                return value;
            }

            var artifact = funcImp.Artifact;
            if (null == artifact)
            {
                Debug.Fail("Null artifact in FunctionImportReturnTypeTypeEditor");
                // returning value means "no change"
                return value;
            }

            if (null == artifact.Uri)
            {
                Debug.Fail("Null artifact.Uri in FunctionImportReturnTypeTypeEditor");
                // returning value means "no change"
                return value;
            }

            if (null == artifact.Uri.LocalPath)
            {
                Debug.Fail("Null artifact.Uri.LocalPath in FunctionImportReturnTypeTypeEditor");
                // returning value means "no change"
                return value;
            }

            // get SchemaVersion for current edmx file
            if (null == artifact.SchemaVersion)
            {
                Debug.Fail("Could not determine Version for path " + artifact.Uri.LocalPath);
                // returning value means "no change"
                return value;
            }

            var cModel = artifact.ConceptualModel();
            if (null == cModel)
            {
                Debug.Fail("Null Conceptual Model in FunctionImportReturnTypeTypeEditor");
                // returning value means "no change"
                return value;
            }

            var sModel = artifact.StorageModel();
            var cContainer = cModel.FirstEntityContainer as ConceptualEntityContainer;
            // Since "value" parameter does not contain namespace information, so we could not differentiate between a complex type named "Decimal" and "Decimal" primitive type,
            // we need to get the normalized-return-type-string from function import and use it instead.
            var selectedElement = ModelHelper.FindComplexTypeEntityTypeOrPrimitiveTypeForFunctionImportReturnType(
                cModel, funcImp.ReturnTypeToNormalizedString);

            EntityDesignViewModelHelper.EditFunctionImport(
                funcImpDesc.EditingContext,
                funcImp,
                sModel,
                cModel,
                cContainer,
                selectedElement,
                EfiTransactionOriginator.ExplorerWindowOriginatorId);

            return value;
        }
    }
}
