// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.VSXmlDesignerBase.Refactoring
{
    /// <summary>
    ///     The ContributorInput class represents the data input to each of the contributors.
    ///     For each contributor type, a derived ContributorInput class will be created.
    ///     ex) SymbolChangeContributorInput, SymbolReferenceChangeContributorInput
    ///     We will have RefactorOperation on each ContributorInput.
    ///     The RefactorOperation class will set this property when it creates the initial ContributorInput.
    ///     Then the RefactoringManager will be reponsible to pass this property to any side effect ContributorInput.
    /// </summary>
    internal abstract class ContributorInput
    {
        /// <summary>
        ///     Compare if two ContributorInput objects are equal
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract override bool Equals(object obj);

        /// <summary>
        ///     Returns hash code for this object
        /// </summary>
        public abstract override int GetHashCode();
    }
}
