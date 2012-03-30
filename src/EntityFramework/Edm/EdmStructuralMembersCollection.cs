namespace System.Data.Entity.Edm
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    ///     Enumerates all <see cref = "EdmStructuralMember" /> s declared or inherited by an <see cref = "EdmStructuralType" /> .
    /// </summary>
    internal sealed class EdmStructuralTypeMemberCollection : IEnumerable<EdmStructuralMember>
    {
        private readonly Func<IEnumerable<EdmStructuralMember>> getAllMembers;

        internal EdmStructuralTypeMemberCollection(
            Func<IEnumerable<EdmStructuralMember>> allMembers)
        {
            getAllMembers = allMembers;
        }

        public IEnumerator<EdmStructuralMember> GetEnumerator()
        {
            return getAllMembers().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return getAllMembers().GetEnumerator();
        }
    }
}
