// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Refactoring
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.UI.Views.Dialogs;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Common;
    using Microsoft.VisualStudio.OLE.Interop;

    internal class RefactorEFObject
    {
        internal static void RefactorRenameElement(EFObject objectToRefactor, string newName = null, bool showPreview = true)
        {
            if (objectToRefactor != null
                && objectToRefactor.Artifact != null)
            {
                var namedObject = objectToRefactor as EFNormalizableItem;
                if (namedObject != null)
                {
                    // If the API call did not supply a new name for the object, bring up the dialog for the user to input a name
                    if (newName == null)
                    {
                        var dialog = new RefactorRenameDialog(namedObject);
                        if (dialog.ShowModal() == true)
                        {
                            newName = dialog.NewName;

                            if (dialog.ShowPreview.HasValue)
                            {
                                showPreview = dialog.ShowPreview.Value;
                            }
                        }
                    }

                    if (newName != null)
                    {
                        RefactorRenameElementInDesignerOnly(namedObject, newName, showPreview);
                    }
                }
            }
        }

        private static void RefactorRenameElementInDesignerOnly(EFNormalizableItem namedObject, string newName, bool showPreview)
        {
            Debug.Assert(namedObject != null, "namedObject != null");
            Debug.Assert(newName != null, "namedObject != newName");

            var input = new EFRenameContributorInput(namedObject, newName, namedObject.Name.Value);
            var refactoringOperation = new EFRefactoringOperation(
                namedObject,
                newName,
                input,
                new ServiceProviderHelper(PackageManager.Package.GetService(typeof(IServiceProvider)) as IServiceProvider));

            refactoringOperation.HasPreviewWindow = showPreview;
            refactoringOperation.DoOperation();
        }
    }
}
