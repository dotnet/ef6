// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Package
{
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;

    internal static class MicrosoftDataEntityDesignCommands
    {
        internal static readonly Guid menuGroupGuid = new Guid(Constants.MicrosoftDataEntityDesignCommandSetId);

        private const int cmdidViewExplorer = 0x0001;
        private const int cmdidViewMapping = 0x0002;

        private const int cmdIdAddEntity = 0x400;
        private const int cmdIdAddAssociation = 0x401;
        private const int cmdIdAddInheritance = 0x402;
        private const int cmdIdAddScalarProperty = 0x403;
        private const int cmdIdAddFunctionImport = 0x404;
        private const int cmdIdAddComplexProperty = 0x405;
        private const int cmdIdAddNavigationProperty = 0x406;

        private const int cmdIdRename = 0x410;
        private const int cmdIdCollapseEntityTypeShape = 0x411;
        private const int cmdIdExpandEntityTypeShape = 0x412;
        private const int cmdIdRefactorRename = 0x420;

        private const int cmdIdZoomIn = 0x500;
        private const int cmdIdZoomOut = 0x501;
        private const int cmdIdZoomToFit = 0x502;
        private const int cmdIdZoomCustom = 0x503;

        private const int cmdIdShowGrid = 0x600;
        private const int cmdIdSnapToGrid = 0x601;

        private const int cmdIdLayoutDiagram = 0x700;
        private const int cmdIdExportDiagramAsImage = 0x701;
        private const int cmdIdCollapseAllEntityTypeShapes = 0x703;
        private const int cmdIdExpandAllEntityTypeShapes = 0x704;

        private const int cmdIdDisplayName = 0x1000;
        private const int cmdIdDisplayNameAndType = 0x1001;

        private const int cmdIdShowMappingDesigner = 0x1100;
        private const int cmdIdShowEdmExplorer = 0x1101;
        private const int cmdIdTableMappings = 0x1106;
        private const int cmdIdShowInEdmExplorer = 0x1107;
        private const int cmdIdAssociationMappings = 0x1108;
        private const int cmdIdSprocMappings = 0x1109;
        private const int cmdIdShowInDiagram = 0x110A;

        private const int cmdIdValidate = 0x1110;

        private const int cmdIdAddNewTemplate = 0x1120;

        private const int cmdIdGenerateDB = 0x1104;
        private const int cmdIdGenerateDDL = 0x1105;

        private const int cmdIdSelectAll = 0x1200;

        private const int cmdIdEntityKey = 0x1400;

        private const int cmdIdSelectEntityEnd1 = 0x1500;
        private const int cmdIdSelectPropertyEnd1 = 0x1501;
        private const int cmdIdSelectEntityEnd2 = 0x1502;
        private const int cmdIdSelectPropertyEnd2 = 0x15003;
        private const int cmdIdSelectAssociation = 0x15004;
        private const int cmdIdSelectBaseType = 0x15005;
        private const int cmdIdSelectSubtype = 0x15006;

        private const int cmdIdRefreshFromDatabase = 0x1600;
        private const int cmdIdGenerateDatabaseScriptFromModel = 0x1601;
        private const int cmdIdCreateFunctionImport = 0x1700;
        private const int cmdIdAddComplexType = 0x1701;
        private const int cmdIdGoToDefinition = 0x1702;
        private const int cmdIdCreateComplexType = 0x1703;
        private const int cmdIdExplorerComplexTypes = 0x1704;
        private const int cmdIdFunctionImportMapping = 0x1705;
        private const int cmdIdAddNewDiagram = 0x1706;
        private const int cmdIdOpenDiagram = 0x1707;
        private const int cmdIdAddToDiagram = 0x1708;
        private const int cmdIdCloseDiagram = 0x1709;
        private const int cmdIdMoveToNewDiagram = 0x1711;
        private const int cmdIdRemoveFromDiagram = 0x1712;
        private const int cmdIdIncludeRelatedEntityType = 0x1713;
        private const int cmdIdMoveDiagramsToSeparateFile = 0x1714;
        private const int cmdIdMovePropertyUp = 0x1715;
        private const int cmdIdMovePropertyDown = 0x1716;
        private const int cmdIdMovePropertyUpMulti = 0x1717;
        private const int cmdIdMovePropertyDownMulti = 0x1718;
        private const int cmdIdMovePropertyToTop = 0x1719;
        private const int cmdIdMovePropertyToBottom = 0x1720;
        private const int cmdIdAddNewEnumType = 0x1721;
        private const int cmdIdConvertToEnum = 0x1722;

        private const int cmdidEdit = 0x1800;

        internal const int cmdIdLayerCommandsBase = 0x3000;
        internal const int cmdIdLayerRefactoringCommandsBase = 0x4000;

        public static readonly int[] ZoomLevels = { 10, 25, 33, 50, 66, 75, 100, 125, 150, 200, 300, 400 };

        private static readonly int[] ZoomIds =
            {
                0x0504, 0x0505, 0x0506, 0x0507, 0x0508, 0x0509, 0x050A, 0x050B, 0x050C, 0x050D, 0x050E,
                0x050F
            };

        public static CommandID[] CommandZoomLevels;

        public static readonly CommandID ViewExplorer = new CommandID(menuGroupGuid, cmdidViewExplorer);

        public static readonly CommandID ViewMapping = new CommandID(menuGroupGuid, cmdidViewMapping);

        public static readonly CommandID LayerCommandsBase = new CommandID(menuGroupGuid, cmdIdLayerCommandsBase);

        public static readonly CommandID LayerRefactoringCommandsBase = new CommandID(menuGroupGuid, cmdIdLayerRefactoringCommandsBase);

        public static readonly CommandID Edit = new CommandID(menuGroupGuid, cmdidEdit);

        public static readonly CommandID AddEntity = new CommandID(menuGroupGuid, cmdIdAddEntity);

        public static readonly CommandID AddScalarProperty = new CommandID(menuGroupGuid, cmdIdAddScalarProperty);

        public static readonly CommandID AddComplexProperty = new CommandID(menuGroupGuid, cmdIdAddComplexProperty);

        public static readonly CommandID AddAssociation = new CommandID(menuGroupGuid, cmdIdAddAssociation);

        public static readonly CommandID AddInheritance = new CommandID(menuGroupGuid, cmdIdAddInheritance);

        public static readonly CommandID AddFunctionImport = new CommandID(menuGroupGuid, cmdIdAddFunctionImport);

        public static readonly CommandID AddNavigationProperty = new CommandID(menuGroupGuid, cmdIdAddNavigationProperty);

        public static readonly CommandID Rename = new CommandID(menuGroupGuid, cmdIdRename);

        public static readonly CommandID RefactorRename = new CommandID(menuGroupGuid, cmdIdRefactorRename);

        public static readonly CommandID CollapseEntityTypeShape = new CommandID(menuGroupGuid, cmdIdCollapseEntityTypeShape);

        public static readonly CommandID ExpandEntityTypeShape = new CommandID(menuGroupGuid, cmdIdExpandEntityTypeShape);

        public static readonly CommandID ZoomIn = new CommandID(menuGroupGuid, cmdIdZoomIn);

        public static readonly CommandID ZoomOut = new CommandID(menuGroupGuid, cmdIdZoomOut);

        public static readonly CommandID ZoomToFit = new CommandID(menuGroupGuid, cmdIdZoomToFit);

        public static readonly CommandID ZoomCustom = new CommandID(menuGroupGuid, cmdIdZoomCustom);

        public static readonly CommandID ShowGrid = new CommandID(menuGroupGuid, cmdIdShowGrid);

        public static readonly CommandID SnapToGrid = new CommandID(menuGroupGuid, cmdIdSnapToGrid);

        public static readonly CommandID LayoutDiagram = new CommandID(menuGroupGuid, cmdIdLayoutDiagram);

        public static readonly CommandID ExportDiagramAsImage = new CommandID(menuGroupGuid, cmdIdExportDiagramAsImage);

        public static readonly CommandID CollapseAllEntityTypeShapes = new CommandID(menuGroupGuid, cmdIdCollapseAllEntityTypeShapes);

        public static readonly CommandID ExpandAllEntityTypeShapes = new CommandID(menuGroupGuid, cmdIdExpandAllEntityTypeShapes);

        public static readonly CommandID DisplayName = new CommandID(menuGroupGuid, cmdIdDisplayName);

        public static readonly CommandID DisplayNameAndType = new CommandID(menuGroupGuid, cmdIdDisplayNameAndType);

        public static readonly CommandID ShowMappingDesigner = new CommandID(menuGroupGuid, cmdIdShowMappingDesigner);

        public static readonly CommandID ShowEdmExplorer = new CommandID(menuGroupGuid, cmdIdShowEdmExplorer);

        public static readonly CommandID TableMappings = new CommandID(menuGroupGuid, cmdIdTableMappings);

        public static readonly CommandID AssociationMappings = new CommandID(menuGroupGuid, cmdIdAssociationMappings);

        public static readonly CommandID SprocMappings = new CommandID(menuGroupGuid, cmdIdSprocMappings);

        public static readonly CommandID ShowInEdmExplorer = new CommandID(menuGroupGuid, cmdIdShowInEdmExplorer);

        public static readonly CommandID ShowInDiagram = new CommandID(menuGroupGuid, cmdIdShowInDiagram);

        public static readonly CommandID FunctionImportMapping = new CommandID(menuGroupGuid, cmdIdFunctionImportMapping);

        public static readonly CommandID GenerateDB = new CommandID(menuGroupGuid, cmdIdGenerateDB);

        public static readonly CommandID GenerateDDL = new CommandID(menuGroupGuid, cmdIdGenerateDDL);

        public static readonly CommandID Validate = new CommandID(menuGroupGuid, cmdIdValidate);

        public static readonly CommandID SelectAll = new CommandID(menuGroupGuid, cmdIdSelectAll);

        public static readonly CommandID EntityKey = new CommandID(menuGroupGuid, cmdIdEntityKey);

        public static readonly CommandID SelectEntityEnd1 = new CommandID(menuGroupGuid, cmdIdSelectEntityEnd1);

        public static readonly CommandID SelectPropertyEnd1 = new CommandID(menuGroupGuid, cmdIdSelectPropertyEnd1);

        public static readonly CommandID SelectEntityEnd2 = new CommandID(menuGroupGuid, cmdIdSelectEntityEnd2);

        public static readonly CommandID SelectPropertyEnd2 = new CommandID(menuGroupGuid, cmdIdSelectPropertyEnd2);

        public static readonly CommandID SelectAssociation = new CommandID(menuGroupGuid, cmdIdSelectAssociation);

        public static readonly CommandID SelectBaseType = new CommandID(menuGroupGuid, cmdIdSelectBaseType);

        public static readonly CommandID SelectSubtype = new CommandID(menuGroupGuid, cmdIdSelectSubtype);

        public static readonly CommandID RefreshFromDatabase = new CommandID(menuGroupGuid, cmdIdRefreshFromDatabase);

        public static readonly CommandID GenerateDatabaseScriptFromModel = new CommandID(
            menuGroupGuid, cmdIdGenerateDatabaseScriptFromModel);

        public static readonly CommandID CreateFunctionImport = new CommandID(menuGroupGuid, cmdIdCreateFunctionImport);

        public static readonly CommandID AddComplexType = new CommandID(menuGroupGuid, cmdIdAddComplexType);

        public static readonly CommandID GoToDefinition = new CommandID(menuGroupGuid, cmdIdGoToDefinition);

        public static readonly CommandID CreateComplexType = new CommandID(menuGroupGuid, cmdIdCreateComplexType);

        public static readonly CommandID ExplorerComplexTypes = new CommandID(menuGroupGuid, cmdIdExplorerComplexTypes);

        public static readonly CommandID AddNewTemplate = new CommandID(menuGroupGuid, cmdIdAddNewTemplate);

        public static readonly CommandID AddNewDiagram = new CommandID(menuGroupGuid, cmdIdAddNewDiagram);

        public static readonly CommandID OpenDiagram = new CommandID(menuGroupGuid, cmdIdOpenDiagram);

        public static readonly CommandID AddToDiagram = new CommandID(menuGroupGuid, cmdIdAddToDiagram);

        public static readonly CommandID CloseDiagram = new CommandID(menuGroupGuid, cmdIdCloseDiagram);

        public static readonly CommandID MoveToNewDiagram = new CommandID(menuGroupGuid, cmdIdMoveToNewDiagram);

        public static readonly CommandID RemoveFromDiagram = new CommandID(menuGroupGuid, cmdIdRemoveFromDiagram);

        public static readonly CommandID IncludeRelatedEntityType = new CommandID(menuGroupGuid, cmdIdIncludeRelatedEntityType);

        public static readonly CommandID MoveDiagramsToSeparateFile = new CommandID(menuGroupGuid, cmdIdMoveDiagramsToSeparateFile);

        public static readonly CommandID MovePropertyUp = new CommandID(menuGroupGuid, cmdIdMovePropertyUp);

        public static readonly CommandID MovePropertyDown = new CommandID(menuGroupGuid, cmdIdMovePropertyDown);

        public static readonly CommandID MovePropertyPageUp = new CommandID(menuGroupGuid, cmdIdMovePropertyUpMulti);

        public static readonly CommandID MovePropertyPageDown = new CommandID(menuGroupGuid, cmdIdMovePropertyDownMulti);

        public static readonly CommandID MovePropertyTop = new CommandID(menuGroupGuid, cmdIdMovePropertyToTop);

        public static readonly CommandID MovePropertyBottom = new CommandID(menuGroupGuid, cmdIdMovePropertyToBottom);

        public static readonly CommandID AddEnumType = new CommandID(menuGroupGuid, cmdIdAddNewEnumType);

        public static readonly CommandID ConvertToEnum = new CommandID(menuGroupGuid, cmdIdConvertToEnum);

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static MicrosoftDataEntityDesignCommands()
        {
            CommandZoomLevels = new CommandID[ZoomIds.Length];
            for (var i = 0; i < ZoomIds.Length; i++)
            {
                CommandZoomLevels[i] = new CommandID(menuGroupGuid, ZoomIds[i]);
            }
        }
    }
}
