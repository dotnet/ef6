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
        private readonly Func<IEnumerable<EdmStructuralMember>> getDeclaredMembers;

        internal EdmStructuralTypeMemberCollection(
            Func<IEnumerable<EdmStructuralMember>> allMembers, Func<IEnumerable<EdmStructuralMember>> declaredMembers)
        {
            getAllMembers = allMembers;
            getDeclaredMembers = declaredMembers;
        }

        internal EdmStructuralTypeMemberCollection(Func<IEnumerable<EdmStructuralMember>> declaredMembers)
            : this(declaredMembers, declaredMembers)
        {
        }

#if IncludeUnusedEdmCode
    /// <summary>
    /// Returns only the members that are declared by the structural type, without those members that are inherited from any base types.
    /// </summary>
        public IEnumerable<EdmStructuralMember> DeclaredOnly { get { return this.getDeclaredMembers(); } }
#endif

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
