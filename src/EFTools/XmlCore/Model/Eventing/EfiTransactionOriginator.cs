// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Eventing
{
    /// <summary>
    ///     This class stores the Originator Ids for various parts of the designer, those parts that can initiate transactions.  These strings
    ///     are not visible to the user.
    /// </summary>
    internal class EfiTransactionOriginator
    {
        internal static string MappingDetailsOriginatorId
        {
            get { return "MappingDetails"; }
        }

        internal static string PropertyWindowOriginatorId
        {
            get { return "PropertyWindow"; }
        }

        internal static string ExplorerWindowOriginatorId
        {
            get { return "ExplorerWindow"; }
        }

        internal static string EntityDesignerOriginatorId
        {
            get { return "EntityDesigner"; }
        }

        internal static string XmlEditorOriginatorId
        {
            get { return "XmlEditor"; }
        }

        internal static string SchemaTranslatorId
        {
            get { return "SchemaTranslator"; }
        }

        internal static string UpdateModelFromDatabaseId
        {
            get { return "UpdateModelFromDatabase"; }
        }

        internal static string GenerateDatabaseScriptFromModelId
        {
            get { return "GenerateDatabaseScriptFromModel"; }
        }

        internal static string AddNewArtifactGenerationItemId
        {
            get { return "AddNewArtifactGenerationItem"; }
        }

        internal static string UndoRedoOriginatorId
        {
            get { return "UndoRedo"; }
        }

        internal static string TransactionOriginatorDiagramId
        {
            get { return "DesignerViewDiagramId"; }
        }

        internal static string CreateNewModelId
        {
            get { return "CreateNewModel"; }
        }

        internal static string ByReferenceOriginatorId
        {
            get { return "ByReference"; }
        }
    }
}
