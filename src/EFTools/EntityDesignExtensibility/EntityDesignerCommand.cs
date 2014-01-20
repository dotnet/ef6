// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Diagnostics;
    using System.Xml.Linq;

    internal class EntityDesignerCommand
    {
        private readonly ExecuteAction _execute;
        private readonly CanExecuteFunction _canExecute;

        internal delegate void ExecuteAction(
            XObject selectedXElement, object docView, object singleViewModelSelection, object propertiesCompartment, bool isSingleSelection);

        internal delegate Tuple<bool, bool> CanExecuteFunction(
            XObject selectedXElement, object docView, object singleViewModelSelection, object propertiesCompartment, bool isSingleSelection);

        internal string Name { get; set; }

        internal bool IsRefactoringCommand { get; set; }

        // <summary>
        //     Creates an EntityDesignerCommand which can be executed from a context menu in the designer
        // </summary>
        // <param name="name">The name of the command, and the label text that will appear in the designer's context menu</param>
        // <param name="executeAction">A simple Action that will get passed in the EntityDesignerSelection and the selected XElement</param>
        // <param name="canExecuteFunction">Delegate which accepts an EntityDesignerSelection and will return a Tuple where the first boolean corresponds to whether the command is shown, and the second corresponds to whether the command is enabled</param>
        // <param name="isRefactoringCommand">Specifies whether this command is a refactoring operation, in which case it may be placed separately in the resulting context menu</param>
        internal EntityDesignerCommand(
            string name,
            ExecuteAction executeAction,
            CanExecuteFunction canExecuteFunction = null,
            bool isRefactoringCommand = false)
        {
            if (name == null)
            {
                throw new ArgumentException("name should not be null");
            }
            if (executeAction == null)
            {
                throw new ArgumentException("executeAction should not be null");
            }

            Name = name;
            _execute = executeAction;
            _canExecute = canExecuteFunction;
            IsRefactoringCommand = isRefactoringCommand;
        }

        // <summary>
        //     Delegate which is called when determining whether to show or enable this command for a particular selection
        // </summary>
        // <param name="selectedElement">The XML object corresponding to the selection</param>
        // <param name="docView">The document view corresponding to the selection</param>
        // <param name="singleViewModelSelection">The selection in the view model</param>
        // <param name="propertiesCompartment">If this selection is within an EntityType, this specifies the Properties compartment within that EntityType</param>
        // <param name="isSingleSelection">Specifies whether the selection is over multiple objects or not</param>
        // <returns>a Tuple where the first boolean corresponds to whether the command is shown, and the second corresponds to whether the command is enabled</returns>
        [DebuggerStepThrough]
        internal Tuple<bool, bool> CanExecute(
            XObject selectedElement, object docView, object singleViewModelSelection, object propertiesCompartment, bool isSingleSelection)
        {
            return _canExecute == null
                       ? new Tuple<bool, bool>(true, true)
                       : _canExecute(selectedElement, docView, singleViewModelSelection, propertiesCompartment, isSingleSelection);
        }

        // <summary>
        //     The execution logic of the command
        // </summary>
        // <param name="selectedElement">The XElement corresponding to the selection in the Entity Designer</param>
        // <param name="docView">The document view corresponding to this selection</param>
        // <param name="singleViewModelSelection">The selection in the view model</param>
        // <param name="propertiesCompartment">If this selection is within an EntityType, this specifies the Properties compartment within that EntityType</param>
        // <param name="isSingleSelection">Specifies whether the selection is over multiple objects</param>
        internal void Execute(
            XObject selectedElement, object docView, object singleViewModelSelection, object propertiesCompartment, bool isSingleSelection)
        {
            _execute(selectedElement, docView, singleViewModelSelection, propertiesCompartment, isSingleSelection);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var otherCommand = obj as EntityDesignerCommand;
            if (otherCommand == null)
            {
                return false;
            }

            // TODO support layer discrimination as well
            return Name == otherCommand.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
